using System;
using System.Reactive.Linq;
using CTrue.Fs.FlightData.Contracts;
using IFlightDataStore = CTrue.FsTrackLog.Core.IFlightDataStore;

namespace CTrue.FsTrackLog.Core
{
    public interface IFsTrackLogManager
    {
        event EventHandler<bool> ConnectionChanged;
        event EventHandler<IFsTrackLog> CurrentTrackLogChanged;
        
        void Initialize(FsTrackLogConfig config);
        void Start();
        void Stop();
    }

    public class FsTrackLogManager : IFsTrackLogManager
    {
        private readonly IFlightDataProvider _provider;
        private readonly IFlightDataStore _store;
        private IFsTrackLog _currentTrackLog;

        private IObservable<AircraftInfoV1> _aircraftInfoObservable;

        public event EventHandler<bool> ConnectionChanged;

        public event EventHandler<IFsTrackLog> CurrentTrackLogChanged;

        public IFsTrackLog CurrentTrackLog => _currentTrackLog;

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

            _provider.Closed += (sender, args) => onClose();
            _provider.ConnectionChanged += (sender, b) => ConnectionChanged?.Invoke(this, b);

            _aircraftInfoObservable.Subscribe(onNext, onClose);

            _provider.AutoConnect = config.AutoConnect;
            _provider.Initialize();

            _provider.Start();
        }

        public void Start()
        {
            _provider.Start();
        }

        public void Stop()
        {
            _provider.Stop();
        }

        private void onNext(AircraftInfoV1 value)
        {
            if (_currentTrackLog == null)
            {
                _currentTrackLog = _store.CreateTrackLog();
                CurrentTrackLogChanged?.Invoke(this, _currentTrackLog);
            }

            _currentTrackLog.Write(value);
        }

        private void onClose()
        {
            _currentTrackLog.Close();
        }
    }
}
