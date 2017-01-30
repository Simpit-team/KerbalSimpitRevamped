# Kerbal Sim Pit

This is a [Kerbal Space Program](https://kerbalspaceprogram.com/) plugin
to enable communication with devices over a serial connection.

It includes an accompanying [Arduino](https://www.arduino.cc/) library to
make building hardware devices utilising this plugin easy.

## Current status

The plugin is capable of completing a handshake with a single device.
No other data is currently sent or acknowledged. It is able to open more
than one port, but I haven't yet tested handshaking with multiple
devices.

The Arduino library has a basic inbound packet parser. It still needs to be
fleshed out to handle sending data, and some more example code.

### Operating system compatibility

I regularly test this plugin on MacOS and Linux (64 bit Debian).

Windows 10 is not (yet) supported. The device is opened, but from what I
can see with a logic analyzer hooked up to the UART, no data is sent or
received. Any help troubleshooting this would be much appreciated.

Other versions of Windows are untested.

### KSP compatibility

This plugin is developed against KSP version 1.2.2. Other versions are not
supported.

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
