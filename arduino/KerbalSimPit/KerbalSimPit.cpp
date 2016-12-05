#include "KerbalSimPit.h"

KerbalSimPit::KerbalSimPit(int speed)
{
  Serial.begin(speed);
}

void KerbalSimPit::init()
{
  // Nothing yet
}

void KerbalSimPit::inboundHandler(void (*packetHandler)(void))
{
  _packetHandler = packetHandler;
}

void KerbalSimPit::update()
{
  while (Serial.available()) {
    byte nextByte = Serial.read();
    switch (_receiveState) {
    case WaitingFirstByte:
      if (nextByte == 0xAA) {
        _receiveState = WaitingSecondByte;
        break;
      }
    case WaitingSecondByte:
      if (nextByte == 0x50) {
        _receiveState = WaitingSize;
        break;
      }
    case WaitingSize:
      _inboundBuffer[2] = nextByte;
      _receiveState = WaitingType;
      break;
    case WaitingType:
      _inboundBuffer[3] = nextByte;
      _receivedIndex = 4;
      _receiveState = WaitingData;
      break;
    case WaitingData:
      _inboundBuffer[_receivedIndex] = nextByte;
      _receivedIndex++;
      if (_receivedIndex == _inboundBuffer[2]) {
        // TODO: process packet here
        _receiveState = WaitingFirstByte;
        _receivedIndex = 0;
        break;
      }
    }
  }
}
