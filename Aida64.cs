using System.Timers;
using MoBro.Plugin.Aida64.Extensions;
using MoBro.Plugin.SDK.Models;

namespace MoBro.Plugin.Aida64;

public class Aida64 : IMoBroPlugin
{
  private static readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(1000);

  private readonly Timer _timer;

  private IMoBroService? _service;

  public Aida64()
  {
    _timer = new Timer
    {
      Interval = UpdateInterval.TotalMilliseconds,
      AutoReset = true,
      Enabled = false
    };
    _timer.Elapsed += Update;
  }

  public void Init(IMoBroSettings settings, IMoBroService service)
  {
    _service = service;
    _timer.Start();
  }

  public void Pause() => _timer.Stop();

  public void Resume() => _timer.Start();

  private void Update(object? sender, ElapsedEventArgs e)
  {
    var readings = SharedMemoryReader.Read();

    // register new metrics (if any)
    var unregistered = GetUnregisteredMetrics(readings).ToArray();
    if (unregistered.Length > 0)
    {
      _service?.RegisterItems(unregistered);
    }

    // map and update values
    var now = DateTime.UtcNow;
    var values = readings.Select(r => r.ToMetricValue(now));
    _service?.UpdateMetricValues(values);
  }

  private IEnumerable<IMoBroItem> GetUnregisteredMetrics(IList<SensorReading> readings)
  {
    if (_service == null || !readings.Any()) return Enumerable.Empty<IMoBroItem>();

    return readings
      .Where(r => !_service.TryGetItem<IMetric>(r.Id, out _))
      .Select(r => r.ToMetric());
  }

  public void Dispose()
  {
  }
}