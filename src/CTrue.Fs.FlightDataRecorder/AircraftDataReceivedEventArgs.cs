using System;
using CTrue.Fs.FlightData.Contracts;

namespace CTrue.Fs.FlightData.Provider
{
    public class AircraftDataReceivedEventArgs : EventArgs
    {
        public AircraftInfo AircraftInfo { get; }

        public AircraftDataReceivedEventArgs(AircraftInfo aircraftInfo)
        {
            AircraftInfo = aircraftInfo;
        }
    }
}