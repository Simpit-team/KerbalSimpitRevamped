# Kerbal Sim Pit

This is a [Kerbal Space Program](https://kerbalspaceprogram.com/) plugin
to enable communication with devices over a serial connection.

It works with an accompanying [Arduino library](https://bitbucket.org/pjhardy/kerbalsimpit-arduino)
to make building hardware devices simpler.

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

## Usage

Refer to the [wiki](https://bitbucket.org/pjhardy/kerbalsimpit/wiki/Home)
for installation and usage.

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
