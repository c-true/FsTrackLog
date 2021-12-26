using System;

namespace CTrue.Fs.FlightData.Contracts
{
    public interface IFlightDataProvider : IDisposable
    {
        event EventHandler<bool> ConnectionChanged;
        event EventHandler<AircraftDataReceivedEventArgs> AircraftDataReceived;
        event EventHandler Closed;

        string HostName { get; set; }
        uint Port { get; set; }
        bool AutoConnect { get; set; }
        void Start();
        void Stop();
        void Initialize();
    }
}