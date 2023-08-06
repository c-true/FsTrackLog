using CTrue.FsTrackLog.Core.File;
using NUnit.Framework;

namespace CTrue.FsTrackLog.Test.File
{
    [TestFixture]
    public class FlightDataStoreTest
    {
        [Test]
        public void Test()
        {
            // Arrange
            FlightDataStore store = new FlightDataStore();
            
            store.Initialize(@"C:\temp\FsTrackLog");

            // Act
            var trackLogs = store.GetTrackLogs();

            // Assert
            Assert.That(trackLogs, Is.Not.Empty);
        }
    }
}