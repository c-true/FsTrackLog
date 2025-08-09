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

GeneratorSettings settings = new GeneratorSettings();
settings.BaseDir = rootPath;
settings.WorkFolder = Directory.CreateTempSubdirectory("fskml_").FullName;

Console.WriteLine($"Work folder: {settings.WorkFolder}");

settings.TourConfig = LoadTourConfig(rootPath);
settings.StyleConfig = LoadKmlStyleConfig(rootPath);

string baseName = settings.TourConfig.Name;
string overviewKmlName = $"{baseName}-AllFlights.kml";
string overviewKmzName = $"{baseName}-AllFlights.kmz";
string outputFolder = Path.Combine(settings.WorkFolder, "overview");
Directory.CreateDirectory(outputFolder);

var allRouteKmlByFolder = new Dictionary<string, List<string>>();
var allTrackKmlByFolder = new Dictionary<string, List<string>>();

var allFolders = Directory.GetDirectories(rootPath)
    .Where(d => Path.GetFileName(d)?.StartsWith("WT24-") == true)
    .ToList();

foreach (var folder in allFolders)
{
    // Routes
    // Also include any manually placed KMLs
    var routeKmlFiles = Directory.GetFiles(folder, "*.kml");
    allRouteKmlByFolder[Path.GetFileName(folder)!] = routeKmlFiles.ToList();

    // Tracks
    var csvFiles = Directory.GetFiles(folder, "*.csv");
    var trackKmlFiles = new List<string>();

    foreach (var csv in csvFiles)
    {
        var converter = new FlightTrackConverter(settings, csv);
        converter.Convert();
        trackKmlFiles.Add(converter.OutputKmlPath);
    }

    allTrackKmlByFolder[Path.GetFileName(folder)!] = trackKmlFiles;
}

// Generate the aggregated KML
var combined = new StringBuilder();
combined.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
combined.AppendLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\" xmlns:gx=\"http://www.google.com/kml/ext/2.2\">");
combined.AppendLine("<Document>");
combined.AppendLine($"  <name>{settings.TourConfig.Title}</name>");
combined.AppendLine($"  <description>{settings.TourConfig.Description}</description>");

// Style for overall plan
combined.AppendLine("    <Style id=\"yellowLineGreenPoly0\">");
combined.AppendLine("       <LineStyle>");
combined.AppendLine($"        <color>{settings.StyleConfig.PlanStyle.LineColor}</color>");
combined.AppendLine($"        <width>{settings.StyleConfig.PlanStyle.LineWidth}</width>");
combined.AppendLine("       </LineStyle>");
combined.AppendLine("       <PolyStyle>");
combined.AppendLine($"           <color>{settings.StyleConfig.PlanStyle.PolyStyle}</color>");
combined.AppendLine("       </PolyStyle>");
combined.AppendLine("    </Style>");

// Styles for route
combined.AppendLine("    <Style id=\"RouteMark\">");
combined.AppendLine("       <LineStyle>");
combined.AppendLine($"           <color>{settings.StyleConfig.RouteStyle.LineColor}</color>");
combined.AppendLine($"           <width>{settings.StyleConfig.RouteStyle.LineWidth}</width>");
combined.AppendLine("       </LineStyle>");
combined.AppendLine("       <PolyStyle> ");
combined.AppendLine($"           <color>{settings.StyleConfig.RouteStyle.PolyStyle}</color>");
combined.AppendLine("       </PolyStyle> ");
combined.AppendLine("    </Style>");
combined.AppendLine("    <Style id=\"FixMark\">");
combined.AppendLine("       <IconStyle>");
combined.AppendLine("           <Icon>");
combined.AppendLine("               <href>http://maps.google.com/mapfiles/kml/shapes/triangle.png</href>");
combined.AppendLine("           </Icon>");
combined.AppendLine("       </IconStyle>");
combined.AppendLine("       <color>ffffffff</color>");
combined.AppendLine("    </Style>");

// Styles for track
combined.AppendLine("    <Style id=\"defaultStyle\">");
combined.AppendLine("      <LineStyle>");
combined.AppendLine($"        <color>{settings.StyleConfig.TrackStyle.LineColor}</color>");
combined.AppendLine($"        <width>{settings.StyleConfig.TrackStyle.LineWidth}</width>");
combined.AppendLine("      </LineStyle>");
combined.AppendLine("      <PolyStyle>");
combined.AppendLine($"       <color>{settings.StyleConfig.TrackStyle.PolyStyle}</color>");
combined.AppendLine("       </PolyStyle>");
combined.AppendLine("    </Style>");

// Plans
var allPlanKmlFiles = new Dictionary<string, List<string>>();
string plansFolder = Path.Combine(rootPath, "Plans");
var planFiles = Directory.GetFiles(plansFolder, "*.kml");

allPlanKmlFiles["Plans"] = planFiles.ToList();

AddKmlFilesAsFolders(allPlanKmlFiles, combined);

combined.AppendLine($"  <Folder><name>Routes</name>");

AddKmlFilesAsFolders(allRouteKmlByFolder, combined);

combined.AppendLine("  </Folder>");

combined.AppendLine($"  <Folder><name>Tracks</name>");

AddKmlFilesAsFolders(allTrackKmlByFolder, combined);

combined.AppendLine("  </Folder>");

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

// Copy output files
string overviewFolder = Path.Combine(settings.BaseDir, "overview");

CopyAllFiles(outputFolder, overviewFolder);

Console.WriteLine("Removing work folder");
Directory.Delete(settings.WorkFolder, true);

Console.WriteLine($"KMZ created at: {overviewKmzPath}");

static void CopyAllFiles(string sourceFolder, string destinationFolder)
{
    if (!Directory.Exists(sourceFolder))
        throw new DirectoryNotFoundException($"Source folder not found: {sourceFolder}");

    // Create destination folder if it doesn't exist
    Directory.CreateDirectory(destinationFolder);

    // Copy each file
    foreach (string filePath in Directory.GetFiles(sourceFolder))
    {
        string fileName = Path.GetFileName(filePath);
        string destPath = Path.Combine(destinationFolder, fileName);
        File.Copy(filePath, destPath, overwrite: true);
    }
}

static void AddKmlFilesAsFolders(Dictionary<string, List<string>> kmlFiles, StringBuilder kml)
{
    foreach (var kvp in kmlFiles)
    {
        kml.AppendLine($"  <Folder><name>{kvp.Key}</name>");
        foreach (var kmlFile in kvp.Value)
        {
            AppendKmlFileAsPlacemark(kmlFile, kml);
        }
        kml.AppendLine("  </Folder>");
    }
}

// Extracts the Placement element of a KML file
static void AppendKmlFileAsPlacemark(string kmlFile, StringBuilder kml)
{
    var content = File.ReadAllText(kmlFile);
    var start = content.IndexOf("<Placemark");
    var end = content.LastIndexOf("</Placemark>");
    if (start != -1 && end != -1)
    {
        var placemark = content.Substring(start, end - start + "</Placemark>".Length);
        kml.AppendLine(placemark);
    }
}

static TourConfig LoadTourConfig(string baseDir)
{
    var configFileName = "tourconfig.json";

    var searchPaths = new[]
    {
        Path.Combine(baseDir, "config", configFileName)
    };

    foreach (var path in searchPaths)
    {
        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<TourConfig>(json);
                if (config != null) return config;
            }
            catch { }
        }
    }

    return new TourConfig();
}

static KmlStyleConfig LoadKmlStyleConfig(string baseDir)
{
    var configFileName = "kmlconfig.json";

    var searchPaths = new[]
    {
        Path.Combine(baseDir, "config", configFileName)
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