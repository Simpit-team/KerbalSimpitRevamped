/* KerbalSimPitHelloWorld

   Simplest hello world sketch

   Peter Hardy <peter@hardy.dropbear.id.au>
*/

#include "KerbalSimPit.h"

KerbalSimPit mySimPit(115200);

void setup() {
  Serial.begin(115200);
  if(mySimPit.init()) {
    pinMode(13, OUTPUT);
  } else {
    while (true);
  }
}

void loop() {
  digitalWrite(13, LOW);
  // todo, serial echo here
  delay(1000);
  // nothing here yet
  digitalWrite(13, HIGH);
  delay(1000);
}
