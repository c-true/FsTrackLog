using System;
using CTrue.Fs.FlightData.Contracts;
using CTrue.Fs.FlightData.Store;

namespace CTrue.FsTrackLog.Core
{
    public interface IFsTrackLog
    {
        event EventHandler TrackLogUpdated;
    }

    public class FsTrackLog : IFsTrackLog
    {
        private readonly IFlightDataStore _store;

        public event EventHandler TrackLogUpdated;

        public string FsTrackLogId { get; }

        public string AircraftName { get; set; }

        public AircraftInfoV1 LastValue { get; set; }

        public FsTrackLog(IFlightDataStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            FsTrackLogId = Guid.NewGuid().ToString();
        }

        public void Write(AircraftInfoV1 value)
        {
            LastValue = value;

            _store.Write(value);
            
            TrackLogUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            _store.Close();
        }
    }
}