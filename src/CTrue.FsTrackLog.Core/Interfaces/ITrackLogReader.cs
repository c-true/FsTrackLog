using CTrue.FsTrackLog.Core.File.Generated.v1;

namespace CTrue.FsTrackLog.Core
{
    public interface ITrackLogReader
    {
        FsTrackPoint ReadNext();
    }
}