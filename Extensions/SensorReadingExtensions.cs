using System.Globalization;

namespace MoBro.Plugin.Aida64.Extensions;

internal static class SensorReadingExtensions
{
  // categories
  private static readonly string[] ProcessorLabels = { "cpu", "processor" };
  private static readonly string[] GraphicsLabels = { "gpu", "video" };
  private static readonly string[] MemoryLabels = { "memory", "dimm", "ram" };
  private static readonly string[] StorageLabels = { "drive", "disk", "hdd", "ssd" };
  private static readonly string[] BatteryLabels = { "battery" };
  private static readonly string[] NetworkLabels = { "nic" };

  private static readonly string[] MotherboardLabels =
    { "motherboard", "mainboard", "chipset", "vrm", "soc", "asus", "msi", "gigabyte", "asrock", "evga", "acer" };

  // metric types
  private static readonly string[] UsageLabels = { "utilization", "%" };
  private static readonly string[] FrequencyLabels = { "clock", "frequency" };
  private static readonly string[] MultiplierLabels = { "multiplier" };
  private static readonly string[] DataLabels = { "memory", "space" };

  public static IMetric ToMetric(this SensorReading reading)
  {
    return MoBroItem
      .CreateMetric()
      .WithId(reading.Id)
      .WithLabel(reading.Label)
      .OfType(ParseType(reading))
      .OfCategory(ParseCategory(reading))
      .OfNoGroup()
      .Build();
  }

  public static MetricValue ToMetricValue(this SensorReading reading, in DateTime time)
  {
    return new MetricValue(
      reading.Id,
      time,
      ReadingToValue(reading)
    );
  }

  private static CoreCategory ParseCategory(in SensorReading reading)
  {
    var id = reading.Id.ToLower();
    var label = reading.Label.ToLower();

    if (ContainsAny(ProcessorLabels, id, label)) return CoreCategory.Cpu;
    if (ContainsAny(GraphicsLabels, id, label)) return CoreCategory.Gpu;
    if (ContainsAny(StorageLabels, id, label)) return CoreCategory.Storage;
    if (ContainsAny(BatteryLabels, id, label)) return CoreCategory.Battery;
    if (ContainsAny(NetworkLabels, id, label)) return CoreCategory.Network;
    if (ContainsAny(MotherboardLabels, id, label)) return CoreCategory.Mainboard;
    if (ContainsAny(MemoryLabels, id, label)) return CoreCategory.Ram;

    // default
    return CoreCategory.Miscellaneous;
  }

  private static CoreMetricType ParseType(in SensorReading reading)
  {
    var lowerLabel = reading.Label.ToLower();

    // check for usage first as it is present across all reading types
    if (lowerLabel.ContainsAny(UsageLabels)) return CoreMetricType.Usage;

    switch (reading.Type)
    {
      case "sys": // System

        // all non numeric values => text
        if (!TryParseDouble(reading.Value, out _)) return CoreMetricType.Text;

        if (lowerLabel.ContainsAny(FrequencyLabels)) return CoreMetricType.Frequency;
        if (lowerLabel.ContainsAny(MultiplierLabels)) return CoreMetricType.Multiplier;
        if (lowerLabel.ContainsAny(DataLabels)) return CoreMetricType.Data;

        // network (nic) handling 
        if (lowerLabel.ContainsAny(NetworkLabels))
        {
          if (lowerLabel.Contains("total")) return CoreMetricType.Data;
          if (lowerLabel.ContainsAny("speed", "rate")) return CoreMetricType.DataFlow;
        }
        // handling of drives
        else if (lowerLabel.ContainsAny(StorageLabels))
        {
          if (lowerLabel.ContainsAny("read speed", "write speed"))
          {
            return CoreMetricType.DataFlow;
          }
        }

        // return default type
        break;

      case "temp": // Temperature
        return CoreMetricType.Temperature;
      case "fan": // Cooling Fans
      case "duty": // Fan Speeds
        return CoreMetricType.Rotation;
      case "volt": // Voltage
        return CoreMetricType.ElectricPotential;
      case "curr": // Current
        return CoreMetricType.ElectricCurrent;
      case "pwr": // Power
        return CoreMetricType.Power;
      case "flow": // Flow Sensors
        return CoreMetricType.VolumeFlow;
    }

    return TryParseDouble(reading.Value, out _) ? CoreMetricType.Numeric : CoreMetricType.Text;
  }

  private static object? ReadingToValue(in SensorReading reading)
  {
    // no need to convert non numeric values
    if (!TryParseDouble(reading.Value, out var doubleValue)) return reading.Value;

    // convert to base units
    return ParseType(reading) switch
    {
      CoreMetricType.Data => doubleValue * 1_000_000, // MB -> byte
      CoreMetricType.Frequency => doubleValue * 1_000_000, // MHz -> Hz
      CoreMetricType.DataFlow => ConvertDataFlow(reading, doubleValue),
      _ => doubleValue
    };
  }

  private static double ConvertDataFlow(in SensorReading reading, double parsedValue)
  {
    var lowerLabel = reading.Label.ToLower();

    // nic connection speed (gbit, 100mbit, ...)
    if (lowerLabel.Contains("connection speed")) return parsedValue * 1_000_000; // MBit -> bit

    // disk speed
    if (lowerLabel.ContainsAny("read speed", "write speed")) return parsedValue * 8_000_000; // MB -> bit

    // default (download, upload, etc.)
    return parsedValue * 8_000; //KB -> bit
  }

  private static bool ContainsAny(IEnumerable<string> labels, params string[] strs)
  {
    return strs.Length switch
    {
      <= 0 => false,
      1 => labels.Any(strs[0].Contains),
      _ => labels.Any(l => strs.Any(s => s.Contains(l)))
    };
  }

  private static bool TryParseDouble(string value, out double parsedValue)
  {
    return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedValue);
  }
}