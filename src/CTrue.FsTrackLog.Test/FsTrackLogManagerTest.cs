using CTrue.Fs.FlightData.Contracts;
using CTrue.Fs.FlightData.Store;
using CTrue.FsTrackLog.Core;
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
                Port = 57490,
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


    }
}