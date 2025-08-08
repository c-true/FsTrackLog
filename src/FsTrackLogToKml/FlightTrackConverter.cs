using System.Globalization;
using System.Text;
using System.Text.Json;

namespace FsTrackLogToKml;

public class FlightTrackConverter
{
    public string CsvPath { get; }
    public string TrackName { get; }
    public string OutputKmlPath { get; }
    public KmlStyleConfig StyleConfig { get; }

    public FlightTrackConverter(string csvPath)
    {
        CsvPath = Path.GetFullPath(csvPath);

        var folder = Path.GetFileName(Path.GetDirectoryName(CsvPath)) ?? "Unknown";
        var fileName = Path.GetFileNameWithoutExtension(CsvPath) ?? "Unknown";
        var parts = fileName.Split('_');
        var fromIcao = parts.Length > 0 ? parts[0] : "XXXX";
        var toIcao = parts.Length > 1 ? parts[1] : "XXXX";

        TrackName = $"{folder}-{fromIcao}-{toIcao}";
        OutputKmlPath = Path.Combine(Path.GetDirectoryName(CsvPath)!, $"{TrackName}.kml");

        StyleConfig = LoadKmlStyleConfig(CsvPath);
    }

    public void Convert()
    {
        var lines = File.ReadAllLines(CsvPath);
        if (lines.Length < 2)
            throw new InvalidOperationException("CSV file is empty or missing header.");

        var headers = lines[0].Split(',');
        int tsIdx = Array.IndexOf(headers, "TimeStamp");
        int latIdx = Array.IndexOf(headers, "Latitude");
        int lonIdx = Array.IndexOf(headers, "Longitude");
        int altIdx = Array.IndexOf(headers, "Altitude");
        int headingIdx = Array.IndexOf(headers, "Heading");

        if (tsIdx < 0 || latIdx < 0 || lonIdx < 0 || altIdx < 0 || headingIdx < 0)
            throw new InvalidOperationException("CSV missing required columns.");

        var coords = new List<string>();
        var times = new List<string>();
        var angles = new List<string>();

        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split(',');
            if (fields.Length <= Math.Max(tsIdx, Math.Max(latIdx, Math.Max(lonIdx, Math.Max(altIdx, headingIdx)))))
                continue;

            if (DateTime.TryParseExact(fields[tsIdx], "yyyy-MM-dd-HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var ts) &&
                double.TryParse(fields[lonIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) &&
                double.TryParse(fields[latIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(fields[altIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out var altFt) &&
                double.TryParse(fields[headingIdx], NumberStyles.Float, CultureInfo.InvariantCulture, out var heading))
            {
                double altMeters = altFt * 0.3048;
                times.Add(ts.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
                coords.Add($"{lon.ToString(CultureInfo.InvariantCulture)} {lat.ToString(CultureInfo.InvariantCulture)} {altMeters.ToString(CultureInfo.InvariantCulture)}");
                angles.Add($"{heading.ToString(CultureInfo.InvariantCulture)} 0 0");
            }
        }

        var kml = new StringBuilder();
        kml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        kml.AppendLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:gx=\"http://www.google.com/kml/ext/2.2\">");
        kml.AppendLine("  <Document>");
        kml.AppendLine($"    <name>{TrackName}</name>");
        kml.AppendLine("    <Style id=\"defaultStyle\">");
        kml.AppendLine("      <LineStyle>");
        kml.AppendLine($"        <color>{StyleConfig.LineColor}</color>");
        kml.AppendLine($"        <width>{StyleConfig.LineWidth}</width>");
        kml.AppendLine("      </LineStyle>");
        kml.AppendLine("    </Style>");
        kml.AppendLine("    <Placemark>");
        kml.AppendLine($"      <name>{TrackName}</name>");
        kml.AppendLine("      <styleUrl>#defaultStyle</styleUrl>");
        kml.AppendLine("      <gx:Track>");
        kml.AppendLine("        <altitudeMode>absolute</altitudeMode>");
        for (int i = 0; i < times.Count; i++) kml.AppendLine($"        <when>{times[i]}</when>");
        for (int i = 0; i < coords.Count; i++) kml.AppendLine($"        <gx:coord>{coords[i]}</gx:coord>");
        for (int i = 0; i < angles.Count; i++) kml.AppendLine($"        <gx:angles>{angles[i]}</gx:angles>");
        kml.AppendLine("      </gx:Track>");
        kml.AppendLine("    </Placemark>");
        kml.AppendLine("  </Document>");
        kml.AppendLine("</kml>");

        File.WriteAllText(OutputKmlPath, kml.ToString());
        Console.WriteLine($"Generated: {OutputKmlPath}");
    }

    private static KmlStyleConfig LoadKmlStyleConfig(string csvPath)
    {
        var configFileName = "kmlconfig.json";
        var baseDir = Path.GetDirectoryName(csvPath)!;
        var parentDir = Directory.GetParent(baseDir)?.FullName;

        var searchPaths = new[]
        {
            Path.Combine(baseDir, configFileName),
            Path.Combine(parentDir ?? "", configFileName),
            Path.Combine(parentDir ?? "", "kmlconfig", configFileName)
        };

        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var config = JsonSerializer.Deserialize<KmlStyleConfig>(json);
                    if (config != null) return config;
                }
                catch { }
            }
        }

        return new KmlStyleConfig();
    }
}
