# Kerbal Simpit Revamped

This is the repository for a revamped version of the excellent KSP mod _Kerbal Simpit_, to try and bring it up to date with some of the recent changes to the game. I am also planning to look at incorporating some new features into the mod, to better match the needs of the community.

## About

This is a [Kerbal Space Program](https://kerbalspaceprogram.com/) plugin to enable communication with devices over a serial connection.

It works with an accompanying [Arduino library](https://github.com/LRTNZ/KerbalSimpitRevamped-Arduino) to make building hardware devices simpler.

There is currently  no wiki, but the original repository  still contains an [outdated wiki](https://bitbucket.org/pjhardy/kerbalsimpit/wiki/Home) that will likely still be of use.

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

## How to contribute
### TODO

* Begin implementing actual data handlers.
* Flesh out Arduino library, and fix the warnings that break compile on
  some platforms (like teensy).
* Runtime configuration of serial ports.
