namespace CTrue.FsTrackLog.Core
{
    public interface IFlightDataStore
    {
        void Initialize(string directory);

        IFsTrackLog CreateTrackLog();
    }
}