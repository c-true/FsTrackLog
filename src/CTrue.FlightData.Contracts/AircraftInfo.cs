using System;

namespace CTrue.Fs.FlightData.Contracts
{
    public struct AircraftInfo
    {
        public string Title;
        public string AtcId;
        public string AtcModel;
        public DateTime TimeStamp;
        public double Latitude;
        public double Longitude;
        public double AltitudeAboveGround;
        public double Altitude;
        public double Heading;
        public double Speed;

        public bool SimOnGround;
        public bool AutopilotMaster;
        public double FuelTotalQuantity;
        public double FuelTotalCapacity;
    }
}