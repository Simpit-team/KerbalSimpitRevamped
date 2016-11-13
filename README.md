# Kerbal Sim Pit

This is a [Kerbal Space Program](https://kerbalspaceprogram.com/en/) plugin
to enable communication with devices over a serial connection.

It includes an accompanying [Arduino](https://www.arduino.cc/) library to
make building hardware devices utilising this plugin easy.

## Current status

This is stil a non functional prototype. The plugin can open one or more
serial ports, but no data is sent or received.

The Arduino library has a basic inbound packet parser. It still needs to be
fleshed out to handle sending data, and some more example code.

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

This library currently depends on the
[PsimaxSerial](https://github.com/phardy/PsimaxSerial) serial library.
This is very likely to change before a final release.
