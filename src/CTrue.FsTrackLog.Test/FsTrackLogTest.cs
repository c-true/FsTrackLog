﻿using System;
using System.IO;
using CTrue.Fs.FlightData.Contracts;
using CTrue.FsTrackLog.Core.File;
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

            TrackLogFileWriter writer = new TrackLogFileWriter(ms);

            writer.Write(new AircraftInfo()
            {
                Title = "A",
                AtcId = "B",
                AtcModel = "C",
                Latitude = 60,
                Longitude = 10,
                Altitude = 100,
                AltitudeAboveGround = 50,
                Heading = 10,
                Speed = 20,
                SimOnGround = false,
                TimeStamp = new DateTime(2021, 1, 11, 0, 16, 40),
                FuelTotalCapacity = 60,
                FuelTotalQuantity = 40
            });

            //writer.Close();

            ms.Position = 0;

            FsTrackLogFileReader reader = new FsTrackLogFileReader(ms);

            Assert.That(reader.Version, Is.EqualTo(2));
            Assert.That(reader.Title, Is.EqualTo("A"));
            Assert.That(reader.AtcId, Is.EqualTo("B"));
            Assert.That(reader.AtcModel, Is.EqualTo("C"));
            Assert.That(reader.FuelTotalCapacity, Is.EqualTo(60));

            var tp = reader.ReadNext();

            // Assert

            Assert.That(tp, Is.Not.Null);
            Assert.That(DateTime.FromBinary(tp.Time), Is.EqualTo(new DateTime(2021, 1, 11, 0, 16, 40)));
            Assert.That(tp.Latitude, Is.EqualTo(60));
            Assert.That(tp.Longitude, Is.EqualTo(10));
            Assert.That(tp.Altitude, Is.EqualTo(100));
            Assert.That(tp.AltitudeAboveGround, Is.EqualTo(50));
            Assert.That(tp.Heading, Is.EqualTo(10));
            Assert.That(tp.Speed, Is.EqualTo(20));
            Assert.That(tp.FuelTotalQuantity, Is.EqualTo(40));

            var tp2 = reader.ReadNext();
            Assert.That(tp2, Is.Null);
        }

        [Test]
        public void ReadFile()
        {
            byte[] testData = TestResource.FsTrackLog_20210404123943_v1;

            MemoryStream ms = new MemoryStream(testData);

            FsTrackLogFileReader reader = new FsTrackLogFileReader(ms);

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
