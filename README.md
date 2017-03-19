# Kerbal Sim Pit

This is a [Kerbal Space Program](https://kerbalspaceprogram.com/) plugin
to enable communication with devices over a serial connection.

It works with an accompanying [Arduino library](https://bitbucket.org/pjhardy/kerbalsimpit-arduino)
to make building hardware devices simpler.

## Getting started

1. Read the "Installation" section below.
1. Install the plugin.
1. There is currently no supplied configuration file. Run the game
once to generate one. It can be found in
`GameData/KerbalSimPit/PluginData/Settings.cfg`
1. Flash one of the [Arduino library](https://bitbucket.org/pjhardy/kerbalsimpit-arduino) example sketches to a demo board.
1. Edit the configuration file to point to your device's serial port.
1. Run the game again. A successful handshake should be logged to
the `KSP.log`.

## Current status

The plugin has full support for specifying multiple devices, and will
attempt to handshake with all of them at startup.

Inbound and outbound event handling is in place. Any class can add an
event handler to receive data from serial devices on specific channels.
It can also find and publish to any specific channel. Serial devices that
have registered an interest in those channels will receive data published.
A method has also been provided to allow classes to send data to one
specific device. See the Interfacing section below for details on using
these.

An echo provider has been completed. This provider responds to
EchoRequest packets with an EchoResponse packet to the originating
device.

All of the functionality for serial devices registering and deregistering
from channels is implemented but not heavily tested.

### Operating system compatibility

I regularly test this plugin on MacOS and Linux (64 bit Debian).

Windows 10 is not (yet) supported. This requires further testing, but I
suspect it's down to the multithreaded serial read implementation. I'm
in the process of exploring other options for this, and may just fall
back on using the event handler that's proved buggy on other operating
systems, with a config option to switch between the two. Help is still
appreciated with this.

Other versions of Windows are untested.

### KSP compatibility

This plugin is developed against KSP version 1.2.9. Other versions are not
supported.

## TODO

* Begin implementing actual data handlers.
* Flesh out Arduino library, and fix the warnings that break compile on
  some platforms (like teensy).
* Runtime configuration of serial ports.

## Planned features

* Small, lightweight communication. Data items are sent individually, on
an as-needed basis, rather than regular monolithic data packets.
* Dynamic configuration. Serial ports and speeds are configured through an
in-game UI. Supported features are then negotiated between the plugin and
the connected device.
* Multiple devices. The plugin supports an arbitrary number of serial
connections, each with individual input and output configuration.
* Easily extensible. New functionality should be provided to the plugin by
including small classes with a well-defined interface. It should also
support extending capabilities by external add-ons.

## Installing

1. Don't.

## Building

This plugin depends on
[SerialPortLib2](https://github.com/JTrotta/SerialPortLib2). I build it
from source, targetting .NET 3.5. The source code is otherwise unchanged.

The project file makes these assumptions:

* SerialPortLib2.dll is present in the same directory as the project file.
  I symlink this to the release build.
* KSP resources are available in a KSPresources directory in the same
  directory as the project file. It's easiest to just create this as a
  symlink to `Resources/Data/Managed` in the KSP root.

# Using the plugin

## Data format

A data packet is at most 32 bytes, with a 4 byte header and variable
payload size. The payload size and content is channel-dependant.

Byte|Use          |Notes
----|-------------|-----
 0  |Magic byte 1 |The first byte of a packet is 0xAA, alternating 1 and 0)
 1  |Magic byte 2 |The second magic byte is 0x50. Alternating 0 and 1, with the last four bits denoting protocol version 0.
 2  |Packet length|Size of the payload. This does *not* include the header.
 3  |Packet type  |The data channel this packet is intended for.
 4: |Payload      |

### Channels

There are a maximum of 256 inbound and outbound channels. A serial device
can register to receive events for specific outbound channels, and the
plugin will only send it packets addressed to those channels. A serial
device can send inbound packets to any channel. A class can register
callbacks for one or more channels, and those callbacks will only be
called when inbound packets are received for those channels.

For both inbound and outbound packets, channel numbers 0x00-0x0F are
reserved for internal use. No other channel numbers are currently assigned,
but definitely don't expect them to be stable yet.

## Receiving data from serial devices

The plugin uses KSP's GameEvents extension to pass data between devices
and classes.

First, create a callback method. Callbacks should have this signature:

    public void myEventCallback(byte ID, object Data)
    
Where ID is the ID of the serial port that sent the packet, and Data
is (almost always) a byte array.

Then, to find the game event and register your callback, use a routine
like this.

    private EventData<byte, object> mySerialEvent;
    // Look up the GameEvent for serial packets on channel 17
    mySerialEvent = GameEvents.FindEvent<EventData<byte, object>>("onSerialReceived17");
    // Add our callback to the event's handler
    if (mySerialEvent != null) mySerialEvent.Add(myEventCallback);

It's also important to remove your handler in your class OnDestroy:

    if (mySerialEvent != null) mySerialEvent.Remove(myEventCallback);

## Sending data to serial devices

### Sending to a channel

When a serial device subscribes to a channel, it adds a Send callback to the
appropriate GameEvent. When a class wants to publish to that channel, it
just needs to use that GameEvent's Fire method.

    private EventData<byte, object> myDeviceChannel;
    // Look up the GameEvent for outbound packets on channel 21
    myDeviceChannel = GameEvents.FindEvent<EventData<byte, object>>("toSerial21");

    byte channel = 21; // The channel we're sending on
    int Data = 1024; // Data can be any serializable datatype
    if (myDeviceChannel != null) myDeviceChannel.Fire(channel, Data);

### Sending to a specific device

Sometimes it's appropriate to send to a specific device, eg responding to
a request for information from a serial device. The echo handler uses this
to ensure that EchoResponse packets are only sent to the device that sent the
EchoRequest.

    // This callback method receives EchoRequest packets
    public void EchoRequestCallback(byte ID, object Data)
    {
        byte channel = 21; // The channel we're sending our packet on
        int Data = 1024; // Data can be serializable datatype
        KerbalSimPit.SendToSerialPort(ID, channel, Data);
    }

### What types of data can be sent

Both send methods will accept any serializable data type, as long as its
total size does not exceed the maximum - packets are not fragmented.

A struct can be used to send several different variables at once

    [StructLayout(LayoutKind.Sequential, Pack=1)][Serializable]
    public struct myData
    {
        public int varA;
        public int varB;
    }

    private myData md;
    md.varA = 17;
    md.varB = 21;
    byte channel = 37;
    if (myDeviceChannel != null) myDeviceChannel.Fire(channel, md);

Alternatively, the plugin will also accept an array of Bytes.

## Sending scheduled data

It's expected that several classes will want to send data periodically,
eg for vessel telemetry. Rather than a class having to maintain its own
schedule of when to send, it can register a callback with this plugin,
and receive a notification of when it has a short timeslice to send
data in.

To use this, create a callback function that accepts no arguments, and
pass it to `KerbalSimPit.AddToDeviceHandler()`. This function will be
called whenever the plugin expects the passing class to gather and
send data.

Ensure the function is removed in the class' OnDestroy with
`KerbalSimPit.RemoveToDeviceHandler()`.
