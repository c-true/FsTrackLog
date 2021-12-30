using System;
using System.Collections.Generic;
using System.IO;

namespace CTrue.FsTrackLog.Core.File
{
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

        public List<IFsTrackLog> GetTrackLogs()
        {
            List<IFsTrackLog> trackLogs = new List<IFsTrackLog>();

            var fileList = _storeDirectory.GetFiles("*.fst");

            foreach (var fileInfo in fileList)
            {
                FsTrackLogFileReader reader = new FsTrackLogFileReader(fileInfo.FullName);

                FsTrackLog trackLog = new FsTrackLog(reader);
                
                if(reader.Version != 1)
                    continue;

                trackLog.Read();

                trackLogs.Add(trackLog);
            }

            return trackLogs;
        }
    }
}