using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using CTrue.FsConnect;
using CTrue.FsConnect.Managers;
using Microsoft.FlightSimulator.SimConnect;
using SpatialLite.Gps.Geometries;
using SpatialLite.Gps.IO;

namespace FsTrackLog
{
    class Program
    {
        private static AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private static FileStream _fileStream;
        private static string _fileName;
        private static bool _displayCharStar = true;
        private static int _sampleCount = 0;

        private const byte FSTRACKLOG_BINARY_VERSION = 0x01;
        private const int SIZE_AIRCRAFT_INFO = 56; // Size of aircraft binary data, not including the version header

        static void Main(string[] args)
        {
            Parser parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);

            parserResult
                .WithParsed(Run)
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }

        private static void Run(Options options)
        {
            Subject<AircraftInfo> _subject = new Subject<AircraftInfo>();

            try
            {
                if (!string.IsNullOrEmpty(options.Directory))
                {
                    DirectoryInfo di = new DirectoryInfo(options.Directory);

                    if (!di.Exists)
                        throw new Exception("Could not find directory: " + options.Directory);

                    _fileName = Path.Combine(options.Directory, GetFileName());
                    _fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write);
                    _fileStream.Write(new []{FSTRACKLOG_BINARY_VERSION}, 0, 1);
                    Console.WriteLine($"Writing binary Track Log to {_fileName}");
                }
                else if (!string.IsNullOrEmpty(options.FileName))
                {
                    ConvertBinaryToGpx(options.FileName);
                    return;
                }

                if(options.Verbose)
                    WriteSequenceToConsole(_subject);

                WriteSequenceToFile(_subject, options);


                FsConnect fsConnect = new FsConnect();
                fsConnect.SimConnectFileLocation = SimConnectFileLocation.MyDocuments;
                fsConnect.ConnectionChanged += (sender, args) =>
                {
                    if(args)
                    {
                        InitializeFlightSimulator(fsConnect);
                        _resetEvent.Set();
                    }
                };

                fsConnect.Connect("FS Track Log", options.Hostname, options.Port, SimConnectProtocol.Ipv4);

                bool success = _resetEvent.WaitOne(2000);

                if (!success)
                {
                    Console.WriteLine($"Could not connect to {options.Hostname}:{options.Port}.");
                    return;
                }
                else
                    Console.WriteLine("Connected to Flight Simulator");

                AircraftManager<AircraftInfo> aircraftManager =
                    new AircraftManager<AircraftInfo>(fsConnect, FsDefinitions.AircraftInfo, FsRequests.AircraftPeriodic);
                aircraftManager.Updated += (sender, args) => { _subject.OnNext(args.AircraftInfo); };
                aircraftManager.RequestMethod = RequestMethod.Continuously;

                ConsoleKeyInfo cki;

                Console.WriteLine("Press ESC to quit or any other key for status.");

                do
                {
                    if(_sampleCount > 0)
                        Console.WriteLine($"\r{_sampleCount}");

                    cki = Console.ReadKey();
                } while (cki.Key != ConsoleKey.Escape);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message, e);    
            }
            
            _subject.OnCompleted();
        }

        static void WriteSequenceToConsole(IObservable<AircraftInfo> sequence)
        {
            sequence.Subscribe(value =>
            {
                Console.WriteLine($"({value.Latitude:F3}, {value.Longitude:F3}), Elev: {value.Altitude:F0}m, On ground: {value.SimOnGround}");
            }, () =>
            {
                Console.WriteLine("Completed");
            });
        }

        static void WriteSequenceToFile(IObservable<AircraftInfo> sequence, Options options)
        {
            sequence.Subscribe(value =>
            {
                if (value.SimOnGround) return;

                if(!options.Verbose)
                {
                    Console.Write("\r{0}", _displayCharStar ? "+" : "-");
                    _displayCharStar = !_displayCharStar;
                }

                byte[] byteArray = GetAircraftInfoBytes(value);
                _fileStream.Write(byteArray, 0, byteArray.Length);
                _sampleCount++;
            }, () =>
            {
                Console.WriteLine($"\rCompleted. {_sampleCount} track points written to {_fileName}");
                _fileStream?.Flush(true);
                _fileStream?.Close(); 

                if(_fileStream != null && options.GenerateGpx)
                    ConvertBinaryToGpx(_fileStream.Name);
            });
        }

        private static byte[] GetAircraftInfoBytes(AircraftInfo value)
        {
            List<byte> byteArray = new List<byte>();

            DateTime utcTime = GetDateTime(value.ZuluYear, value.ZuluDayOfYear, value.ZuluTime);
            byteArray.AddRange(BitConverter.GetBytes(utcTime.ToBinary()));
            byteArray.AddRange(BitConverter.GetBytes(value.Latitude));
            byteArray.AddRange(BitConverter.GetBytes(value.Longitude));
            byteArray.AddRange(BitConverter.GetBytes(value.Altitude));
            byteArray.AddRange(BitConverter.GetBytes(value.AltitudeAboveGround));
            byteArray.AddRange(BitConverter.GetBytes(value.Heading));
            byteArray.AddRange(BitConverter.GetBytes(value.Speed));

            return byteArray.ToArray();
        }

        private static void ConvertBinaryToGpx(string fileName)
        {
            Console.WriteLine($"Converting binary Track Log to GPX.");

            GpxWriter gpxWriter = null;
            string gpxFileName = null;
            int numberOfTrackPoint = 0;

            try
            {
                gpxFileName =  Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".gpx");
                gpxWriter = new SpatialLite.Gps.IO.GpxWriter(new FileStream(gpxFileName, FileMode.Create, FileAccess.Write), new GpxWriterSettings()
                {
                    GeneratorName = "FS TrackLog",
                    IsReadOnly = false,
                    WriteMetadata = false
                });

                gpxWriter.WriteTrackStart();

                bool quit = false;
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                int fsTrackLogBinaryVersion = fileStream.ReadByte();
                Console.WriteLine("Reading binary file, version: " + fsTrackLogBinaryVersion);
                
                if(fsTrackLogBinaryVersion != FSTRACKLOG_BINARY_VERSION)
                    Console.WriteLine("Warning: Not a current version, there may be issues reading this older file format");

                byte[] aircraftInfoBuffer = new byte[SIZE_AIRCRAFT_INFO];
                while (!quit)
                {
                    int res = fileStream.Read(aircraftInfoBuffer, 0, SIZE_AIRCRAFT_INFO);

                    if (res == SIZE_AIRCRAFT_INFO)
                    {
                        AircraftInfo aircraftInfo = new AircraftInfo();

                        long dateData = BitConverter.ToInt64(aircraftInfoBuffer, 0);
                        aircraftInfo.Latitude = BitConverter.ToDouble(aircraftInfoBuffer, 8);
                        aircraftInfo.Longitude = BitConverter.ToDouble(aircraftInfoBuffer, 16);
                        aircraftInfo.Altitude = BitConverter.ToDouble(aircraftInfoBuffer, 24);
                        aircraftInfo.AltitudeAboveGround = BitConverter.ToDouble(aircraftInfoBuffer, 32);
                        aircraftInfo.Heading = BitConverter.ToDouble(aircraftInfoBuffer, 40);
                        aircraftInfo.Speed = BitConverter.ToDouble(aircraftInfoBuffer, 48);

                        DateTime utcTime = DateTime.FromBinary(dateData);
                        Console.WriteLine($"{utcTime} - {aircraftInfo.Latitude:F6} - {aircraftInfo.Longitude:F6} - {aircraftInfo.Altitude:F2} - {aircraftInfo.AltitudeAboveGround:F2}");
                        
                        gpxWriter.WriteTrackPoint(new GpxPoint(aircraftInfo.Longitude, aircraftInfo.Latitude, aircraftInfo.Altitude, utcTime));
                        numberOfTrackPoint++;
                    }
                    else
                        quit = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while writing binary track point to GPX file: " + e);
            }

            try
            {
                if(gpxWriter != null)
                {
                    gpxWriter.WriteTrackEnd();
                    gpxWriter.Dispose();
                    if(!string.IsNullOrEmpty(gpxFileName))
                        Console.WriteLine($"{numberOfTrackPoint} track points written to GPX file at '{gpxFileName}'.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not close GPX file. It is probably invalid. Message: " + e.Message);
            }
        }

        private static string GetFileName()
        {
            return $"FsTrackLog_{DateTime.Now.ToString("yyyyMMddhhmmss")}.fst";
        }

        private static DateTime GetDateTime(ulong year, ulong dayInYear, ulong secondsInDay)
        {
            return new DateTime((int)year, 1, 1).AddDays(dayInYear).AddSeconds(secondsInDay);
        }

        private static void InitializeFlightSimulator(FsConnect fsConnect)
        {
            List<SimProperty> definition = new List<SimProperty>();

            definition.Add(new SimProperty(FsSimVar.ZuluYear, FsUnit.Number, SIMCONNECT_DATATYPE.INT64));
            definition.Add(new SimProperty(FsSimVar.ZuluDayOfYear, FsUnit.Number, SIMCONNECT_DATATYPE.INT64));
            definition.Add(new SimProperty(FsSimVar.ZuluTime, FsUnit.Seconds, SIMCONNECT_DATATYPE.INT64));
            definition.Add(new SimProperty(FsSimVar.PlaneLatitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty(FsSimVar.PlaneLongitude, FsUnit.Degree, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty(FsSimVar.PlaneAltitudeAboveGround, FsUnit.Meter, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty(FsSimVar.PlaneAltitude, FsUnit.Meter, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty(FsSimVar.PlaneHeadingDegreesTrue, FsUnit.Degrees, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty(FsSimVar.AirspeedTrue, FsUnit.MeterPerSecond, SIMCONNECT_DATATYPE.FLOAT64));
            definition.Add(new SimProperty(FsSimVar.SimOnGround, FsUnit.Boolean, SIMCONNECT_DATATYPE.INT32));

            fsConnect.RegisterDataDefinition<AircraftInfo>(FsDefinitions.AircraftInfo, definition);
        }

        static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            HelpText helpText;
            if (errs.IsVersion())  //check if error is version request
                helpText = HelpText.AutoBuild(result);
            else
            {
                helpText = HelpText.AutoBuild(result, h =>
                {
                    //configure help
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = "Flight Simulator Track Log";
                    h.Copyright = "";
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
            }
            Console.WriteLine(helpText);
        }
    }
}
