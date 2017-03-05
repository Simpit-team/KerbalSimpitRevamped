/* KerbalSimPitHelloWorld

   Simplest hello world sketch

   Peter Hardy <peter@hardy.dropbear.id.au>
*/

#include "KerbalSimPit.h"

KerbalSimPit mySimPit(115200);

bool state = false;

void setup() {
  for (int i=0; i<14; i++) {
    pinMode(i, OUTPUT);
    digitalWrite(i, LOW);
  }
  Serial.begin(115200);
  pinMode(13, OUTPUT);
  if(mySimPit.init()) {
    digitalWrite(13, LOW);
    mySimPit.inboundHandler(packetHandler);
  } else {
    // Don't know what to do. Abort.
    digitalWrite(13, HIGH);
    while (true);
  }
}

void loop() {
  mySimPit.send(ECHO_REQ_PACKET, "low", 4);
  delay(1000);
  mySimPit.send(ECHO_REQ_PACKET, "high", 5);
  delay(1000);
}

void packetHandler(byte packetSize, byte *msg, byte msgSize) {
  if (state) {
    digitalWrite(13, LOW);
  } else {
    digitalWrite(13, HIGH);
  }
  state = !state;
}
