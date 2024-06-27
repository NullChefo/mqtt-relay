# What is this all about

Parse MQTT messages and dynamically proccess them and store them into InfluxDB

# Commands uses

## influxDB client

dotnet add package InfluxDB.Client

## if needed Coravel package to schedule background jobs easily:

dotnet add package Coravel

## Add the dotnet mqtt

```bash
dotnet add package MQTTnet
```

# Add influxDB

```bash
dotnet add package InfluxDB.Client

```

## Initialize the Client

```dotnet
using System;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;

namespace Examples
{
  public class Examples
  {
    public static async Task Main(string[] args)
    {
      // You can generate an API token from the "API Tokens Tab" in the UI
      var token = Environment.GetEnvironmentVariable("INFLUX_TOKEN")!;
      const string bucket = "Iot";
      const string org = "my-org";

      using var client = new InfluxDBClient("http://192.168.0.135:8086", token);
    }
  }
}


```

## Write Data

### Option 1: Use InfluxDB Line Protocol to write data

```dotnet
const string data = "mem,host=host1 used_percent=23.43234543";
using (var writeApi = client.GetWriteApi())
{
  writeApi.WriteRecord(bucket, org, WritePrecision.Ns, data);
}

```

### Option 2: Use a Data Point to write data

```dotnet
var point = PointData
  .Measurement("mem")
  .Tag("host", "host1")
  .Field("used_percent", 23.43234543)
  .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

using (var writeApi = client.GetWriteApi())
{
  writeApi.WritePoint(bucket, org, point);
}


```

### Option 3: Use POCO and corresponding class to write data

```dotnet
var mem = new Mem { Host = "host1", UsedPercent = 23.43234543, Time = DateTime.UtcNow };

using (var writeApi = client.GetWriteApi())
{
  writeApi.WriteMeasurement(bucket, org, WritePrecision.Ns, mem);
}


[Measurement("mem")]
private class Mem
{
  [Column("host", IsTag = true)] public string Host { get; set; }
  [Column("used_percent")] public double? UsedPercent { get; set; }
  [Column(IsTimestamp = true)] public DateTime Time { get; set; }
}

```

### Execute a Flux query

```dotnet
var query = "from(bucket: \"Iot\") |> range(start: -1h)";
var tables = await client.GetQueryApi().QueryAsync(query, org);

foreach (var record in tables.SelectMany(table => table.Records))
{
    Console.WriteLine($"{record}");
}
```