using System;

namespace CTrue.FsTrackLog.Core
{
    public class FsTrackLog
    {
        public string FsTrackLogId { get; }

        public string AircraftName { get; set; }

        public FsTrackLog()
        {
            FsTrackLogId = Guid.NewGuid().ToString();
        }


    }
}