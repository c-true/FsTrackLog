namespace CTrue.FsTrackLog.Core
{
    public class FsTrackLogConfig
    {
        public string HostName { get; set; }

        public uint Port { get; set; }

        public bool AutoConnect { get; set; }

        public bool AutoLog { get; set; }

        public string StorePath { get; set; }
    }
}