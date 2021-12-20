using System;
using System.Reactive.Linq;
using CTrue.Fs.FlightData.Contracts;
using CTrue.Fs.FlightData.Store;

namespace CTrue.FsTrackLog.Core
{
    public class FsTrackLogManager
    {
        private readonly IFlightDataProvider _provider;
        private readonly IFlightDataStore _store;

        private IObservable<AircraftInfoV1> _aircraftInfoObservable;

        public FsTrackLogManager(IFlightDataProvider provider, IFlightDataStore store)
        {
            _provider = provider;
            _store = store;
        }

        public void Initialize(FsTrackLogConfig config)
        {
            _provider.HostName = config.HostName;
            _provider.Port = config.Port;
            
            _store.Initialize(config.StorePath);

            _aircraftInfoObservable = Observable.FromEventPattern<EventHandler<AircraftDataReceivedEventArgs>, AircraftDataReceivedEventArgs>(
                    h => _provider.AircraftDataReceived += h,
                    h => _provider.AircraftDataReceived -= h)
                .Select(k => k.EventArgs.AircraftInfo);

            _aircraftInfoObservable.Subscribe(_store.Write, _store.Close);

            if (config.AutoConnect)
            {
                _provider.Start();
            }
        }

        public void Start()
        {
            _provider.Start();
        }

        public void Stop()
        {
            _provider.Stop();
        }
    }
}
