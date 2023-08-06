using System;
using CTrue.Fs.FlightData.Contracts;

namespace CTrue.FsTrackLog.Core
{
    public interface IFsTrackLog
    {
        event EventHandler TrackLogUpdated;

        AircraftInfo Value { get; set; }
        void Write(AircraftInfo value);
        void Close();
    }
}