# Proposed layout

## KSPSerialPort

An object used to open and talk to a serial port.

## Event handling

Check out http://stackoverflow.com/questions/987050/array-of-events-in-c
for an event handler for an array.

I could have one array for FromDevice events, and one for ToDevice events.

### Events from port to game

The FromDevice event handler array. A class adds its event handler to
the index matching the packet ID. When any serial port receives that
serial ID, it just dispatches the data to FromDevice[ID].

### Events from game to port

The ToDevice event handler array. When a port receives a register/deregister
packet, it adds its event handler to the appropriate ID in the ToDevice
array. When a class wants to publish to that event, it calls a method
in KerbalSimPit, which delegates to the appropriate ID in ToDevice.

#### Periodic events from game

I think the scheduling should be handled by KerbalSimPit. If a class wants
to send data periodically, it registers a callback function. Calling this
function triggers an update by the class, eventually calling the ToDevice
delegate wrapper above.

# Data Packet

* 2 byte header
  Alternating 1s and 0s, the last 4 bits denote the protocol version.
  (version 0 header: 0xAA, 0x50)
* 1 byte size (payload only)
* 1 byte type
* x bytes payload

# Outbound packet types

## Things I want to include:

* Scene. Is it enough to just talk about the flight scene here?
  * Enter flight scene
  * Leave flight scene
* Orbital characteristics:
  * AP
  * PE
  * SemiMajorAxis
  * SemiMinorAxis
  * Eccentricity
  * Inclination
  * Longitude of Periapsis
  * Epoch
  * Mean anomaly at Epoch

* Vessel characteristics
  * Pitch
  * Roll
  * Yaw
  * 
* Vessel pitch / roll / heading.
* Action group on / Action group off. Payload is the AG number.


## List of packet types
* **0x00 - 0x0F**: Reserved for internal use.
  * **0x03**: Scene change. Payload:
    * 0x00: Changed in to flight scene.
    * 0x01: Changed in to any other scene.

# Inbound packet types

## Things I want to include:

* Subscriptions
  * Subscribe to topic
  * Unsubscribe from topic
  
## List of packet types
* **0x00 - 0x0F**: Reserved for internal use.
  * **0x03**: Register handler. Payload is one or more bytes representing
    channels to register for.
  * **0x04**: Deregister handler. Payload is one or more bytes representing
    channels to register for.
    
# Common packet types

## List of packet types

* **0x00 - 0x0F**: Reserved for internal use.
  * **0x00**: Synchronisation. First byte of payload:
    * 0x00: SYN
    * 0x01: SYN/ACK
    * 0x02: ACK
    Second byte of payload is an ID. Syn sets the ID, others reuse it.
  * **0x01**: Echo request. Arbitrary (small) payload.
  * **0x02**: Echo reply. Payload dupes the echo request we're replying to.

# Data generators

Data generators are responsible for gathering data from the game, and sending it to
serial devices.

Each generator is implemented as a class. The class should maintain a list of KSPSerialPort
objects that it will send data to, and its update method should gather required data
and send to the ports.

A data generator can be time- or event- based. Time-based generators have an interval
that can be specified in the config file, and both a global refresh and generator-specific
refresh rates should be supported.

A time-based generator also has a random component, at run-time each generator has a
random offset equal to the refresh rate. The intention is that this will smear updates
for each generator across the update range.

Event-based generators only send data when their associated event takes place. For example,
the action group generator only needs to send an update when an action group changes state.

