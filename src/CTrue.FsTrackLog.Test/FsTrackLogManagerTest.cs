﻿using CTrue.Fs.FlightData.Contracts;
using CTrue.FsTrackLog.Core;
using CTrue.FsTrackLog.Core.File;
using FakeItEasy;
using NUnit.Framework;

namespace CTrue.FsTrackLog.Test
{
    [TestFixture]
    public class FsTrackLogManagerTest
    {
        [Test]
        public void Test()
        {
            // Arrange
            IFlightDataProvider provider = A.Fake<IFlightDataProvider>();
            IFlightDataStore store = A.Fake<IFlightDataStore>();

            var config = new FsTrackLogConfig()
            {
                HostName = "192.168.1.174",
                Port = 500,
                AutoConnect = true,
                AutoLog = true,
                StorePath = "c:\\temp\\FsTrackLog"
            };

            FsTrackLogManager manager = new FsTrackLogManager(provider, store);

            // Act
            manager.Initialize(config);

            // Assert
            A.CallTo(() => provider.Start()).MustHaveHappened();
        }

        [Test]
        public void Test2()
        {
            // Arrange


            // Act


            // Assert
        }


    }
}