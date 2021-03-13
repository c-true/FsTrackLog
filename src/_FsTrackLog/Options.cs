﻿using CommandLine;

namespace FsTrackLog
{
    public class Options
    {
        [Option('h', "hostname", SetName = "connect", HelpText = "Sets the hostname of the host that is running Flight Simulator.")]
        public string Hostname { get; set; }

        [Option('p', "port", SetName = "connect", HelpText = "Sets the TCP port that Flight Simulator is being hosting on.")]
        public uint Port { get; set; }
    }
}