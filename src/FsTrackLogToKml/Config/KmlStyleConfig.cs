namespace FsTrackLogToKml;

public class KmlStyleConfig
{
    public PlanStyle PlanStyle { get; set; } = new PlanStyle();
    public RouteStyle RouteStyle { get; set; } = new RouteStyle();
    public TrackStyle TrackStyle { get; set; } = new TrackStyle();
}

public class PlanStyle
{
    public string LineColor { get; set; } = "7f00ffff";
    public int LineWidth { get; set; } = 4;
    public string PolyStyle { get; set; } = "7f00ff00";
}

public class RouteStyle
{
    public string LineColor { get; set; } = "ddff40fc";
    public int LineWidth { get; set; } = 6;
    public string PolyStyle { get; set; } = "77ff40fc";
}

public class TrackStyle
{
    public string LineColor { get; set; } = "ff00ff00";
    public int LineWidth { get; set; } = 4;
    public string PolyStyle { get; set; } = "7f00ff00";
}