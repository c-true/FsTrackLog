using System;
using CTrue.Fs.FlightData.Contracts;
using CTrue.FsTrackLog.Contracts;
using CTrue.FsTrackLog.Proto.v2;
using IFlightDataStore = CTrue.FsTrackLog.Core.IFlightDataStore;

namespace CTrue.FsTrackLog.Core
{
    public class FsTrackLog : IFsTrackLog
    {
        private readonly ITrackLogWriter _writer;
        private readonly ITrackLogReader _reader;

        public event EventHandler TrackLogUpdated;

        public string FsTrackLogId { get; }

        public AircraftInfo Value { get; set; }

        public FsTrackLog(ITrackLogWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            FsTrackLogId = Guid.NewGuid().ToString();
        }

        public FsTrackLog(ITrackLogReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public void Write(AircraftInfo value)
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

            Value = new AircraftInfo()
            {
                Title = _reader.Title,
                AtcId = _reader.AtcId,
                AtcModel = _reader.AtcModel,
                FuelTotalCapacity = _reader.FuelTotalCapacity,
                TimeStamp = DateTime.FromBinary(trackPoint.Time),
                Latitude = trackPoint.Latitude,
                Longitude = trackPoint.Longitude,
                Altitude = trackPoint.Altitude,
                AltitudeAboveGround = trackPoint.AltitudeAboveGround,
                Heading = trackPoint.Heading,
                Speed = trackPoint.Speed,
                SimOnGround = trackPoint.SimOnGround,
                AutopilotMaster = trackPoint.AutopilotMaster,
                FuelTotalQuantity = trackPoint.FuelTotalQuantity
            };
        }

        public void Close()
        {
            _writer.Close();
        }
    }
}