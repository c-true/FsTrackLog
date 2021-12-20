using System;

namespace CTrue.Fs.FlightData.Contracts
{
    public interface IFlightDataProvider : IDisposable
    {
        event EventHandler<AircraftDataReceivedEventArgs> AircraftDataReceived;

        string HostName { get; set; }
        uint Port { get; set; }
        void Start();
        void Stop();
    }
}