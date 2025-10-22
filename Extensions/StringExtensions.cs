using System;
using System.Linq;

namespace MoBro.Plugin.Aida64.Extensions;

internal static class StringExtensions
{
  public static bool ContainsAny(this string str, params string[] values)
  {
    return values.Length switch
    {
      0 => false,
      1 => str.Contains(values[0], StringComparison.OrdinalIgnoreCase),
      _ => values.Any(v => str.Contains(v, StringComparison.OrdinalIgnoreCase))
    };
  }
}