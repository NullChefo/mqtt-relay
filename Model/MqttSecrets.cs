using MqttRelay.Exceptions;

namespace MqttRelay.Model;

public class MqttSecrets
{
    private readonly string address;
    private int port;
    private string clientId;
    private string username;
    private string password;
    private string topic;


    public string Address => address;
    public int Port => port;
    public string ClientId => clientId;
    public string Username => username;
    public string Password => password;
    public string Topic => topic;



    public MqttSecrets()
    {
        string addressEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_ADDRESS")!;
        if (addressEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(addressEnvironmentVariable));
        else
        {
            address = addressEnvironmentVariable;
        }

        string portEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_PORT")!;
        if (portEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(portEnvironmentVariable));
        else
        {
            //try to parse first if not int throw exception
            if (!int.TryParse(portEnvironmentVariable, out _))
            {
                throw new EnvironmentVariableNotConfigured(nameof(portEnvironmentVariable));
            }

            port = int.Parse(portEnvironmentVariable);
        }

        string clientIdEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_CLIENT_ID")!;
        if (clientIdEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(clientIdEnvironmentVariable));
        else
        {
            clientId = clientIdEnvironmentVariable;
        }

        string usernameEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_USERNAME")!;
        if (usernameEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(usernameEnvironmentVariable));
        else
        {
            username = usernameEnvironmentVariable;
        }

        string passwordEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_PASSWORD")!;
        if (passwordEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(passwordEnvironmentVariable));
        else
        {
            password = passwordEnvironmentVariable;
        }

        string topicEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_TOPIC")!;
        if (topicEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(topicEnvironmentVariable));
        else
        {
            topic = topicEnvironmentVariable;
        }


    }
    
    
    


}
