using System;
using CTrue.Fs.FlightData.Contracts;

namespace CTrue.FsTrackLog.Core
{
    public interface IFsTrackLog
    {
        event EventHandler TrackLogUpdated;

        AircraftInfoV1 Value { get; set; }
        void Write(AircraftInfoV1 value);
        void Close();
    }
}