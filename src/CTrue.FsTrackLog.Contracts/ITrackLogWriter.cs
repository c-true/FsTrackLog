using System;
using CTrue.Fs.FlightData.Contracts;

namespace CTrue.FsTrackLog.Contracts
{
    public interface ITrackLogWriter
    {
        /// <summary>
        /// Writes a data point to the track log.
        /// </summary>
        /// <param name="ai"></param>
        void WriteNext(AircraftInfoV1 ai);

        void Close();
    }
}
