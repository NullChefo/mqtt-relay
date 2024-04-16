using System.Text;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

using MQTTnet;
using MQTTnet.Client;
using MqttRelay.Exceptions;
using Newtonsoft.Json.Linq;

namespace MqttRelay.Services;

public class MqttToInfluxDbBackgroundService : IHostedService
{
    private readonly string _address;
    private readonly int _port ;
    private readonly string _clientId;
    private readonly string _username;
    private readonly string _password;
    private readonly string _topic;
    
    private readonly string _influxDbAddress;
    private readonly string _influxDbToken;
    private readonly string _influxDbBucket;
    private readonly string _influxDbOrganization;
    
    private readonly  ILogger<MqttToInfluxDbBackgroundService> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly InfluxDBClient  _influxDbClient;
    private readonly MqttClientOptions _mqttClientOptions;

    // https://www.linkedin.com/pulse/using-influxdb-c-amir-doosti-dbklf/
    
    public  MqttToInfluxDbBackgroundService(ILogger<MqttToInfluxDbBackgroundService> logger)
    {
        _logger = logger;
        
        var influxDbTokenEnvironmentVariable= Environment.GetEnvironmentVariable("INFLUX_TOKEN")!;
        if (influxDbTokenEnvironmentVariable == "")
        {
            throw new EnvironmentVariableNotConfigured(nameof(influxDbTokenEnvironmentVariable));
        }
        else
        {
            _influxDbToken = influxDbTokenEnvironmentVariable;
        }
        var influxDbAddressEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_ADDRESS")!;
        if (influxDbAddressEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbAddressEnvironmentVariable));
        else
        {
            _influxDbAddress = influxDbAddressEnvironmentVariable;
        }
        var influxDbBucketEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_BUCKET")!;
        if (influxDbBucketEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbBucketEnvironmentVariable));
        else
        {
            _influxDbBucket = influxDbBucketEnvironmentVariable;
        }
        var influxDbOrganizationEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_ORGANIZATION")!;
        if (influxDbOrganizationEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbOrganizationEnvironmentVariable));
        else
        {
            _influxDbOrganization = influxDbOrganizationEnvironmentVariable;
        }
        var addressEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_ADDRESS")!;
        if (addressEnvironmentVariable== "")
            throw new EnvironmentVariableNotConfigured(nameof(addressEnvironmentVariable));
        else
        {
            _address = addressEnvironmentVariable;
        }
        var portEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_PORT")!;
        if (portEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(portEnvironmentVariable));
        else
        {
            //try to parse first if not int throw exception
            if (!int.TryParse(portEnvironmentVariable, out _))
            {
                throw new EnvironmentVariableNotConfigured(nameof(portEnvironmentVariable));
            }
            _port = int.Parse(portEnvironmentVariable);
        }
        var clientIdEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_CLIENT_ID")!;
        if (clientIdEnvironmentVariable == "" )
            throw new EnvironmentVariableNotConfigured(nameof(clientIdEnvironmentVariable));
        else
        {
            _clientId = clientIdEnvironmentVariable;
        }
        var usernameEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_USERNAME")!;
        if (usernameEnvironmentVariable== "" )
            throw new EnvironmentVariableNotConfigured(nameof(usernameEnvironmentVariable));
        else
        {
            _username = usernameEnvironmentVariable;
        }
        var passwordEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_PASSWORD")!;
        if (passwordEnvironmentVariable== "")
            throw new EnvironmentVariableNotConfigured(nameof(passwordEnvironmentVariable));
        else
        {
            _password = passwordEnvironmentVariable;
        }
        var topicEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_TOPIC")!;
        if (topicEnvironmentVariable== "")
            throw new EnvironmentVariableNotConfigured(nameof(topicEnvironmentVariable));
        else
        {
            _topic = topicEnvironmentVariable;
        }
        


     
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
        _mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_address, _port).WithClientId(_clientId)
            .WithCredentials(_username, _password).Build();
         _influxDbClient = new InfluxDBClient(_influxDbAddress, _influxDbToken);
    }

    private void MqttConnect()
    {
        _mqttClient.ConnectAsync(_mqttClientOptions).ContinueWith(async e =>
        {
            _logger.LogInformation("Connected to MQTT broker");
            var topicFilter = new MqttTopicFilterBuilder().WithTopic(_topic).Build();
            await _mqttClient.SubscribeAsync(
                new MqttClientSubscribeOptionsBuilder().WithTopicFilter(topicFilter).Build());
        });
    }


    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO FIX
        MqttConnect();

        _influxDbClient.PingAsync();
        
        _mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            var stringValue = e.ApplicationMessage.ConvertPayloadToString();

            var topic = e.ApplicationMessage.Topic;


            // trying to escape blobs 
            if (topic.Contains("snapshot"))
            {
                return Task.CompletedTask;
            }

            // trying to parse it as a json
            JObject json = null;
            try
            {
                json = JObject.Parse(stringValue);
            }
            catch (Exception exception)
            {
                // non critical exception
            }


            // try to parse it as float

            double parsedDouble = Double.NaN;

            try
            {
                parsedDouble = double.Parse(stringValue);
            }
            catch (Exception exception)
            {
                // non critical exception
            }

            PointData point;

            if (!Double.IsNaN(parsedDouble))
            {
                point = PointData
                    .Measurement(topic)
                    .Field("value", parsedDouble)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns)
                    .Tag("topic", topic);
            }

            // trying to use the InfluxDB Line Protocol
            else if (json != null)
            {

                point = null;

                // const string line = "temperature,city=Paris temperature=25.15, {timestamp}";
                StringBuilder lineStringBuilder = new StringBuilder();
                lineStringBuilder.Append(topic).Append(',').Append("topic=")
                    .Append(topic).Append(' ');

                lineStringBuilder.Append(ProcessJson(json));

                lineStringBuilder.Append(' ');

                DateTime currentTime = DateTime.Now;

                lineStringBuilder.Append(((DateTimeOffset)currentTime).ToUnixTimeSeconds());


                _logger.LogInformation("The string value of the below is: \n : {StringValue}", stringValue);

                // we supply the value after the statment aka segregation of values 
                _logger.LogInformation("The json value is: \n {JsonValue}", lineStringBuilder.ToString());



                _influxDbClient.GetWriteApiAsync().WriteRecordAsync(record: lineStringBuilder.ToString(),
                    bucket: _influxDbBucket, org: _influxDbBucket, cancellationToken: cancellationToken);
            }
            else
            {
                point = null;

                _logger.LogInformation("This topic is string: {Topic} with message", stringValue);
            }

            if (point != null)
            {
                _influxDbClient.GetWriteApiAsync().WritePointAsync(point: point, bucket: _influxDbBucket,
                    org: _influxDbOrganization, cancellationToken: cancellationToken);

            }

            // Console.WriteLine(e.ApplicationMessage.ConvertPayloadToString());

            return Task.CompletedTask;
        };

        return Task.CompletedTask;
    }
    
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Code to execute when the application stops
        Console.WriteLine("Application stopping. Performing cleanup tasks...");
        return Task.CompletedTask;
    }

    // Assuming json is a JObject containing the nested JSON structure
    private static String ProcessJson(JObject json)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var property in json.Properties())
        {
            if (property.Value.Type == JTokenType.Object)
            {
                // If the property value is an object, recursively process it
                ProcessJson((JObject)property.Value);
            }
            else if (property.Value.Type == JTokenType.Array)
            {
                
                Console.WriteLine("This is an array: "  + property.Value.ToString());
            }
            else
            {
                // Otherwise, it's a leaf node, so process it
                if (property.Value.ToString() == "")
                {
                    sb.Append(property.Path).Append('=').Append("null").Append(',');
                }
                else
                {
                    sb.Append(property.Path).Append('=').Append(property.Value).Append(',');
                }
            }
        }

        if (sb.Length > 1)
        {
            sb.Length -= 1; // Remove the last comma 
        }

        return sb.ToString();
    }
}

