using System;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CTrue.Fs.FlightData.Contracts;
using CTrue.Fs.FlightData.Provider;
using CTrue.Fs.FlightData.Store;
using FsTrackLogApp.Annotations;

namespace FsTrackLogApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private FlightDataProvider _provider;
        private FlightDataStore _store;
        private IObservable<AircraftInfoV1> _aircraftInfoObservable;

        private bool _connected = false;
        private bool _started = false;

        private string _status;
        private string _startStopButtonText = "START";
        private string _connectButtonText;
        private AircraftInfoViewModel _aircraftInfo;

        public ICommand ConnectCommand { get; }
        public ICommand StartStopCommand { get; }

        public string StartStopButtonText
        {
            get => _startStopButtonText;
            set
            {
                if (value == _startStopButtonText) return;
                _startStopButtonText = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public string ConnectButtonText
        {
            get => _connectButtonText;
            set
            {
                if (value == _connectButtonText) return;
                _connectButtonText = value;
                OnPropertyChanged();
            }
        }

        public AircraftInfoViewModel AircraftInfo
        {
            get => _aircraftInfo;
            set
            {
                _aircraftInfo = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            _provider = new FlightDataProvider();
            _store = new FlightDataStore();

            AircraftInfo = new AircraftInfoViewModel();

            ConnectButtonText = "CONNECT";
            StartStopButtonText = "START";
            ConnectCommand = new DelegateCommand<object>(Connect, CanConnect);
            StartStopCommand = new DelegateCommand<object>(StartStop, CanStartStop);

            _aircraftInfoObservable = Observable.FromEventPattern<EventHandler<AircraftDataReceivedEventArgs>, AircraftDataReceivedEventArgs>(
                    h => _provider.AircraftDataReceived += h,
                    h => _provider.AircraftDataReceived -= h)
                .Select(k => k.EventArgs.AircraftInfo);

            WriteSequenceToView(_aircraftInfoObservable);
        }

        private void Connect(object obj)
        {
            try
            {
                if (_connected)
                {
                    ConnectButtonText = "CONNECT";
                    _provider.Stop();
                    _connected = false;
                }
                else
                {
                    _provider.HostName = "192.168.1.174";
                    _provider.Port = 57490;
                    _provider.Start();

                    Status = "Connected";
                    ConnectButtonText = "DISCONNECT";
                    _connected = true;
                    ((DelegateCommand<object>)StartStopCommand).RaiseCanExecuteChanged();
                }
            }
            catch (Exception e)
            {
                Status = "Failed: " + e.Message;
            }
        }

        private bool CanConnect(object arg)
        {
            return true;
        }

        private void StartStop(object obj)
        {
            if (!_started)
            {
                StartStopButtonText = "STOP";
                _store.Initialize("c:\\temp\\FsTrackLog");
                _aircraftInfoObservable.Subscribe(_store.Write, _store.Close);
                _started = true;
            }
            else
            {
                StartStopButtonText = "START";
                _store.Close();
                _started = false;
            }
        }

        private bool CanStartStop(object arg)
        {
            return _connected;
        }

        void WriteSequenceToView(IObservable<AircraftInfoV1> sequence)
        {
            sequence.Sample(TimeSpan.FromSeconds(3)).Subscribe(value =>
            {
                AircraftInfo.SetModel(value);
            }, () =>
            {
                Status = "Completed";
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}