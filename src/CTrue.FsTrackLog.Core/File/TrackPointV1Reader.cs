using System.IO;
using System.Runtime.InteropServices.ComTypes;
using CTrue.FsTrackLog.Proto.v2;

namespace CTrue.FsTrackLog.Core.File
{
    public class TrackPointV1Reader : TrackPointReader
    {
        public TrackPointV1Reader(Stream stream) : base(stream)
        {
        }

        protected override void ReadTrackLogHeader()
        {
            // V1 has no header. Must generate content
            Title = "";
            AtcId = "";
            AtcModel = "";
            FuelTotalCapacity = 0;
        }

        public override bool CanReadNext()
        {
            return _stream.Position < _stream.Length;
        }

        public override FsTrackPoint ReadNext()
        {
            if (_stream.Position == _stream.Length) return null;

            var v1Data = CTrue.FsTrackLog.Proto.v1.FsTrackPoint.Parser.ParseDelimitedFrom(_stream);
            
            return new FsTrackPoint()
            {
                Time = v1Data.Time,
                Latitude = v1Data.Latitude,
                Longitude = v1Data.Longitude,
                Altitude = v1Data.Altitude,
                AltitudeAboveGround = v1Data.AltitudeAboveGround,
                Heading = v1Data.Heading,
                Speed = v1Data.Speed,
                SimOnGround = false,
                AutopilotMaster = false,
                FuelTotalQuantity = 0
            };
        }
    }
}