using System.IO;
using CTrue.FsTrackLog.Proto.v2;

namespace CTrue.FsTrackLog.Core.File
{
    public class TrackPointV2Reader : TrackPointReader
    {
        public TrackPointV2Reader(Stream stream) : base(stream)
        {
        }

        protected override void ReadTrackLogHeader()
        {
            var logHeader = FsTrackLogHeader.Parser.ParseDelimitedFrom(_stream);
            Title = logHeader.Title;
            AtcId = logHeader.AtcId;
            AtcModel = logHeader.AtcModel;
            FuelTotalCapacity = logHeader.FuelTotalCapacity;
        }

        public override bool CanReadNext()
        {
            return _stream.Position < _stream.Length;
        }

        public override FsTrackPoint ReadNext()
        {
            if (_stream.Position == _stream.Length) return null;

            return FsTrackPoint.Parser.ParseDelimitedFrom(_stream);
        }
    }
}