using System;
using CTrue.Fs.FlightData.Contracts;
using CTrue.FsTrackLog.Contracts;
using IFlightDataStore = CTrue.FsTrackLog.Core.File.IFlightDataStore;

namespace CTrue.FsTrackLog.Core
{
    public class FsTrackLog : IFsTrackLog
    {
        private readonly ITrackLogWriter _writer;

        public event EventHandler TrackLogUpdated;

        public string FsTrackLogId { get; }

        public string AircraftName { get; set; }

        public AircraftInfoV1 Value { get; set; }

        public FsTrackLog(ITrackLogWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            FsTrackLogId = Guid.NewGuid().ToString();
        }

        public void Write(AircraftInfoV1 value)
        {
            Value = value;

            _writer.Write(value);
            
            TrackLogUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            _writer.Close();
        }
    }
}