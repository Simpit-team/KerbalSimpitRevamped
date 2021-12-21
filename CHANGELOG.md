# Kerbal Simpit Changelog

## v(to be decided), not in an official release yet

### Features

Allow DTR flag to the serial connection (seems to have no impact with Arduino connection and allow connection from a Pi Pico).

### Bugfixes

## v2.1.1 (2021-12-20)

### Bugfixes

Fix an issue when ARP is not correctly detected (seen on Mac)
Fix an issue when a custom ressource is not found and a NullReference issue is not caught

## v2.1.0 (2021-12-12)

### Features

Add a generic parseMessage function (in the Arduino lib)
Add message for atmospheric conditions
Add the Comnet control level, current stage, a boolean to indicate if a target is set and current vessel type in a FlightStatus message
Change internal way to handle some messages. In particular prevent sending a message if no information has changed for some channels. This is applied to ressources messages and FlightStatusMessage for now.
Add a message with information about current vessel rotation, its velocity orientation, maneuver and target orientation (change in maneuver and target message).
Add a message to control custom axis.
Add a message to close the connection from the Arduino.


### Bugfixes

Fix an issue where the messages to control throttle did not work for parts assigned to main throttle.
Fix a typo in FlightStatus helper functions

## v2.0.0 (2021-09-07)

### Features
 
Add a GUI to monitor the connection state and open/close ports.
Add message for Xenon ressource
Add message for TAC Life support ressources
Add message for generic ressource, to be configured by the user. Tested with CommunityResourcePack.
Add message for deltaV and burntime
Add message for maneuver data
Add message for current temperature of vessel (with respect to the maximum temporature)
Add message with all the orbit parameters (not only apoapsis and periapsis).
Add message for CAG status (broken for CAG > 10 with AGExt)
Add message to control Timewarp
Add message to control camera position
Add message to send a log line to KSP from Arduino
Add message to emulate a keypress in KSP (to open the map, quicksave, etc.)
Add message to inform the controller about the current SAS mode
Add message for flight status (including warp speed, status, crew, com)

Add support for console command to start/stop the connection
Update of the examples and addition of an example for using the numeric input (throttle, roll, pich, yaw, etc.) or keyboard emulation.
Add some helper functions in the Arduino lib.
Reset all subscribed channels when handshake is complete (typical case of Arduino being reset).
Add warning displayed to the user when no port name is set.

### Bugfixes

Fix an error when a non-vessel target is set
Fix an issue when the command are send to the wrong vessel when switching vessel
Fix sending of the action state not done when all actions were off
Fix an issue where the current SOI is not sent when the channel is first requested
Fix the scene change message not being sent
Improve I/O Error handling (including crash to desktop when the Arduino is unplugged) and improve Mac compatibility
Fix an issue where EVA Message is not sent in KSP 1.11+ due to an unfixed bug in ARP on those versions
Fix an issue when the handshake takes a lot of time to happen after an Arduino reset

### Known issues

When subscribing several times to the same channel, messages are send several times at each tick.
The IDLE status of a connection is only computed when messages are sent (only an issue when there is no periodic channel subscribed).
After an Arduino reset, it sometimes takes several seconds to reconnect while spamming the log with "SYN received on port XXX"


## v1.4.1 (2020-07-02)

* Built against KSP 1.10.0.

## v1.4.0 (2020-06-29)

* Built against KSP 1.9.1.
* Remove KerbalSimpitSerial. Use Unity's System.IO.Ports provided
  by KSP instead.

## v1.3.0 (2018-12-18)

Adds new channel for SAS mode. Thanks Zack Thomas.

## v1.2.9 (2018-10-21)

Built against KSP 1.5.1.

## v1.2.8 (2018-06-30)

Built against KSP 1.4.4.

## v1.2.7 (2018-05-04)

Built against KSP 1.4.3.

## v1.2.6 (2018-03-29)

Built against KSP 1.4.2.

## v1.2.5 (2018-03-16)

* Partial fix for broken throttle handler. The receive handler is
  set up properly now, but it looks like it's still getting buggy
  values.
* Added debug logging around the throttle handler. With verbose
  set to true in the config file, all decoded throttle packets
  are logged.

## v1.2.4 (2018-03-14)

Built against KSP 1.4.1.

## v1.2.3 (2018-03-11)

Built against KSP 1.4.

## v1.2.2 (2017-09-06)

Built against KSP 1.3.1 prerelease.

## v1.2.1 (2017-07-03)

* Serial port wrapper now uses an internal queue, to prevent multiple
  threads trying to write to the port. Should prevent TimeoutExceptions.
* Increased default RefreshRate to 125.

## v1.2.0 (2017-06-24)

* Added new channels for flight control state handling:
  * Rotation of active vessel
  * Translation of active vessel
  * Wheel steer and throttle of active vessel
  * Main throttle of active vessel

## v1.1.1 (2017-06-20)

* Fix action group status sending.

## v1.1 (2017-06-01)

* Fix nullrefs from the resource provider when Alternate Resource Panel
  is not installed.
* Add channel for SoI of active vessel.

## v1.0 (2017-05-28)

First stable release. Changed names across both this and the Arduino
projects to be consistent.

## v0.10 prerelease (2017-05-26)

Built against KSP 1.3. Update metadata to only support 1.3.

## v0.9 prerelease (2017-05-22)

Hopefully the last prerelease.

* Hardcoded `Documentation` parameter in config file. The config class
  keeps this paramater as static, pointing to a URL giving info on the
  config file format.
* RefreshRate parameter now exposed in the configuration. This is a
  global parameter - every provider that wants to send data
  periodically will send to all devices that want that provider's
  data once per `RefreshRate` milliseconds.
* New providers:
  * `ApsidesTime` gives time to next apoapsis and periapsis, in seconds.
  * `TargetInfo` gives information about any object the active vessel has
    targetted. Both distance to target and relative velocity are sent.

## v0.8 prerelease (2017-05-18)

* Added channels for all stock resources. This is done by depending on the
Alternate Resource Panel mod.
* Added apsides channel (apoapsis and periapsis).
* Added velocity channel (surface, orbital and vertical).
* Added a channel that reports on action group status. The format is identical
to the command channel for AGs.
* Changed the underlying serial driver again. The library now includes its
own driver, a straight fork of the System.IO.Ports class from mono. There's
been a lot of internal restructuring to let this happen.
* Build and deploy system has been overhauled.

## v0.7 prerelease (2017-05-01)

Big focus on getting the KSPSerialPort class receiving properly on Windows.

* Switched from SerialPortLib2 to PsimaxSerial for the underlying serial
driver. This will soon be migrated (again), to my own fork of mono's
System.IO.Ports class.
* Added a new serial read thread to, that just periodically polls.
* Currently using regex matching on the serial port name and initialisation:
  * COM ports imply Windows, and the new serial polling read thread is used.
  * For all other ports, the existing async event handler thread is used.

## v0.6 prerelease (2017-04-24)

* Add support for standard action groups (staging, abort, lights etc). As
with custom AGs, packets for enable, disable and toggle have been added.

## v0.5 prerelease (2017-04-22)

* Add toggle Custom AG packet and handler.

## v0.4 prerelease (2017-04-20)

* Fix off-by-one errors in the array iteration loops. This was most
obvious in the staging handler, but potentially happening elsewhere.

## v0.3 prerelease (2017-04-01)

* Modify Staging handler to accept byte array payload (can now enable and
disable the staging AG in a single packet).

## v0.2 prerelease (2017-03-29)

* Custom Action Group support added (including Action Groups Extended
integration)

## v0.1 prerelease (2017-03-26)

* Asynchronous serial read thread complete.
* Echo handler implemented.
* Staging test handler implemented.
* Altitude handler implemented.
* Initial release.
