using System.Linq;

namespace MoBro.Plugin.Aida64.Extensions;

internal static class StringExtensions
{
  public static bool ContainsAny(this string str, params string[] values)
  {
    return values.Any(str.Contains);
  }
}