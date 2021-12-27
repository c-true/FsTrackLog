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
using CTrue.FsTrackLog.Core;
using FsTrackLogApp.Annotations;
using Serilog;

namespace FsTrackLogApp
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private IFsTrackLogManager _trackLogManager;

        private bool _connected = false;
        private bool _started = false;

        private string _status;
        private string _connectionStatusText = "DISCONNECTED";
        private string _startStopButtonText = "START";
        private string _connectButtonText;
        private AircraftInfoViewModel _aircraftInfo;

        public ICommand ConnectCommand { get; }
        public ICommand StartStopCommand { get; }

        public string ConnectionStatusText
        {
            get => _connectionStatusText;
            set
            {
                if (value == _connectionStatusText) return;
                _connectionStatusText = value;
                OnPropertyChanged();
            }
        }

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
            try
            {
                _trackLogManager = new FsTrackLogManager(new FlightDataProvider(), new FlightDataStore());
                _trackLogManager.ConnectionChanged += HandleConnectionChanged;

                var config = new FsTrackLogConfig()
                {
                    HostName = "192.168.1.174",
                    Port = 500,
                    AutoConnect = true,
                    AutoLog = true,
                    StorePath = "c:\\temp\\FsTrackLog"
                };

                _trackLogManager.Initialize(config);
            }
            catch (Exception e)
            {
                Log.Error("Could not initialize track log manager.", e);
            }

            AircraftInfo = new AircraftInfoViewModel();

            ConnectButtonText = "CONNECT";
            StartStopButtonText = "START";
            ConnectCommand = new DelegateCommand<object>(Connect, CanConnect);
            StartStopCommand = new DelegateCommand<object>(StartStop, CanStartStop);

            WriteSequenceToView(_trackLogManager.AircraftInfoObservable);
        }

        private void HandleConnectionChanged(object? sender, bool connectionStatus)
        {
            _connected = connectionStatus;

            ConnectionStatusText = connectionStatus ? "CONNECTED" : "DISCONNECTED";
            ConnectButtonText = connectionStatus ? "DISCONNECTED" : "CONNECT";
            
            ((DelegateCommand<object>)StartStopCommand).RaiseCanExecuteChanged();
        }


        private void Connect(object obj)
        {
            try
            {
                if (_connected)
                {
                    _trackLogManager.Stop();
                }
                else
                {
                    _trackLogManager.Start();
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

                _started = true;
            }
            else
            {
                StartStopButtonText = "START";

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