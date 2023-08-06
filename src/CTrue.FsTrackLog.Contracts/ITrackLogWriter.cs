using System;
using CTrue.Fs.FlightData.Contracts;

namespace CTrue.FsTrackLog.Contracts
{
    public interface ITrackLogWriter
    {
        bool IsOpen { get; }

        /// <summary>
        /// Writes a data point to the track log.
        /// </summary>
        /// <param name="ai"></param>
        void Write(AircraftInfo ai);

        void Close();
    }
}
