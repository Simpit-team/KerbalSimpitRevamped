# Proposed layout

## KSPSerialPort

An object used to open and talk to a serial port.

## SimPitController

An object representing a control panel. Contains a **KSPSerialPort**,
and a list of **SimPitControllerOutput** references for outputs this
controller would like to receive.

## SimPitControllerOutput

The **SimPitControllerOutput** class defines a piece of data that will be sent
from the game to a controller.

It should keep a list of controllers to send output to.



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

