using System.IO.MemoryMappedFiles;
using System.Text.RegularExpressions;
using System.Xml;

namespace MoBro.Plugin.Aida64.SharedMemory;

internal static class SharedMemoryReader
{
  private static readonly Regex IdSanitationRegex = new(@"[^\w\.\-]", RegexOptions.Compiled);

  private const string SharedMemoryFileName = "Global\\AIDA64_SensorValues";

  public static IList<SensorReading> Read()
  {
    try
    {
      using var mmf = MemoryMappedFile.OpenExisting(SharedMemoryFileName, MemoryMappedFileRights.Read);
      using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

      Span<byte> bytes = stackalloc byte[(int)accessor.Capacity];
      accessor.SafeMemoryMappedViewHandle.ReadSpan(0, bytes);
      var endIdx = bytes.IndexOf(Convert.ToByte('\x00'));
      var sharedMemoryXml = Encoding.ASCII.GetString(bytes[..endIdx]);

      var xmlDoc = new XmlDocument();
      xmlDoc.LoadXml($"<root>{sharedMemoryXml}</root>");

      if (xmlDoc.FirstChild == null) return new List<SensorReading>(0);

      var childNodes = xmlDoc.FirstChild.ChildNodes;
      var readings = new List<SensorReading>(childNodes.Count);
      foreach (XmlNode node in childNodes)
      {
        var id = ReadNodeText(node, "id");
        if (id == null) continue;

        var label = ReadNodeText(node, "label");
        if (label == null) continue;

        var value = ReadNodeText(node, "value");
        if (value == null) continue;

        readings.Add(new SensorReading(SanitizeId(id), label, value, node.Name.Trim()));
      }

      return readings;
    }
    catch (Exception)
    {
      // error while trying to read from shared memory file
      return new List<SensorReading>(0);
    }
  }

  private static string? ReadNodeText(XmlNode node, string xPath)
  {
    return node.SelectSingleNode(xPath)?.InnerText.Trim();
  }

  private static string SanitizeId(string id)
  {
    return IdSanitationRegex.Replace(id, "");
  }
}