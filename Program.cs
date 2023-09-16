using System;
using MoBro.Plugin.Aida64;
using MoBro.Plugin.SDK;
using Serilog.Events;

var plugin = MoBroPluginBuilder
  .Create<Plugin>()
  .WithLogLevel(LogEventLevel.Debug)
  .Build();

Console.ReadLine();