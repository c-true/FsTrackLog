using System;
using System.IO;
using CTrue.Fs.FlightData.Contracts;

namespace CTrue.Fs.FlightData.Store
{
    public class FlightDataStore
    {
        private static FsTrackLogger _trackLogger;

        public void Initialize(string directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);

            if (!di.Exists)
                throw new Exception("Could not find directory: " + directory);

            _trackLogger = new FsTrackLogger(di.FullName);
            Console.WriteLine($"Writing binary Track Log to {_trackLogger.FileName}");
        }

        public void Close()
        {
            _trackLogger?.Close();
        }

        public void Write(AircraftInfo value)
        {
            if (value.SimOnGround) return;

            _trackLogger?.LogTrackPoint(value);
        }
    }
}