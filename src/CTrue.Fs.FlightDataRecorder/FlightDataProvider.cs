using System;
using System.Reactive.Subjects;
using System.Threading;
using CTrue.Fs.FlightData.Contracts;
using CTrue.FsConnect;
using CTrue.FsConnect.Managers;

namespace CTrue.Fs.FlightData.Provider
{
    public class FlightDataProvider : IFlightDataProvider
    {
        private static AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private FsConnect.FsConnect _fsConnect;

        public event EventHandler<AircraftDataReceivedEventArgs> AircraftDataReceived;

        public string HostName { get; set; }

        public uint Port { get; set; }

        public FlightDataProvider()
        {
        }

        public void Start()
        {
            _fsConnect = new FsConnect.FsConnect();
            _fsConnect.SimConnectFileLocation = SimConnectFileLocation.MyDocuments;
            _fsConnect.ConnectionChanged += (sender, args) =>
            {
                if (args)
                {
                    _fsConnect.RegisterDataDefinition<PlaneInfo>(FsDefinitions.AircraftInfo);
                    _resetEvent.Set();
                }
            };

            _fsConnect.Connect("FS Track Log", HostName, Port, SimConnectProtocol.Ipv4);

            bool success = _resetEvent.WaitOne(2000);

            if (!success)
            {
                Console.WriteLine($"Could not connect to {HostName}:{Port}.");
                return;
            }
            else
                Console.WriteLine("Connected to Flight Simulator");


            AircraftManager<PlaneInfo> aircraftManager =
                new AircraftManager<PlaneInfo>(_fsConnect, FsDefinitions.AircraftInfo, FsRequests.AircraftPeriodic);

            aircraftManager.RequestMethod = RequestMethod.Continuously;
            aircraftManager.Updated += (sender, args) =>
            {
                var aircraftInfo = ConvertToDataFormat(args.AircraftInfo);

                AircraftDataReceived?.Invoke(this, new AircraftDataReceivedEventArgs(aircraftInfo));
            };
        }

        private AircraftInfoV1 ConvertToDataFormat(PlaneInfo value)
        {
            return new AircraftInfoV1()
            {
                Title = value.Title,
                Category = value.Category,
                TimeStamp = GetDateTime(value.ZuluYear, value.ZuluDayOfYear, value.ZuluTime),
                Latitude = value.Latitude,
                Longitude = value.Longitude,
                Altitude = value.Altitude,
                AltitudeAboveGround = value.AltitudeAboveGround,
                Heading = value.Heading,
                Speed = value.Speed,
                SimOnGround = value.SimOnGround
            };
        }

        public void Stop()
        {
            _fsConnect.Disconnect();
            _fsConnect.Dispose();
            _fsConnect = null;
        }

        private static DateTime GetDateTime(ulong year, ulong dayInYear, ulong secondsInDay)
        {
            return new DateTime((int)year, 1, 1).AddDays(dayInYear).AddSeconds(secondsInDay);
        }
    }
}
