using CommandLine;

namespace FsTrackLog
{
    public class Options
    {
        [Option('h', "hostname", SetName = "connect", HelpText = "Sets the hostname of the host that is running Flight Simulator.", Required = true)]
        public string Hostname { get; set; }

        [Option('p', "port", SetName = "connect", HelpText = "Sets the TCP port that Flight Simulator is being hosting on.", Required = true)]
        public uint Port { get; set; }

        [Option('f', "filename", HelpText = "Sets the filename that is to be written to or read from.")]
        public string FileName { get; set; }

        [Option('d', "directory", HelpText = "Sets the directory that track log will be written to.")]
        public string Directory { get; set; }

        [Option('g', "generate", HelpText = "Generate GPX file directly after ending logging.")]
        public bool GenerateGpx { get; set; }

        [Option('v', "verbose", HelpText = "Specifies if real time logging of aircraft data should be enabled.")]
        public bool Verbose { get; set; }
    }
}