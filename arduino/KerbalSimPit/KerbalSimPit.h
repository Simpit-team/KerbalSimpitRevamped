#ifndef KerbalSimPit_h
#define KerbalSimPit_h

#include <Arduino.h>

class KerbalSimPit
{
 public:
  KerbalSimPit(int speed);
  void inboundHandler(void (*packetHandler)(void));
  void update();

  enum CommonPackets
  {
    Synchronisation = 0x00,
    EchoRequest = 0x01,
    EchoResponse = 0x02,
  };
  enum InboundPacketes
  {
    SceneChange = 0x03,
  };
  enum OutboundPackets
  {
    RegisterHandler = 0x03,
    DeregisterHandler = 0x04,
  };

 private:
  byte _inboundBuffer[32];
  byte _outboundBuffer[32];
  
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

  void (*_packetHandler)(void);
};

#endif
