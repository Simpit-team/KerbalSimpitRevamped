#ifndef KerbalSimPit_h
#define KerbalSimPit_h

#include <Arduino.h>

// These are the recognised packet types
// Sync packets are used for handshaking
const byte SYNC_PACKET = 0x00;
// Echo request packet. Either end can send this.
const byte ECHO_REQ_PACKET = 0x01;
// Echo response packet. Sent in reply to an echo request.
const byte ECHO_RESP_PACKET = 0x02;

// Scene change packets are sent by the game when
// entering or leaving the flight scene.
const byte SCENE_CHANGE_PACKET = 0x03;

// Register and deregister packets are sent by a device to register
// or deregister to event channels.
const byte REGISTER_PACKET = 0x03;
const byte DEREGISTER_PACKET = 0x04;

// How often the plugin should poll for serial data, in ms
static byte CLAMP_TIME = 2;

class KerbalSimPit
{
 public:
  KerbalSimPit(int speed);
  bool init();
  void inboundHandler(void (*packetHandler)(byte packetType,
                                            byte *msg, byte msgSize));
  void send(byte packetType, byte *msg, byte msgSize);
  void update();

 private:
  byte _inboundType;
  byte _inboundSize;
  byte _inboundBuffer[32];
  byte _outboundBuffer[32];
  byte _outboundSize;
  unsigned long _lastPoll;

  enum ReceiveState_t
  {
    WaitingFirstByte,
    WaitingSecondByte,
    WaitingSize,
    WaitingType,
    WaitingData,
  };
  ReceiveState_t _receiveState;
  byte _receivedIndex;

  void (*_packetHandler)(byte packetType, byte *msg, byte msgSize);
};

#endif
