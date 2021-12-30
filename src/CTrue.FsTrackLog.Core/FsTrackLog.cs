using System;
using CTrue.Fs.FlightData.Contracts;
using CTrue.FsTrackLog.Contracts;
using CTrue.FsTrackLog.Core.File;
using CTrue.FsTrackLog.Core.File.Generated.v1;
using IFlightDataStore = CTrue.FsTrackLog.Core.IFlightDataStore;

namespace CTrue.FsTrackLog.Core
{
    public class FsTrackLog : IFsTrackLog
    {
        private readonly ITrackLogWriter _writer;
        private readonly ITrackLogReader _reader;

        public event EventHandler TrackLogUpdated;

        public string FsTrackLogId { get; }

        public string AircraftName { get; set; }

        public AircraftInfoV1 Value { get; set; }

        public FsTrackLog(ITrackLogWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            FsTrackLogId = Guid.NewGuid().ToString();
        }

        public FsTrackLog(ITrackLogReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public void Write(AircraftInfoV1 value)
        {
            Value = value;

            _writer.Write(value);
            
            TrackLogUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Read()
        {
            var trackPoint = _reader.ReadNext();

            if (trackPoint == null)
                return;

            Value = trackPoint.ToAircraftInfoV1();
        }

        public void Close()
        {
            _writer.Close();
        }
    }

    public static class FsTrackPointExtensions
    {
        public static AircraftInfoV1 ToAircraftInfoV1(this FsTrackPoint trackPoint)
        {
            return new AircraftInfoV1()
            {
                Title = "",
                TimeStamp = DateTime.FromBinary(trackPoint.Time),
                Latitude = trackPoint.Latitude,
                Longitude = trackPoint.Longitude,
                Altitude = trackPoint.Altitude,
                AltitudeAboveGround = trackPoint.AltitudeAboveGround,
                Heading = trackPoint.Heading,
                Speed = trackPoint.Speed
            };
        }
    }
}