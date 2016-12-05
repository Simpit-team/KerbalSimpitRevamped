#include "KerbalSimPit.h"

KerbalSimPit::KerbalSimPit(int speed)
{
  Serial.begin(speed);
}

bool KerbalSimPit::init()
{
  Serial.begin(115200);
  delay(10);
  while (!Serial);
  _outboundBuffer[0] = 0x00;
  _outboundBuffer[1] = 0x37;
  send(0x00, _outboundBuffer, 2); // Send SYN
  while (!Serial.available());
  if (Serial.read() == 0xAA) { // First byte of header
    while (!Serial.available());
    if (Serial.read() == 0x50) { // Second byte of header
      while (!Serial.available());
      Serial.read(); // size
      while (!Serial.available());
      if (Serial.read() == 0x00) { // type
        while (!Serial.available());
        if (Serial.read() == 0x01) { // first byte of payload, we got a SYNACK
          // TODO: Do we care about tracking handshake state?
          _outboundBuffer[0] = 0x02;
          send(0x00, _outboundBuffer, 2); // Send ACK
          return true;
        }
      }
    }
  }
  return false;
}

void KerbalSimPit::inboundHandler(void (*packetHandler)(void))
{
  _packetHandler = packetHandler;
}

void KerbalSimPit::send(byte PacketType, byte *msg, byte msgSize)
{
  Serial.write(0xAA);
  Serial.write(0x50);
  Serial.write(msgSize);
  Serial.write(PacketType);
  for (int x=0; x<msgSize; x++) {
    Serial.write(*(msg+x));
  }
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
