using CTrue.FsTrackLog.Proto.v2;

namespace CTrue.FsTrackLog.Core
{
    public interface ITrackLogReader
    {
        string Title { get; }
        string AtcId { get; }
        string AtcModel { get; }
        double FuelTotalCapacity { get; }
        FsTrackPoint ReadNext();
    }
}