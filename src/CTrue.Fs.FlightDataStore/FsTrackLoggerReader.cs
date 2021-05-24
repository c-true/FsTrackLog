using System.IO;
using FsTrackLog.Proto.Generated;

namespace CTrue.Fs.FlightData.Store
{
    public class FsTrackLoggerReader
    {
        private Stream _stream;

        private FsTrackLogHeader _header;

        public int Version => _header.Version;

        public FsTrackLoggerReader(string fileName)
        {
            _stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        public FsTrackLoggerReader(Stream stream)
        {
            _stream = stream;

            _header = FsTrackLogHeader.Parser.ParseDelimitedFrom(_stream);
        }

        public FsTrackPoint ReadNext()
        {
            if (_stream.Position == _stream.Length) return null;

            return FsTrackPoint.Parser.ParseDelimitedFrom(_stream);
        }
    }
}