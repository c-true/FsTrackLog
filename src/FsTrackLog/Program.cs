using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using CTrue.Fs.FlightData.Contracts;
using CTrue.Fs.FlightData.Provider;
using CTrue.FsTrackLog.Core.File;
using CTrue.FsTrackLog.Core.Gpx;

namespace FsTrackLog
{
    class Program
    {
        private static int _sampleCount = 0;

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
            IObservable<AircraftInfo> _aircraftInfoObservable;
            FlightDataProvider _provider = new FlightDataProvider();
            FlightDataStore _store = new FlightDataStore();

            try
            {
                if (!string.IsNullOrEmpty(options.Directory))
                {
                    _store.Initialize(options.Directory);
                }
                else if (!string.IsNullOrEmpty(options.FileName))
                {
                    ConvertBinaryToGpx(options.FileName);

                    return;
                }

                //
                // Set up subscribers
                //


                _aircraftInfoObservable = Observable.FromEventPattern<EventHandler<AircraftDataReceivedEventArgs>, AircraftDataReceivedEventArgs>(
                        h => _provider.AircraftDataReceived += h,
                        h => _provider.AircraftDataReceived -= h)
                    .Select(k => k.EventArgs.AircraftInfo);

                if (options.Verbose)
                    WriteSequenceToConsole(_aircraftInfoObservable);

                var trackLog = _store.CreateTrackLog();
                var disposable = _aircraftInfoObservable.Subscribe(trackLog.Write, trackLog.Close);

                //
                // Start Flight Data Provider
                // 

                _provider.HostName = options.Hostname;
                _provider.Port = options.Port;

                _provider.Initialize();
                _provider.Start();

                ConsoleKeyInfo cki;

                Console.WriteLine("Press ESC to quit or any other key for status.");

                do
                {
                    if(_sampleCount > 0)
                        Console.WriteLine($"\r{_sampleCount}");

                    cki = Console.ReadKey();
                } while (cki.Key != ConsoleKey.Escape);

                // Event pattern on Observable does not close observable, so must close tracklog explicitly
                trackLog.Close();
                disposable.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message, e);    
            }

            if (options.GenerateGpx)
            {
                DirectoryInfo di = new DirectoryInfo(options.Directory);
                foreach (var fi in di.EnumerateFiles("*.fst"))
                {
                    try
                    {
                        string baseName = Path.GetFileNameWithoutExtension(fi.FullName);
                        string gpxFileName = Path.Combine(fi.DirectoryName, baseName + ".gpx");

                        if(!File.Exists(gpxFileName))
                            ConvertBinaryToGpx(fi.FullName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Could not convert file: '{fi.FullName}': " + e.Message);
                    }
                }
            }
        }

        static void WriteSequenceToConsole(IObservable<AircraftInfo> sequence)
        {
            sequence.Subscribe(value =>
            {
                Console.WriteLine($"({value.Latitude:F3}, {value.Longitude:F3}), Elev: {value.Altitude:F0}m / {value.Altitude*3.2808399d:F0}ft, On ground: {value.SimOnGround}");
            }, () =>
            {
                Console.WriteLine("Completed");
            });
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
                    h.Heading = "Flight Simulator Track Log v" + GetAssemblyVersion();
                    h.Copyright = "";
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
            }

            Console.WriteLine(helpText);
        }

        private static string GetAssemblyVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}
