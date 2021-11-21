﻿using System.Runtime.InteropServices;
using CTrue.FsConnect;
using Microsoft.FlightSimulator.SimConnect;

namespace CTrue.Fs.FlightData.Provider
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Title;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Category;
        [SimVar(NameId = FsSimVar.ZuluYear, UnitId = FsUnit.Number, DataType = SIMCONNECT_DATATYPE.INT64)]
        public ulong ZuluYear;
        [SimVar(NameId = FsSimVar.ZuluDayOfYear, UnitId = FsUnit.Number, DataType = SIMCONNECT_DATATYPE.INT64)]
        public ulong ZuluDayOfYear;
        [SimVar(NameId = FsSimVar.ZuluTime, UnitId = FsUnit.Second, DataType = SIMCONNECT_DATATYPE.INT64)]
        public ulong ZuluTime;
        [SimVar(NameId = FsSimVar.PlaneLatitude, UnitId = FsUnit.Degree)]
        public double Latitude;
        [SimVar(NameId = FsSimVar.PlaneLongitude, UnitId = FsUnit.Degree)]
        public double Longitude;
        [SimVar(NameId = FsSimVar.PlaneAltitudeAboveGround, UnitId = FsUnit.Feet)]
        public double AltitudeAboveGround;
        [SimVar(NameId = FsSimVar.PlaneAltitude, UnitId = FsUnit.Feet)]
        public double Altitude;
        [SimVar(NameId = FsSimVar.PlaneHeadingDegreesTrue, UnitId = FsUnit.Degree)]
        public double Heading;
        [SimVar(NameId = FsSimVar.AirspeedTrue, UnitId = FsUnit.Knot)]
        public double Speed;

        public bool SimOnGround;
    }
}