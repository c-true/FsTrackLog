using System.IO;
using CTrue.FsTrackLog.Proto;
using CTrue.FsTrackLog.Proto.v2;

namespace CTrue.FsTrackLog.Core.File
{
    public class FsTrackLogFileReader : ITrackLogReader
    {
        private Stream _stream;
        private readonly TrackPointReader _trackPointReader;

        private FsTrackLogFileHeader _fileHeader;

        public int Version => _fileHeader.Version;
        public string Title => _trackPointReader.Title;
        public string AtcId => _trackPointReader.AtcId;
        public string AtcModel => _trackPointReader.AtcModel;
        public double FuelTotalCapacity => _trackPointReader.FuelTotalCapacity;

        public FsTrackLogFileReader(string fileName) : this(new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
        }

        public FsTrackLogFileReader(Stream stream)
        {
            _stream = stream;

            _fileHeader = FsTrackLogFileHeader.Parser.ParseDelimitedFrom(_stream);

            if (_fileHeader.Version == 1)
            {
                _trackPointReader = new TrackPointV1Reader(_stream);
            }
            else
            {
                _trackPointReader = new TrackPointV2Reader(_stream);
            }
        }

        public bool CanReadNext()
        {
            return _trackPointReader.CanReadNext();
        }

        public FsTrackPoint ReadNext()
        {
            return _trackPointReader.ReadNext();
        }
    }
}