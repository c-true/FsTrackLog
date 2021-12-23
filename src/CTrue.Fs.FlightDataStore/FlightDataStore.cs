using System;
using System.IO;
using CTrue.Fs.FlightData.Contracts;

namespace CTrue.Fs.FlightData.Store
{
    public interface IFlightDataStore
    {
        void Initialize(string directory);
        void Close();

        void Write(AircraftInfoV1 value);
    }

    public class FlightDataStore : IFlightDataStore
    {
        private static FsTrackLogger _trackLogger;
        private bool _isOpen;

        public void Initialize(string directory)
        {
            if (_isOpen) return;

            DirectoryInfo di = new DirectoryInfo(directory);

            if (!di.Exists)
                throw new Exception("Could not find directory: " + directory);

            _trackLogger = new FsTrackLogger(di.FullName);
            _isOpen = true;
            
            Console.WriteLine($"Writing binary Track Log to {_trackLogger.FileName}");
        }

        public void Close()
        {
            if (!_isOpen) return;

            _trackLogger?.Close();
            _isOpen = false;
        }

        public void Write(AircraftInfoV1 value)
        {
            if (!_isOpen) return;

            if (value.SimOnGround) return;

            _trackLogger?.WriteNext(value);
        }
    }
}