using System;
using System.Collections.Generic;
using System.Linq;
using AIDA64.Readers;
using MoBro.Plugin.Aida64.Extensions;
using MoBro.Plugin.SDK;
using MoBro.Plugin.SDK.Enums;
using MoBro.Plugin.SDK.Models.Metrics;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.Aida64;

public class Plugin : IMoBroPlugin
{
  private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(2);
  private const int DefaultUpdateFrequencyMs = 1000;
  private const string DependencyKey = "aida64";

  private readonly IMoBroService _service;
  private readonly IMoBroScheduler _scheduler;
  private readonly SharedMemoryReader _sharedMemoryReader;

  private readonly int _updateFrequency;
  private readonly TimeSpan _nullValueThreshold;
  private readonly IDictionary<string, MetricValue> _values;

  private int _metricCount;

  public Plugin(IMoBroService service, IMoBroScheduler scheduler, IMoBroSettings settings)
  {
    _service = service;
    _scheduler = scheduler;
    _updateFrequency = settings.GetValue("update_frequency", DefaultUpdateFrequencyMs);
    _sharedMemoryReader = new SharedMemoryReader();
    _nullValueThreshold = TimeSpan.FromMilliseconds(_updateFrequency * 2);
    _values = new Dictionary<string, MetricValue>();
  }

  public void Init()
  {
    _scheduler.Interval(Update, TimeSpan.FromMilliseconds(_updateFrequency), InitialDelay);
  }

  private void Update()
  {
    var now = DateTime.UtcNow;
    var sensorValues = _sharedMemoryReader.Read().ToList();
    if (sensorValues.Count == 0)
    {
      _service.SetDependencyStatus(DependencyKey, DependencyStatus.Missing);
      return;
    }

    _service.SetDependencyStatus(DependencyKey, DependencyStatus.Ok);

    // register new metrics (if any)
    if (sensorValues.Count != _metricCount)
    {
      var unregisteredMetrics = sensorValues
        .Where(r => !_service.TryGet<Metric>(r.Id, out _))
        .Select(r => r.ToMetric())
        .ToList();

      _service.Register(unregisteredMetrics);
      _metricCount = sensorValues.Count;
    }

    // map and update values
    var metricValues = sensorValues
      .Select(r => r.ToMetricValue(now))
      .Where(mv => FlickerFilter(mv))
      .ToList();

    metricValues.ForEach(mv => _values[mv.Id] = mv);
    _service.UpdateMetricValues(metricValues);
  }

  private bool FlickerFilter(in MetricValue newVal)
  {
    // filter out 'null' values in case the existing non-null value is not older than a given threshold
    // this is to avoid 'flickering' of metric values in case the value is not available for short periods
    return newVal.Value != null
           || !_values.TryGetValue(newVal.Id, out var currValue)
           || currValue.Value == null
           || DateTime.UtcNow - currValue.Timestamp > _nullValueThreshold;
  }
}