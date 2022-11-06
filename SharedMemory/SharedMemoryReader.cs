using System.IO.MemoryMappedFiles;
using System.Text.RegularExpressions;
using System.Xml;
using MoBro.Plugin.SDK.Exceptions;

namespace MoBro.Plugin.Aida64.SharedMemory;

internal static class SharedMemoryReader
{
  private static readonly Regex IdSanitationRegex = new(@"[^\w\.\-]", RegexOptions.Compiled);

  private const string SharedMemoryFileName = "Global\\AIDA64_SensorValues";

  public static IEnumerable<SensorReading> ReadSensors()
  {
    try
    {
      using var mmf = MemoryMappedFile.OpenExisting(SharedMemoryFileName, MemoryMappedFileRights.Read);
      return Read(mmf);
    }
    catch (Exception)
    {
      // error while trying to read from shared memory file
      // -> may no longer be valid (i.e. AIDA64 was closed)
      return Enumerable.Empty<SensorReading>();
    }
  }

  private static IEnumerable<SensorReading> Read(MemoryMappedFile mmf)
  {
    try
    {
      using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
      var i = 1;
      var b = accessor.ReadByte(0);
      List<byte> list = new();
      while (b != '\x00')
      {
        list.Add(b);
        b = accessor.ReadByte(i++);
      }

      var xml = "<root>" + Encoding.ASCII.GetString(list.ToArray()).Trim() + "</root>";
      XmlDocument xmlDoc = new();
      xmlDoc.LoadXml(xml);

      if (xmlDoc.FirstChild == null) return Enumerable.Empty<SensorReading>();
      return
      (
        from XmlNode node in xmlDoc.FirstChild.ChildNodes
        let id = node.SelectSingleNode("id")?.InnerText.Trim().ToLower()
        let label = node.SelectSingleNode("label")?.InnerText.Trim()
        let value = node.SelectSingleNode("value")?.InnerText.Trim()
        where id != null && label != null && value != null
        select new SensorReading(SanitizeId(id), label, value, node.Name.Trim())
      ).ToList();
    }
    catch (Exception e)
    {
      // error reading from shared memory file
      // may no longer be valid (i.e. Aida64 was closed)
      throw new PluginDependencyException("Failed to read from AIDA64 shared memory file", e);
    }
  }

  private static string SanitizeId(string id)
  {
    return IdSanitationRegex.Replace(id, "");
  }
}