using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FsTrackLog.Proto.Generated;
using Google.Protobuf;

namespace FsTrackLog
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

            DateTime utcTime = GetDateTime(value.ZuluYear, value.ZuluDayOfYear, value.ZuluTime);

            tp.Time = utcTime.ToBinary();
            tp.Latitude = value.Latitude;
            tp.Longitude = value.Longitude;
            tp.Altitude = value.Altitude;
            tp.AltitudeAboveGround = value.AltitudeAboveGround;
            tp.Heading = value.Heading;
            tp.Speed = value.Speed;

            return tp;
        }

        private static DateTime GetDateTime(ulong year, ulong dayInYear, ulong secondsInDay)
        {
            return new DateTime((int)year, 1, 1).AddDays(dayInYear).AddSeconds(secondsInDay);
        }
    }
}