using MqttRelay.Exceptions;

namespace MqttRelay.Model;

public class InfluxDbSecrets
{
    private string influxDbAddress;
    private string influxDbToken;
    private string influxDbBucket;
    private string influxDbOrganization;



    // Getters
    public string Address => influxDbAddress;
    public string Token => influxDbToken;
    public string Bucket => influxDbBucket;
    public string Organization => influxDbOrganization;




    public InfluxDbSecrets()
    {
        string influxDbTokenEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_TOKEN")!;
        if (influxDbTokenEnvironmentVariable == "")
        {
            throw new EnvironmentVariableNotConfigured(nameof(influxDbTokenEnvironmentVariable));
        }
        else
        {
            influxDbToken = influxDbTokenEnvironmentVariable;
        }

        string influxDbAddressEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_ADDRESS")!;
        if (influxDbAddressEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbAddressEnvironmentVariable));
        else
        {
            influxDbAddress = influxDbAddressEnvironmentVariable;
        }

        string influxDbBucketEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_BUCKET")!;
        if (influxDbBucketEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbBucketEnvironmentVariable));
        else
        {
            influxDbBucket = influxDbBucketEnvironmentVariable;
        }

        string influxDbOrganizationEnvironmentVariable = Environment.GetEnvironmentVariable("INFLUX_ORGANIZATION")!;
        if (influxDbOrganizationEnvironmentVariable == "")
            throw new EnvironmentVariableNotConfigured(nameof(influxDbOrganizationEnvironmentVariable));
        else
        {
            influxDbOrganization = influxDbOrganizationEnvironmentVariable;
        }
    }

}
