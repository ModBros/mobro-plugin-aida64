using MoBro.Plugin.Aida64;
using MoBro.Plugin.SDK.Testing;
using Serilog.Events;

var plugin = MoBroPluginBuilder
  .Create<Aida64>()
  .WithLogLevel(LogEventLevel.Debug)
  .Build();

Console.ReadLine();