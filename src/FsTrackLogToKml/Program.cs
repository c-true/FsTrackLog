using System.Globalization;
using System.Text;

if (args.Length != 1)
{
    Console.WriteLine("Usage: FsTrackLogToKml <input.csv>");
    return;
}

var inputCsvPath = args[0];
if (!File.Exists(inputCsvPath))
{
    Console.WriteLine($"File not found: {inputCsvPath}");
    return;
}

var fileName = Path.GetFileNameWithoutExtension(inputCsvPath);
var outputKmlPath = Path.ChangeExtension(inputCsvPath, ".kml");

var times = new List<string>();
var coords = new List<string>();
var headings = new List<string>();

try
{
    using var reader = new StreamReader(inputCsvPath);
    var header = reader.ReadLine();
    if (header == null)
    {
        Console.WriteLine("CSV file is empty.");
        return;
    }

    var headers = header.Split(',');
    int tsIndex = Array.IndexOf(headers, "TimeStamp");
    int latIndex = Array.IndexOf(headers, "Latitude");
    int lonIndex = Array.IndexOf(headers, "Longitude");
    int altIndex = Array.IndexOf(headers, "Altitude");
    int headingIndex = Array.IndexOf(headers, "Heading");

    if (tsIndex == -1 || latIndex == -1 || lonIndex == -1 || altIndex == -1 || headingIndex == -1)
    {
        Console.WriteLine("CSV must contain TimeStamp, Latitude, Longitude, Altitude, and Heading columns.");
        return;
    }

    while (!reader.EndOfStream)
    {
        var line = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(line)) continue;

        var fields = line.Split(',');

        if (fields.Length <= Math.Max(tsIndex, Math.Max(latIndex, Math.Max(lonIndex, Math.Max(altIndex, headingIndex)))))
            continue;

        if (DateTime.TryParseExact(fields[tsIndex], "yyyy-MM-dd-HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp) &&
            double.TryParse(fields[lonIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) &&
            double.TryParse(fields[latIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
            double.TryParse(fields[altIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var alt) &&
            double.TryParse(fields[headingIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out var heading))
        {
            // ISO 8601 format for <when>
            times.Add(timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
            double altMeters = alt * 0.3048;
            coords.Add($"{lon.ToString(CultureInfo.InvariantCulture)} {lat.ToString(CultureInfo.InvariantCulture)} {altMeters.ToString(CultureInfo.InvariantCulture)}");
            headings.Add($"{heading.ToString(CultureInfo.InvariantCulture)} 0 0"); // heading, tilt=0, roll=0
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error reading CSV: {ex.Message}");
    return;
}

if (coords.Count == 0)
{
    Console.WriteLine("No valid coordinates found.");
    return;
}

// Build KML with gx:Track
var kml = new StringBuilder();
kml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
kml.AppendLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\"");
kml.AppendLine("     xmlns:gx=\"http://www.google.com/kml/ext/2.2\">");
kml.AppendLine("  <Document>");
kml.AppendLine($"    <name>{fileName}</name>");

kml.AppendLine("    <Placemark>");
kml.AppendLine("      <name>Aircraft</name>");
kml.AppendLine("      <Style>");
kml.AppendLine("        <IconStyle>");
kml.AppendLine("          <scale>1.2</scale>");
kml.AppendLine("          <Icon>");
kml.AppendLine("            <href>http://maps.google.com/mapfiles/kml/shapes/airports.png</href>");
kml.AppendLine("          </Icon>");
kml.AppendLine("        </IconStyle>");
kml.AppendLine("      </Style>");

kml.AppendLine("      <Style>");
kml.AppendLine("        <LineStyle>");
kml.AppendLine("          <color>ff0000ff</color>"); // Red (ABGR)
kml.AppendLine("          <width>3</width>");
kml.AppendLine("        </LineStyle>");
kml.AppendLine("      </Style>");
kml.AppendLine("      <gx:Track>");
kml.AppendLine("        <Model>");
kml.AppendLine("          <altitudeMode>absolute</altitudeMode>");
kml.AppendLine("          <Location/>"); // Populated dynamically by gx:Track
kml.AppendLine("          <Orientation/>"); // From <gx:angles>
kml.AppendLine("          <Scale><x>1</x><y>1</y><z>1</z></Scale>");
kml.AppendLine("          <Link>");
kml.AppendLine("            <href>http://maps.google.com/mapfiles/kml/shapes/airports.png</href>");
kml.AppendLine("          </Link>");
kml.AppendLine("        </Model>");
kml.AppendLine("        <altitudeMode>absolute</altitudeMode>");

for (int i = 0; i < coords.Count; i++)
{
    kml.AppendLine($"        <when>{times[i]}</when>");
}
for (int i = 0; i < coords.Count; i++)
{
    kml.AppendLine($"        <gx:coord>{coords[i]}</gx:coord>");
}
for (int i = 0; i < coords.Count; i++)
{
    kml.AppendLine($"        <gx:angles>{headings[i]}</gx:angles>");
}

kml.AppendLine("      </gx:Track>");
kml.AppendLine("    </Placemark>");
kml.AppendLine("  </Document>");
kml.AppendLine("</kml>");

try
{
    File.WriteAllText(outputKmlPath, kml.ToString());
    Console.WriteLine($"KML file created: {outputKmlPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to write KML file: {ex.Message}");
}
