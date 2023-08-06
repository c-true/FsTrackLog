using System.IO;
using CTrue.FsTrackLog.Core.Gpx;
using CTrue.FsTrackLog.Test.Resources;
using NUnit.Framework;
using SpatialLite.Gps.Geometries;
using SpatialLite.Gps.IO;

namespace CTrue.FsTrackLog.Test
{
    [TestFixture]
    public class FsGpxWriterTest
    {
        [Test]
        public void Test()
        {
            // Arrange
            byte[] testData = TestResource.FsTrackLog_20210404123943_v1;
            MemoryStream sourceStream = new MemoryStream(testData);

            MemoryStream targetStream = new MemoryStream();

            FsGpxWriter gpxWriter = new FsGpxWriter();

            // Act
            gpxWriter.ConvertBinaryToGpx(sourceStream, targetStream);

            // Assert
            Assert.That(targetStream, Is.Not.Null);

            MemoryStream ms = new MemoryStream(targetStream.GetBuffer());

            GpxReader gpxReader = new GpxReader(ms, new GpxReaderSettings()
            {
                ReadMetadata = true
            });

            var gpxGeometry = gpxReader.Read();

            Assert.That(gpxGeometry, Is.TypeOf<GpxTrack>());
            GpxTrack track = gpxGeometry as GpxTrack;
            GpxTrackSegment segment = track.Geometries[0];
            Assert.That(segment.Points.Count, Is.EqualTo(134));
        }
    }
}