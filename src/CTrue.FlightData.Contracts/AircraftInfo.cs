using System;

namespace CTrue.Fs.FlightData.Contracts
{
    public struct AircraftInfo
    {
        public DateTime TimeStamp;
        public double Latitude;
        public double Longitude;
        public double AltitudeAboveGround;
        public double Altitude;
        public double Heading;
        public double Speed;

        public bool SimOnGround;
    }
}