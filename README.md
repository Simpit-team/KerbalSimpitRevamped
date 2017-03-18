# Kerbal Sim Pit

This is a [Kerbal Space Program](https://kerbalspaceprogram.com/) plugin
to enable communication with devices over a serial connection.

It includes an accompanying [Arduino](https://www.arduino.cc/) library to
make building hardware devices utilising this plugin easy.

## Current status

The plugin has full support for specifying multiple devices, and will
attempt to handshake with all of them at startup. As a proof of concept,
an echo provider has been implemented. The Arduino library comes with
an example sketch that every second will send an echo request. The plugin
will reply with an echo response.

The plugin utilises KSP's GameEvents extension to pass data between
serial ports and handler classes. Any class can register a simple callback
to begin receiving data from a channel, and can publish to a channel by
finding it and calling its Fire method.

All of the functionality for serial devices registering and deregistering
from channels is implemented but not heavily tested.

The Arduino library has a basic inbound packet parser. It still needs to be
fleshed out to handle sending data, and some more example code.

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
