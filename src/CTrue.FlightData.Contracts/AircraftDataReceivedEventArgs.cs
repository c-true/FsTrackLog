using System;

namespace CTrue.Fs.FlightData.Contracts
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