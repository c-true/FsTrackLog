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
        private static FsTrackLogger _trackLogger;
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

                    _trackLogger = new FsTrackLogger(di.FullName);
                    Console.WriteLine($"Writing binary Track Log to {_trackLogger.FileName}");
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

        private static void ConvertBinaryToGpx(string fileName)
        {
            try
            {
                FsGpxWriter gpxWriter = new FsGpxWriter();
                string gpxFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".gpx");
            
                gpxWriter.ConvertBinaryToGpx(fileName, gpxFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while creating GPX file: " + e.Message + "\n\n" + e.ToString());
            }
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

                _trackLogger.LogTrackPoint(value);
                _sampleCount++;
            }, () =>
            {
                Console.WriteLine($"\rCompleted. {_sampleCount} track points written to {_fileName}");
                _trackLogger.Close();

                if(_trackLogger != null && options.GenerateGpx)
                    ConvertBinaryToGpx(_trackLogger.FileName);
            });
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
                    h.Heading = "Flight Simulator Track Log v1.0.1";
                    h.Copyright = "";
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
            }
            Console.WriteLine(helpText);
        }
    }
}
