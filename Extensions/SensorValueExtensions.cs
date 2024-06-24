using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AIDA64.Model;
using MoBro.Plugin.SDK.Builders;
using MoBro.Plugin.SDK.Enums;
using MoBro.Plugin.SDK.Models.Metrics;

namespace MoBro.Plugin.Aida64.Extensions;

internal static class SensorValueExtensions
{
  // categories
  private static readonly string[] ProcessorLabels = { "cpu", "processor" };
  private static readonly string[] GraphicsLabels = { "gpu", "video" };
  private static readonly string[] MemoryLabels = { "memory", "dimm", "ram", "ddr" };
  private static readonly string[] StorageLabels = { "drive", "disk", "hdd", "ssd", "nvme" };
  private static readonly string[] BatteryLabels = { "battery" };
  private static readonly string[] NetworkLabels = { "nic" };

  private static readonly string[] MotherboardLabels =
    { "motherboard", "mainboard", "chipset", "vrm", "soc", "asus", "msi", "gigabyte", "asrock", "evga", "acer" };

  // metric types
  private static readonly string[] UsageLabels = { "utilization", "%" };
  private static readonly string[] FrequencyLabels = { "clock", "frequency" };
  private static readonly string[] MultiplierLabels = { "multiplier" };
  private static readonly string[] DataLabels = { "memory", "space" };

  private const string TrialValue = "TRIAL";

  public static Metric ToMetric(this in SensorValue reading)
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

  public static MetricValue ToMetricValue(this in SensorValue reading, in DateTime time)
  {
    return new MetricValue(
      reading.Id,
      time,
      ReadingToValue(reading)
    );
  }

  private static CoreCategory ParseCategory(in SensorValue reading)
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

  private static CoreMetricType ParseType(in SensorValue reading)
  {
    // check for value of trial version first
    if (TrialValue.Equals(reading.Value.Trim())) return CoreMetricType.Text;

    var lowerLabel = reading.Label.ToLower();

    // check for usage first as it is present across all reading types
    if (lowerLabel.ContainsAny(UsageLabels)) return CoreMetricType.Usage;

    switch (reading.Type)
    {
      case SensorType.System:

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

      case SensorType.Temperature:
        return CoreMetricType.Temperature;
      case SensorType.FanSpeed:
      case SensorType.CoolingFan:
        return CoreMetricType.Rotation;
      case SensorType.Voltage:
        return CoreMetricType.ElectricPotential;
      case SensorType.Current:
        return CoreMetricType.ElectricCurrent;
      case SensorType.Power:
        return CoreMetricType.Power;
      case SensorType.WaterFlow:
        return CoreMetricType.VolumeFlow;
      case SensorType.Unknown:
      default:
        break;
    }

    return TryParseDouble(reading.Value, out _) ? CoreMetricType.Numeric : CoreMetricType.Text;
  }

  private static object? ReadingToValue(in SensorValue reading)
  {
    // no need to convert non numeric values
    if (!TryParseDouble(reading.Value, out var doubleValue)) return reading.Value;

    // convert to base units
    switch (ParseType(reading))
    {
      case CoreMetricType.Data:

        if (ParseCategory(reading) == CoreCategory.Storage)
        {
          return doubleValue * 1_000_000_000; // GB -> byte
        }

        return doubleValue * 1_000_000; // MB -> byte
      case CoreMetricType.Frequency:
        return doubleValue * 1_000_000; // MHz -> Hz
      case CoreMetricType.DataFlow:
        return ConvertDataFlow(reading, doubleValue);
      default:
        return doubleValue;
    }
  }

  private static double ConvertDataFlow(in SensorValue reading, double parsedValue)
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