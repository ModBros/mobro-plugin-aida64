using MoBro.Plugin.Aida64.Extensions;
using MoBro.Plugin.SDK.Models;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.Aida64;

public class Aida64 : IMoBroPlugin
{
  private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(2);
  private const int DefaultUpdateFrequencyMs = 1000;

  private readonly IMoBroService _service;
  private readonly IMoBroScheduler _scheduler;

  private readonly int _updateFrequency;

  public Aida64(IMoBroService service, IMoBroScheduler scheduler, IMoBroSettings settings)
  {
    _service = service;
    _scheduler = scheduler;
    _updateFrequency = settings.GetValue("update_frequency", DefaultUpdateFrequencyMs);
  }

  public void Init()
  {
    _scheduler.Interval(Update, TimeSpan.FromMilliseconds(_updateFrequency), InitialDelay);
  }

  private void Update()
  {
    var readings = SharedMemoryReader.Read();

    // register new metrics (if any)
    var unregistered = GetUnregisteredMetrics(readings).ToArray();
    if (unregistered.Length > 0)
    {
      _service.Register(unregistered);
    }

    // map and update values
    var now = DateTime.UtcNow;
    var values = readings.Select(r => r.ToMetricValue(now));
    _service.UpdateMetricValues(values);
  }

  private IEnumerable<IMoBroItem> GetUnregisteredMetrics(IList<SensorReading> readings)
  {
    if (!readings.Any()) return Enumerable.Empty<IMoBroItem>();

    return readings
      .Where(r => !_service.TryGet<IMetric>(r.Id, out _))
      .Select(r => r.ToMetric());
  }

  public void Dispose()
  {
  }
}