using System;
using System.IO;
using CTrue.Fs.FlightData.Contracts;
using CTrue.Fs.FlightData.Store;
using CTrue.FsTrackLog.Test.Resources;
using FsTrackLog;
using NUnit.Framework;

namespace CTrue.FsTrackLog.Test
{
    [TestFixture]
    public class FsTrackLogTest
    {
        [Test]
        public void Write_read_test()
        {
            // Arrange
            // Act
            MemoryStream ms = new MemoryStream();

            FsTrackLogger writer = new FsTrackLogger(ms);

            writer.LogTrackPoint(new AircraftInfoV1()
            {
                Latitude = 60,
                Longitude = 10,
                Altitude = 100,
                AltitudeAboveGround = 50,
                Heading = 10,
                Speed = 20,
                SimOnGround = false,
                TimeStamp = new DateTime(2021, 1, 11, 0, 16, 40)
            });

            //writer.Close();

            ms.Position = 0;

            FsTrackLoggerReader reader = new FsTrackLoggerReader(ms);

            var tp = reader.ReadNext();

            // Assert
            
            Assert.That(DateTime.FromBinary(tp.Time), Is.EqualTo(new DateTime(2021, 1, 11, 0, 16, 40)));
            Assert.That(tp, Is.Not.Null);
            Assert.That(tp.Latitude, Is.EqualTo(60));
            Assert.That(tp.Longitude, Is.EqualTo(10));
            Assert.That(tp.Altitude, Is.EqualTo(100));
            Assert.That(tp.AltitudeAboveGround, Is.EqualTo(50));
            Assert.That(tp.Heading, Is.EqualTo(10));
            Assert.That(tp.Speed, Is.EqualTo(20));

            var tp2 = reader.ReadNext();
            Assert.That(tp2, Is.Null);
        }

        [Test]
        public void ReadFile()
        {
            byte[] testData = TestResource.FsTrackLog_20210404123943_v1;

            MemoryStream ms = new MemoryStream(testData);

            FsTrackLoggerReader reader = new FsTrackLoggerReader(ms);

            Assert.That(reader.Version, Is.EqualTo(1));

            var tp = reader.ReadNext();

            Assert.That(DateTime.FromBinary(tp.Time), Is.EqualTo(new DateTime(2021, 4, 5, 10, 39, 43)));
            Assert.That(tp.Latitude, Is.EqualTo(60.366496133605395d));
            Assert.That(tp.Longitude, Is.EqualTo(11.109738920632633d));
            Assert.That(tp.Altitude, Is.EqualTo(950.71809014709243d));
            Assert.That(tp.AltitudeAboveGround, Is.EqualTo(596.26497100324877d));
            Assert.That(tp.Heading, Is.EqualTo(355.59349002200895d));
            Assert.That(tp.Speed, Is.EqualTo(51.029213612874308d));
        }
    }
}
