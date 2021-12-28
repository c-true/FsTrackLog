using System;
using System.IO;
using CTrue.Fs.FlightData.Contracts;
using CTrue.FsTrackLog.Contracts;
using CTrue.FsTrackLog.Core.File.Generated;
using CTrue.FsTrackLog.Core.File.Generated.v1;
using Google.Protobuf;

namespace CTrue.FsTrackLog.Core.File
{
    public class TrackLogFileWriter : ITrackLogWriter
    {
        private const byte FSTRACKLOG_BINARY_VERSION = 0x01;

        private Stream _stream;

        public string FileName { get; }

        public TrackLogFileWriter(string directoryName)
        {
            DirectoryInfo di = new DirectoryInfo(directoryName);

            if (!di.Exists)
                throw new Exception("Could not find directory: " + directoryName);

            FileName = Path.Combine(directoryName, GetFileName());
            _stream = new FileStream(FileName, FileMode.Create, FileAccess.Write);

            Initialize();
        }
        
        public TrackLogFileWriter(Stream stream)
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

        public bool IsOpen => _stream != null;

        public void Close()
        {
            _stream?.Flush();
            _stream?.Close();
            _stream = null;
        }
        

        public void Write(AircraftInfoV1 value)
        {
            var tp = GetAircraftInfoBytes(value);
            tp.WriteDelimitedTo(_stream);
        }

        private static string GetFileName()
        {
            return $"FsTrackLog_{DateTime.Now.ToString("yyyyMMddhhmmss")}.fst";
        }

        private static FsTrackPoint GetAircraftInfoBytes(AircraftInfoV1 value)
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