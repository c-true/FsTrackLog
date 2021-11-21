using System.ComponentModel;
using System.Runtime.CompilerServices;
using CTrue.Fs.FlightData.Contracts;
using FsTrackLogApp.Annotations;

namespace FsTrackLogApp
{
    public class AircraftInfoViewModel : INotifyPropertyChanged
    {
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

        public AircraftInfoViewModel()
        {

        }

        public void SetModel(AircraftInfo value)
        {
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