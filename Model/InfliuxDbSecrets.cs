using MqttRelay.Exceptions;

namespace MqttRelay.Model;

public class InfluxDbSecrets
{
    public InfluxDbSecrets()
    {
        var influxDbTokenEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_TOKEN")!;
        if (influxDbTokenEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbTokenEnvironmentVariable));
        Token = influxDbTokenEnvironmentVariable;

        var influxDbAddressEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_ADDRESS")!;
        if (influxDbAddressEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbAddressEnvironmentVariable));
        Address = influxDbAddressEnvironmentVariable;

        var influxDbBucketEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_BUCKET")!;
        if (influxDbBucketEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbBucketEnvironmentVariable));
        Bucket = influxDbBucketEnvironmentVariable;

        var influxDbOrganizationEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_ORGANIZATION")!;
        if (influxDbOrganizationEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbOrganizationEnvironmentVariable));
        Organization = influxDbOrganizationEnvironmentVariable;
    }


    // Getters
    public string Address { get; }

    public string Token { get; }

    public string Bucket { get; }

    public string Organization { get; }
}