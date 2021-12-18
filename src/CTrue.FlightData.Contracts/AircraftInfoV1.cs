using System;

namespace CTrue.Fs.FlightData.Contracts
{
    public struct AircraftInfoV1
    {
        public string Title;
        public string Category;
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