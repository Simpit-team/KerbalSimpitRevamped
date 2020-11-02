# Kerbal Simpit Revamped

This is the repository for my revamped version of the excellent KSP mod Kerbal Simpit, to try and bring it up to date with some of the recent changes to the game. Also, I am planing to look at incorporating some suggested features into this mod, to make it better match what the community wants.

## Original Readme Contents - yet to be updated

This is a [Kerbal Space Program](https://kerbalspaceprogram.com/) plugin
to enable communication with devices over a serial connection.

It works with an accompanying [Arduino library](https://bitbucket.org/pjhardy/kerbalsimpit-arduino)
to make building hardware devices simpler.

The [wiki] contains more information on using, and working on the plugin.

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
