using System;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using CTrue.Fs.FlightData.Contracts;
using CTrue.FsConnect;
using CTrue.FsConnect.Managers;
using Serilog;

namespace CTrue.Fs.FlightData.Provider
{
    public class FlightDataProvider : IFlightDataProvider
    {
        private static AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private FsConnect.FsConnect _fsConnect;
        private AircraftManager<PlaneInfo> _aircraftManager;
        private Timer _connectTimer;

        public event EventHandler<bool> ConnectionChanged;
        public event EventHandler<AircraftDataReceivedEventArgs> AircraftDataReceived;
        public event EventHandler Closed;

        public bool AutoConnect { get; set; }

        public string HostName { get; set; }

        public uint Port { get; set; }

        public bool Connected => _fsConnect?.Connected ?? false;

        public FlightDataProvider()
        {
        }

        public void Initialize()
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
                else
                {
                    Closed?.Invoke(this, EventArgs.Empty);
                }

                ConnectionChanged?.Invoke(this, _fsConnect.Connected);
            };
            
            _aircraftManager = new AircraftManager<PlaneInfo>(_fsConnect, FsDefinitions.AircraftInfo, FsRequests.AircraftPeriodic);

            _aircraftManager.RequestMethod = RequestMethod.Poll;
            _aircraftManager.Updated += (sender, args) =>
            {
                var aircraftInfo = ConvertToDataFormat(args.AircraftInfo);

                AircraftDataReceived?.Invoke(this, new AircraftDataReceivedEventArgs(aircraftInfo));
            };

            if (AutoConnect)
                _connectTimer = new Timer(OnConnectTimer, null, 1000, 0);
        }

        public void Connect()
        {
            if (_fsConnect == null)
                throw new ApplicationException("Can not connect. Flight Data provider is not initialized");

            if (_fsConnect.Connected) return;

            _fsConnect.Connect("FS Track Log", HostName, Port, SimConnectProtocol.Ipv4);

            bool success = _resetEvent.WaitOne(2000);
            _aircraftManager.RequestMethod = RequestMethod.Continuously;

            Log.Information("Connected to Flight Simulator");
        }

        public void Disconnect()
        {
            if (_fsConnect == null)
                throw new ApplicationException("Can not disconnect. Flight Data provider is not initialized");

            if (!_fsConnect.Connected) return;

            _fsConnect.Disconnect();
        }

        public void Start()
        {
            if (_fsConnect == null)
                throw new ApplicationException("Can not start Flight Data provider. Not initialized");

            if(!AutoConnect)    // Wait for connection timer if auto connecting
                Connect();
        }

        public void Stop()
        {
            if (_fsConnect == null) return;

            _aircraftManager.RequestMethod = RequestMethod.Poll;

            Disconnect();
        }


        private void OnConnectTimer(object state)
        {
            try
            {
                if (!_fsConnect.Connected)
                {
                   Log.Debug("Autoconnecting");

                    Connect();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not automatically connect to MSFS");
            }

            _connectTimer.Change(1000, 0);
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

        private static DateTime GetDateTime(ulong year, ulong dayInYear, ulong secondsInDay)
        {
            return new DateTime((int)year, 1, 1).AddDays(dayInYear).AddSeconds(secondsInDay);
        }

        public void Dispose()
        {
            _aircraftManager?.Dispose();
            _fsConnect?.Dispose();
        }
    }
}
