using MqttRelay.Exceptions;

namespace MqttRelay.Model;

public class MqttSecrets
{
    public MqttSecrets()
    {
        var addressEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_ADDRESS")!;
        if (addressEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(addressEnvironmentVariable));
        Address = addressEnvironmentVariable;

        var portEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_PORT")!;
        if (portEnvironmentVariable == "") throw new EnvironmentVariableNotConfigured(nameof(portEnvironmentVariable));

        //try to parse first if not int throw exception
        if (!int.TryParse(portEnvironmentVariable, out _))
            throw new EnvironmentVariableNotConfigured(nameof(portEnvironmentVariable));

        Port = int.Parse(portEnvironmentVariable);

        var clientIdEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_CLIENT_ID")!;
        if (clientIdEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(clientIdEnvironmentVariable));
        ClientId = clientIdEnvironmentVariable;

        var usernameEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_USERNAME")!;
        if (usernameEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(usernameEnvironmentVariable));
        Username = usernameEnvironmentVariable;

        var passwordEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_PASSWORD")!;
        if (passwordEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(passwordEnvironmentVariable));
        Password = passwordEnvironmentVariable;

        var topicEnvironmentVariable = Environment.GetEnvironmentVariable("MQTT_TOPIC")!;
        if (topicEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(topicEnvironmentVariable));
        Topic = topicEnvironmentVariable;
    }


    public string Address { get; }

    public int Port { get; }

    public string ClientId { get; }

    public string Username { get; }

    public string Password { get; }

    public string Topic { get; }
}