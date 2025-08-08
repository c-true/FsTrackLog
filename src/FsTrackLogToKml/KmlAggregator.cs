using System.Text;

namespace FsTrackLogToKml;

public static class KmlAggregator
{
    public static void CombineKmlFiles(string rootFolder, string outputFilePath)
    {
        var kmlFiles = Directory.GetDirectories(rootFolder)
            .SelectMany(dir => Directory.EnumerateFiles(dir, "*.kml"))
            .ToList();

        var placemarks = new List<string>();

        foreach (var file in kmlFiles)
        {
            var content = File.ReadAllText(file);
            var start = content.IndexOf("<Placemark");
            var end = content.LastIndexOf("</Placemark>");
            if (start != -1 && end != -1)
            {
                var placemark = content.Substring(start, end - start + "</Placemark>".Length);
                placemarks.Add(placemark);
            }
        }

        var combined = new StringBuilder();
        combined.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        combined.AppendLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:gx=\"http://www.google.com/kml/ext/2.2\">");
        combined.AppendLine("  <Document>");
        combined.AppendLine("    <name>All Flights</name>");

        foreach (var pm in placemarks)
        {
            combined.AppendLine(pm);
        }

        combined.AppendLine("  </Document>");
        combined.AppendLine("</kml>");

        File.WriteAllText(outputFilePath, combined.ToString());
        Console.WriteLine($"Combined {placemarks.Count} tracks into {outputFilePath}");
    }
}
