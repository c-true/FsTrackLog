using System;
using System.IO;

namespace CTrue.FsTrackLog.Core.File
{
    public interface IFlightDataStore
    {
        void Initialize(string directory);

        IFsTrackLog CreateTrackLog();
    }

    public class FlightDataStore : IFlightDataStore
    {
        DirectoryInfo _storeDirectory;

        public void Initialize(string directory)
        {
            _storeDirectory = new DirectoryInfo(directory);

            if (!_storeDirectory.Exists)
                throw new Exception("Could not find directory: " + directory);
        }

        public IFsTrackLog CreateTrackLog()
        {
            var trackLogger = new TrackLogFileWriter(_storeDirectory.FullName);
            return new FsTrackLog(trackLogger);
        }
    }
}