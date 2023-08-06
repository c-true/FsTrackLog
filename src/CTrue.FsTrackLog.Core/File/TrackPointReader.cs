using System.IO;
using CTrue.FsTrackLog.Proto.v2;

namespace CTrue.FsTrackLog.Core.File
{
    public abstract class TrackPointReader
    {
        protected readonly Stream _stream;

        public string Title { get; set; }
        public string AtcId { get; set; }
        public string AtcModel { get; set; }
        public double FuelTotalCapacity { get; set; }

        public TrackPointReader(Stream stream)
        {
            _stream = stream;

            ReadTrackLogHeader();
        }

        protected abstract void ReadTrackLogHeader();

        public abstract bool CanReadNext();

        public abstract FsTrackPoint ReadNext();
    }
}