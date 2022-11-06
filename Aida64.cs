using System.Collections.Immutable;
using System.Threading.Tasks;
using MoBro.Plugin.Aida64.Extensions;
using MoBro.Plugin.SDK.Models;

namespace MoBro.Plugin.Aida64;

public class Aida64 : IMoBroPlugin
{
  private IMoBro? _mobro;

  public Task Init(IPluginSettings settings, IMoBro mobro)
  {
    _mobro = mobro;

    var readings = SharedMemoryReader.ReadSensors();
    var metrics = GetUnregisteredMetrics(readings).ToArray();
    _mobro.Register(metrics);

    return Task.CompletedTask;
  }

  public Task<IEnumerable<IMetricValue>> GetMetricValues(IList<string> ids)
  {
    var readings = SharedMemoryReader.ReadSensors();
    var readingsArr = readings as SensorReading[] ?? readings.ToArray();

    // register new metrics (if any)
    var unregistered = GetUnregisteredMetrics(readingsArr).ToArray();
    if (unregistered.Length > 0)
    {
      _mobro?.Register(unregistered);
    }

    // map and return values
    var idSet = ids.ToImmutableHashSet();
    var now = DateTime.UtcNow;
    return Task.FromResult(SharedMemoryReader.ReadSensors()
      .Where(r => idSet.Contains(r.Id))
      .Select(r => r.ToMetricValue(now))
    );
  }

  private IEnumerable<IMoBroItem> GetUnregisteredMetrics(IEnumerable<SensorReading> readings)
  {
    var readingsArr = readings as SensorReading[] ?? readings.ToArray();

    if (_mobro == null || readingsArr.Length <= 0) return Enumerable.Empty<IMoBroItem>();

    return readingsArr
      .Where(r => !_mobro.TryGetItem<IMetric>(r.Id, out _))
      .Select(r => r.ToMetric());
  }

  public void Dispose()
  {
  }
}