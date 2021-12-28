namespace CTrue.FsTrackLog.Core.File
{
    public interface IFlightDataStore
    {
        void Initialize(string directory);

        IFsTrackLog CreateTrackLog();
    }
}