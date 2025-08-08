using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using FsTrackLogToKml;

using System.IO.Compression;

if (args.Length != 1)
{
    Console.WriteLine("Usage: FsTrackLogToKml <rootfolder>");
    return;
}

var rootPath = args[0];
if (!Directory.Exists(rootPath))
{
    Console.WriteLine($"File not found: {rootPath}");
    return;
}

string overviewKmlName = "WT24-AllFlights.kml";
string overviewKmzName = "WT24-AllFlights.kmz";
string outputFolder = Path.Combine(rootPath, "overview");
Directory.CreateDirectory(outputFolder);

var allFolders = Directory.GetDirectories(rootPath)
    .Where(d => Path.GetFileName(d)?.StartsWith("WT24-") == true)
    .ToList();

var allKmlByFolder = new Dictionary<string, List<string>>();

foreach (var folder in allFolders)
{
    var csvFiles = Directory.GetFiles(folder, "*.csv");
    var kmlFiles = new List<string>();

    foreach (var csv in csvFiles)
    {
        var converter = new FlightTrackConverter(csv);
        converter.Convert();
        kmlFiles.Add(converter.OutputKmlPath);
    }

    // Also include any manually placed KMLs
    kmlFiles.AddRange(Directory.GetFiles(folder, "*.kml").Where(k => !kmlFiles.Contains(k)));

    allKmlByFolder[Path.GetFileName(folder)!] = kmlFiles;
}

// Generate the aggregated KML
var combined = new StringBuilder();
combined.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
combined.AppendLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:gx=\"http://www.google.com/kml/ext/2.2\">");
combined.AppendLine("<Document>");
combined.AppendLine("  <name>WT24 Overview</name>");

foreach (var kvp in allKmlByFolder)
{
    combined.AppendLine($"  <Folder><name>{kvp.Key}</name>");
    foreach (var kmlFile in kvp.Value)
    {
        var content = File.ReadAllText(kmlFile);
        var start = content.IndexOf("<Placemark");
        var end = content.LastIndexOf("</Placemark>");
        if (start != -1 && end != -1)
        {
            var placemark = content.Substring(start, end - start + "</Placemark>".Length);
            combined.AppendLine(placemark);
        }
    }
    combined.AppendLine("  </Folder>");
}

combined.AppendLine("</Document>");
combined.AppendLine("</kml>");

var overviewKmlPath = Path.Combine(outputFolder, overviewKmlName);
File.WriteAllText(overviewKmlPath, combined.ToString());

Console.WriteLine($"Combined KML saved to: {overviewKmlPath}");

// Create KMZ
var overviewKmzPath = Path.Combine(outputFolder, overviewKmzName);
if (File.Exists(overviewKmzPath)) File.Delete(overviewKmzPath);
using (var archive = ZipFile.Open(overviewKmzPath, ZipArchiveMode.Create))
{
    archive.CreateEntryFromFile(overviewKmlPath, overviewKmlName);
}

Console.WriteLine($"KMZ created at: {overviewKmzPath}");