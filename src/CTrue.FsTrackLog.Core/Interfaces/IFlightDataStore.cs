using System.Collections.Generic;

namespace CTrue.FsTrackLog.Core
{
    public interface IFlightDataStore
    {
        void Initialize(string directory);

        IFsTrackLog CreateTrackLog();
        List<IFsTrackLog> GetTrackLogs();
    }
}