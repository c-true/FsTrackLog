using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CTrue.Fs.FlightData.Contracts;
using FsTrackLogApp.Annotations;

namespace FsTrackLogApp
{
    public class AircraftInfoViewModel : INotifyPropertyChanged
    {
        private AircraftInfoV1 _model;

        private string _position;
        private string _altitude;
        private string _speed;
        private string _heading;
        private bool _simOnGround;
        private string _title;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Position
        {
            get => _position;
            set
            {
                if (value.Equals(_position)) return;
                _position = value;
                OnPropertyChanged();
            }
        }
        
        public string Altitude
        {
            get => _altitude;
            set
            {
                if (value.Equals(_altitude)) return;
                _altitude = value;
                OnPropertyChanged();
            }
        }
        
        public string Heading
        {
            get => _heading;
            set
            {
                if (value.Equals(_heading)) return;
                _heading = value;
                OnPropertyChanged();
            }
        }

        public string Speed
        {
            get => _speed;
            set
            {
                if (value == _speed) return;
                _speed = value;
                OnPropertyChanged();
            }
        }

        public bool SimOnGround
        {
            get => _simOnGround;
            set
            {
                if (value == _simOnGround) return;
                _simOnGround = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowPosCommand { get; }

        public AircraftInfoViewModel()
        {
            ShowPosCommand = new DelegateCommand<object>(ShowPos, CanShowPos);
        }

        private bool CanShowPos(object arg)
        {
            return true;
        }

        private void ShowPos(object obj)
        {
            string lat = _model.Latitude.ToString("F7", CultureInfo.InvariantCulture);
            string lon = _model.Longitude.ToString("F7", CultureInfo.InvariantCulture);
            string url = $"https://www.google.com/maps/search/?api=1&query={lat}%2C{lon}";
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public void SetModel(AircraftInfoV1 value)
        {
            _model = value;

            Title = value.Title;
            Position = $"({value.Latitude.ToString("F3")}, {value.Longitude.ToString("F3")})";
            Altitude = $"{value.Altitude.ToString("F0")}ft ({value.AltitudeAboveGround.ToString("F0")}ft ag)";
            Heading = value.Heading.ToString("F0");
            Speed = value.Speed.ToString("F0");
            
            SimOnGround = value.SimOnGround;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}