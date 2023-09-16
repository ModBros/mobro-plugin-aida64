using System;
using System.Collections.Generic;
using System.Linq;
using AIDA64.Model;
using AIDA64.Readers;
using MoBro.Plugin.Aida64.Extensions;
using MoBro.Plugin.SDK;
using MoBro.Plugin.SDK.Models.Metrics;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.Aida64;

public class Plugin : IMoBroPlugin
{
  private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(2);
  private const int DefaultUpdateFrequencyMs = 1000;

  private readonly IMoBroService _service;
  private readonly IMoBroScheduler _scheduler;

  private readonly int _updateFrequency;
  private readonly SharedMemoryReader _sharedMemoryReader;

  public Plugin(IMoBroService service, IMoBroScheduler scheduler, IMoBroSettings settings)
  {
    _service = service;
    _scheduler = scheduler;
    _updateFrequency = settings.GetValue("update_frequency", DefaultUpdateFrequencyMs);
    _sharedMemoryReader = new SharedMemoryReader();
  }

  public void Init()
  {
    _scheduler.Interval(Update, TimeSpan.FromMilliseconds(_updateFrequency), InitialDelay);
  }

  private void Update()
  {
    var sensorValues = ReadSensorValues().ToList();

    // register new metrics (if any)
    var unregisteredMetrics = sensorValues
      .Where(r => !_service.TryGet<Metric>(r.Id, out _))
      .Select(r => r.ToMetric())
      .ToList();

    if (unregisteredMetrics.Any()) _service.Register(unregisteredMetrics);

    // map and update values
    var now = DateTime.UtcNow;
    var metricValues = sensorValues.Select(r => r.ToMetricValue(now));
    _service.UpdateMetricValues(metricValues);
  }

  private IEnumerable<SensorValue> ReadSensorValues()
  {
    try
    {
      return _sharedMemoryReader.Read();
    }
    catch (Exception)
    {
      // do not fail the plugin on errors resulting from reading the shared memory file
      return Enumerable.Empty<SensorValue>();
    }
  }
}