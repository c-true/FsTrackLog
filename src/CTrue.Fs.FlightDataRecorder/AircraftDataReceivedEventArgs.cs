using System;
using CTrue.Fs.FlightData.Contracts;

namespace CTrue.Fs.FlightData.Provider
{
    public class AircraftDataReceivedEventArgs : EventArgs
    {
        public AircraftInfoV1 AircraftInfo { get; }

        public AircraftDataReceivedEventArgs(AircraftInfoV1 aircraftInfo)
        {
            AircraftInfo = aircraftInfo;
        }
    }
}