﻿using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CTrue.Fs.FlightData.Contracts;
using CTrue.FsTrackLog.Core;
using FsTrackLogApp.Annotations;

namespace FsTrackLogApp
{
    public class TrackLogViewModel : INotifyPropertyChanged
    {
        private IFsTrackLog _model;

        private string _position;
        private string _altitude;
        private string _speed;
        private string _heading;
        private bool _simOnGround;
        private string _title;
        private string _timeStamp;

        public event PropertyChangedEventHandler PropertyChanged;

        public string TimeStamp
        {
            get => _timeStamp;
            set
            {
                if (value == _timeStamp) return;
                _timeStamp = value;
                OnPropertyChanged();
            }
        }

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

        public TrackLogViewModel()
        {
            ShowPosCommand = new DelegateCommand<object>(ShowPos, CanShowPos);
        }

        private bool CanShowPos(object arg)
        {
            return true;
        }

        private void ShowPos(object obj)
        {
            string lat = _model.Value.Latitude.ToString("F7", CultureInfo.InvariantCulture);
            string lon = _model.Value.Longitude.ToString("F7", CultureInfo.InvariantCulture);
            string url = $"https://www.google.com/maps/search/?api=1&query={lat}%2C{lon}";
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public void SetModel(IFsTrackLog tracklog)
        {
            _model = tracklog;

            UpdateViewModel();

            _model.TrackLogUpdated += (sender, args) =>
            {
                UpdateViewModel();
            };
        }

        private void UpdateViewModel()
        {
            TimeStamp = _model.Value.TimeStamp.ToString("s");
            Title = _model.Value.Title;
            Position = $"({_model.Value.Latitude.ToString("F3")}, {_model.Value.Longitude.ToString("F3")})";
            Altitude = $"{_model.Value.Altitude.ToString("F0")}ft ({_model.Value.AltitudeAboveGround.ToString("F0")}ft ag)";
            Heading = _model.Value.Heading.ToString("F0");
            Speed = _model.Value.Speed.ToString("F0");

            SimOnGround = _model.Value.SimOnGround;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}