using System;
using System.IO;
using CTrue.Fs.FlightData.Contracts;
using FsTrackLog.Proto.Generated;
using Google.Protobuf;

namespace CTrue.Fs.FlightData.Store
{
    public class FsTrackLogger
    {
        private const byte FSTRACKLOG_BINARY_VERSION = 0x01;

        private Stream _stream;

        public string FileName { get; }

        public FsTrackLogger(string directoryName)
        {
            DirectoryInfo di = new DirectoryInfo(directoryName);

            if (!di.Exists)
                throw new Exception("Could not find directory: " + directoryName);

            FileName = Path.Combine(directoryName, GetFileName());
            _stream = new FileStream(FileName, FileMode.Create, FileAccess.Write);

            Initialize();
        }
        
        public FsTrackLogger(Stream stream)
        {
            _stream = stream;

            Initialize();
        }

        private void Initialize()
        {
            FsTrackLogHeader header = new FsTrackLogHeader()
            {
                Version = FSTRACKLOG_BINARY_VERSION
            };

            header.WriteDelimitedTo(_stream);
        }

        public void Close()
        {
            _stream?.Flush();
            _stream?.Close();
        }
        

        public void LogTrackPoint(AircraftInfo value)
        {
            var tp = GetAircraftInfoBytes(value);
            tp.WriteDelimitedTo(_stream);
        }

        private static string GetFileName()
        {
            return $"FsTrackLog_{DateTime.Now.ToString("yyyyMMddhhmmss")}.fst";
        }

        private static FsTrackPoint GetAircraftInfoBytes(AircraftInfo value)
        {
            FsTrackPoint tp = new FsTrackPoint();

            tp.Time = value.TimeStamp.ToBinary();
            tp.Latitude = value.Latitude;
            tp.Longitude = value.Longitude;
            tp.Altitude = value.Altitude;
            tp.AltitudeAboveGround = value.AltitudeAboveGround;
            tp.Heading = value.Heading;
            tp.Speed = value.Speed;

            return tp;
        }
    }
}