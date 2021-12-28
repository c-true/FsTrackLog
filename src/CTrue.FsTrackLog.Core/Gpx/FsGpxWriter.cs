using System;
using System.IO;
using CTrue.FsTrackLog.Core.File;
using CTrue.FsTrackLog.Core.File.Generated.v1;
using SpatialLite.Gps.Geometries;
using SpatialLite.Gps.IO;

namespace CTrue.FsTrackLog.Core.Gpx
{
    public class FsGpxWriter
    {
        public void ConvertBinaryToGpx(string fileName, string gpxFileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            FileStream gpxFileStream = new FileStream(gpxFileName, FileMode.Create, FileAccess.Write);

            ConvertBinaryToGpx(fileStream, gpxFileStream);

            fileStream.Close();
            gpxFileStream.Close();
        }

        public void ConvertBinaryToGpx(Stream sourceStream, Stream targetStream)
        {
            Console.WriteLine($"Converting binary Track Log to GPX.");

            GpxWriter gpxWriter = null;
            string gpxFileName = null;
            int numberOfTrackPoint = 0;

            try
            {
                gpxWriter = new SpatialLite.Gps.IO.GpxWriter(targetStream, new GpxWriterSettings()
                {
                    GeneratorName = "FS TrackLog",
                    IsReadOnly = false,
                    WriteMetadata = false
                });

                gpxWriter.WriteTrackStart();

                bool quit = false;

                FsTrackLoggerReader reader = new FsTrackLoggerReader(sourceStream);
                
                Console.WriteLine("Reading binary file, version: " + reader.Version);

                while (!quit)
                {
                    FsTrackPoint fsTrackPoint = reader.ReadNext();

                    if (fsTrackPoint != null)
                    {
                        DateTime utcTime = DateTime.FromBinary(fsTrackPoint.Time);
                        Console.WriteLine($"{utcTime} - {fsTrackPoint.Latitude:F6} - {fsTrackPoint.Longitude:F6} - {fsTrackPoint.Altitude:F2} - {fsTrackPoint.AltitudeAboveGround:F2}");

                        gpxWriter.WriteTrackPoint(new GpxPoint(fsTrackPoint.Longitude, fsTrackPoint.Latitude, fsTrackPoint.Altitude, utcTime));
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
                if (gpxWriter != null)
                {
                    gpxWriter.WriteTrackEnd();
                    gpxWriter.Dispose();
                    if (!string.IsNullOrEmpty(gpxFileName))
                        Console.WriteLine($"{numberOfTrackPoint} track points written to GPX file at '{gpxFileName}'.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not close GPX file. It is probably invalid. Message: " + e.Message);
            }
        }

    }
}