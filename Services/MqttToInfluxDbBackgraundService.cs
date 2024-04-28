using System.Text;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MQTTnet;
using MQTTnet.Client;
using MqttRelay.Exceptions;
using MqttRelay.Model;
using Newtonsoft.Json.Linq;

namespace MqttRelay.Services;

public class MqttToInfluxDbBackgroundService : IHostedService, IDisposable
{

    private  readonly MqttSecrets _mqttSecrets;
    private  readonly InfluxDbSecrets _influxDbSecrets;
    
    
    private  readonly ILogger<MqttToInfluxDbBackgroundService> _logger;
    private  readonly IMqttClient _mqttClient;
    private  readonly InfluxDBClient _influxDbClient;
    private  readonly MqttClientOptions _mqttClientOptions;
    // https://www.linkedin.com/pulse/using-influxdb-c-amir-doosti-dbklf/

    public MqttToInfluxDbBackgroundService(ILogger<MqttToInfluxDbBackgroundService> logger)
    {
        _logger = logger;

        if (_mqttSecrets == null)
        {
            _mqttSecrets = new MqttSecrets();
        }

        if (_influxDbSecrets == null)
        {
            _influxDbSecrets = new InfluxDbSecrets();
        }

        if (_mqttClient == null)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
        }
        
        if (_mqttClientOptions == null)
        {
            
            _mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(_mqttSecrets.Address, _mqttSecrets.Port).WithClientId(_mqttSecrets.ClientId)
            .WithCredentials(_mqttSecrets.Username, _mqttSecrets.Password).Build();
        }


        if (_influxDbClient == null)
        {
            _influxDbClient = new InfluxDBClient(_influxDbSecrets.Address, _influxDbSecrets.Token);
        }


        
    }



    private Task StartAsync2(CancellationToken cancellationToken)
    {

        _mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            
            string topic = e.ApplicationMessage.Topic;
            string payload = e.ApplicationMessage.ConvertPayloadToString();


            // trying to escape blobs 
            if (topic.Contains("snapshot"))
            {
                return Task.CompletedTask;
            }

            // trying to parse it as a json
            JObject? json = null;
            try
            {
                json = JObject.Parse(payload);
            }
            catch (Exception)
            {
                // non critical exception
            }


            // try to parse it as float

            double parsedDouble = Double.NaN;

            try
            {
                parsedDouble = double.Parse(payload);
            }
            catch (Exception)
            {
                // non critical exception
            }

            PointData? point;

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


                _logger.LogDebug("The string value of the below is: \n : {StringValue}", payload);

                // we supply the value after the statment aka segregation of values 
                _logger.LogDebug("The json value is: \n {JsonValue}", lineStringBuilder.ToString());


                _influxDbClient.GetWriteApiAsync().WriteRecordAsync(record: lineStringBuilder.ToString(),
                    bucket: _influxDbSecrets.Bucket, org: _influxDbSecrets.Organization, cancellationToken: cancellationToken);
            }
            else
            {
                point = null;
                _logger.LogDebug("This topic is string: {Topic} with message", payload);
            }

            if (point != null)
            {
                _influxDbClient.GetWriteApiAsync().WritePointAsync(point: point, bucket:  _influxDbSecrets.Bucket,
                    org:  _influxDbSecrets.Organization, cancellationToken: cancellationToken);
                GC.Collect();
            }

            GC.Collect();
            return Task.CompletedTask;
        };
        GC.Collect();
        return Task.CompletedTask;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.ConnectAsync(_mqttClientOptions, cancellationToken);
        /*
         * This sample shows how to reconnect when the connection was dropped.
         * This approach uses a custom Task/Thread which will monitor the connection status.
         * This is the recommended way but requires more custom code!
         */
        _ = Task.Run(
            async () =>
            {
                // // User proper cancellation and no while(true).
                while (true)
                {
                    try
                    {
                        // This code will also do the very first connect! So no call to _ConnectAsync_ is required in the first place.
                        if (!await _mqttClient.TryPingAsync(cancellationToken))
                        {
                            await _mqttClient.ConnectAsync(_mqttClient.Options, cancellationToken);
                            // Subscribe to topics when session is clean etc.
                            _logger.LogInformation("The MQTT client is connected");
                        }

                        var topicFilter = new MqttTopicFilterBuilder().WithTopic(_mqttSecrets.Topic).Build();
                        await _mqttClient.SubscribeAsync(
                            new MqttClientSubscribeOptionsBuilder().WithTopicFilter(topicFilter).Build(),
                            cancellationToken);

                        await StartAsync2(cancellationToken);
                        GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception properly (logging etc.).
                        _logger.LogError(ex, "The MQTT client  connection failed");
                        GC.Collect();
                    }
                    finally
                    {
                        // Check the connection state every 5 seconds and perform a reconnect if required.
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        GC.Collect();
                    }
                    GC.Collect();
                }
                
            });
        GC.Collect();
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Code to execute when the application stops
        // Console.WriteLine("Application stopping. Performing cleanup tasks...");
        Dispose();
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
                // Console.WriteLine("This is an array: " + property.Value.ToString());
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

    public void Dispose()
    {
        _logger.LogDebug("Disposing");
        _mqttClient.Dispose();
        _influxDbClient.Dispose();
        GC.SuppressFinalize(this);
        _logger.LogDebug("Disposed");
        // _mqttClient = null;
        // _influxDbClient = null;
        // _mqttClientOptions = null;
        // _logger = null;
        // _mqttSecrets = null;
        // _influxDbSecrets = null;

        
        GC.Collect();
        
    }
}