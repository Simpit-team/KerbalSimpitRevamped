/* KerbalSimPitHelloWorld

   Simplest hello world sketch

   Peter Hardy <peter@hardy.dropbear.id.au>
*/

#include "KerbalSimPit.h"

KerbalSimPit mySimPit(115200);

void setup() {
  for (int i=0; i<14; i++) {
    pinMode(i, OUTPUT);
    digitalWrite(i, LOW);
  }
  Serial.begin(115200);
  if(mySimPit.init()) {
    pinMode(13, OUTPUT);
  } else {
    while (true);
  }
}

void loop() {
  digitalWrite(13, LOW);
  mySimPit.send(ECHO_REQ_PACKET, "low", 4);
  delay(1000);
  digitalWrite(13, HIGH);
  mySimPit.send(ECHO_REQ_PACKET, "high", 5);
  delay(1000);
}
