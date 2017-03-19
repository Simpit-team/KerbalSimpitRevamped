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
  _receiveState = WaitingFirstByte;
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

void KerbalSimPit::inboundHandler(void (*packetHandler)(byte packetType,
                                                        byte *msg,
                                                        byte msgSize))
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
      _inboundSize = nextByte;
      _receiveState = WaitingType;
      break;
    case WaitingType:
      _inboundType = nextByte;
      _receivedIndex = 0;
      _receiveState = WaitingData;
      break;
    case WaitingData:
      _inboundBuffer[_receivedIndex] = nextByte;
      _receivedIndex++;
      if (_receivedIndex == _inboundSize) {
        _receiveState = WaitingFirstByte;
        _packetHandler(_inboundType, _inboundBuffer, _inboundSize);
      }
      break;
    default:
      _receiveState = WaitingFirstByte;
    }
  }
}
