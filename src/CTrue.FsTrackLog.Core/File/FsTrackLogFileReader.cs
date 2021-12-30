using System.IO;
using CTrue.FsTrackLog.Core.File.Generated;
using CTrue.FsTrackLog.Core.File.Generated.v1;

namespace CTrue.FsTrackLog.Core.File
{
    public class FsTrackLogFileReader : ITrackLogReader
    {
        private Stream _stream;

        private FsTrackLogHeader _header;

        public int Version => _header.Version;

        public FsTrackLogFileReader(string fileName)
        {
            _stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            _header = FsTrackLogHeader.Parser.ParseDelimitedFrom(_stream);
        }

        public FsTrackLogFileReader(Stream stream)
        {
            _stream = stream;

            _header = FsTrackLogHeader.Parser.ParseDelimitedFrom(_stream);
        }

        public bool CanReadNext()
        {
            return _stream.Position < _stream.Length;
        }

        public FsTrackPoint ReadNext()
        {
            if (_stream.Position == _stream.Length) return null;

            return FsTrackPoint.Parser.ParseDelimitedFrom(_stream);
        }
    }
}