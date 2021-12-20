using System;

namespace CTrue.Fs.FlightData.Contracts
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