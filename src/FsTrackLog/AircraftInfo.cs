using System;
using System.Runtime.InteropServices;

namespace FsTrackLog
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct AircraftInfo
    {
        public ulong ZuluYear;
        public ulong ZuluDayOfYear;
        public ulong ZuluTime;
        public double Latitude;
        public double Longitude;
        public double AltitudeAboveGround;
        public double Altitude;
        public double Heading;
        public double Speed;
        public bool SimOnGround;
    }
}