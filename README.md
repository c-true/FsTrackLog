# FsTrackLog
Track logger for Flight Simulator

The Flight Simulator Track Logger is a limited tool to capture track logs from flighs in Flight Simulator.
It can capture tracklogs to a binary format and based on that log generate a GPX file for use in other applications.

# Usage

Example:
```
FsTrackLog -h 127.0.0.1 -p 500 -v -g -d "c:\mylogs\"
```

-h Hostname where Flight Simulator is running  
-p Port that Flight Simulator accepts TCP connections on.  
-v Verbose mode, logs MSFS values to console.  
-g Generate GPX log when logging session has ended.  
-d Directory to store binary and GPX logs.

# Binary Format

## File header

| Name | Size |
|-|-|
| Version ID | 1 byte |

## Version 1

### Header

_This version has no header._

### Track point

| Name | Size |
|-|-|
| Time (UTC) | 8 bytes |
| Latitude | 8 bytes |
| Longitude | 8 bytes |
| Altitude, in meters | 8 bytes |
| Altitude above ground, in meters | 8 bytes |
| Heading, in degrees, True | 8 bytes |
| Speed, in m/s | 8 bytes |


## Version 2

### Header

| Name | Size |
|-|-|
| Title | <=256 byte |
| AtcId | <=256 byte |
| AtcModel | <=256 byte |
| FuelTotalCapacity, in gallons | 8 byte |

### Track points

| Name | Size |
|-|-|
| Version ID | 1 byte |
| Time (UTC) | 8 bytes |
| Latitude | 8 bytes |
| Longitude | 8 bytes |
| Altitude, in meters | 8 bytes |
| Altitude above ground, in meters | 8 bytes |
| Heading, in degrees, True | 8 bytes |
| Speed, in m/s | 8 bytes |
| SimOnGround | 1 bytes |
| AutopilotMaster | 1 bytes |
| FuelTotalQuantity, in gallons | 8 bytes |
