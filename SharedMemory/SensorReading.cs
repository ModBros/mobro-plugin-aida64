namespace MoBro.Plugin.Aida64.SharedMemory;

internal readonly record struct SensorReading(
  string Id,
  string Label,
  string Value,
  string Type
);