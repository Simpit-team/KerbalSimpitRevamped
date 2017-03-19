/* KerbalSimPitHelloWorld

   Simplest hello world sketch

   Peter Hardy <peter@hardy.dropbear.id.au>
*/

#include "KerbalSimPit.h"

KerbalSimPit mySimPit(115200);

bool state = false;
unsigned int lastSent = 0;
int sendInterval = 1000;

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
  unsigned int now = millis();
  if (now > lastSent + sendInterval) {
    if (state) {
      mySimPit.send(ECHO_REQ_PACKET, "high", 5);
    } else {
      mySimPit.send(ECHO_REQ_PACKET, "low", 4);
    }
    lastSent = now;
  }
  mySimPit.update();
}

void packetHandler(byte packetType, byte *msg, byte msgSize) {
  if (strcmp(msg, "low")) {
    digitalWrite(13, LOW);
    state = false;
  } else {
    digitalWrite(13, HIGH);
    state = true;
  }
}
