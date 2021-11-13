using System;
using System.ComponentModel;
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
        private Subject<AircraftInfo> _subject;
        private FlightDataProvider _provider;
        private FlightDataStore _store;
        private string _status;

        public ICommand ConnectCommand { get; }

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

        public MainWindowViewModel()
        {
            _subject = new Subject<AircraftInfo>();
            _provider = new FlightDataProvider(_subject);
            _store = new FlightDataStore();

            ConnectCommand = new DelegateCommand<object>(Connect, CanConnect);

            WriteSequenceToView(_subject);
        }

        private void Connect(object obj)
        {
            try
            {
                _provider.HostName = "192.168.1.174";
                _provider.Port = 57490;
                _provider.Start();

                Status = "Connected";
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

        void WriteSequenceToView(IObservable<AircraftInfo> sequence)
        {
            sequence.Subscribe(value =>
            {
                Status = $"({value.Latitude:F3}, {value.Longitude:F3}), Elev: {value.Altitude:F0}m, On ground: {value.SimOnGround}";
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