/* KerbalSimPitHelloWorld
   Basic test of KSPit capabilities

   This sketch is still using the (very) old initial prototype code.

   Peter Hardy <peter@hardy.dropbear.id.au>
*/

#include "KerbalSimPit.h"

// Blinkin
const int blinkPin = 13;

const int blinkDelay = 2000;
unsigned long timer = 0;
int blinkState = LOW;

void setup() {
  pinMode(blinkPin, OUTPUT);
  digitalWrite(blinkPin, LOW);
  Serial.begin(115200);
}

void loop() {
  unsigned long now = millis();
  if (now > timer) {
    digitalWrite(blinkPin, LOW);
  }
  while (Serial.available()) {
    char x = Serial.read();
    if (x == 'K') {
      turnOn();
    }
  }
}

void turnOn() {
  unsigned long now = millis();
  digitalWrite(blinkPin, HIGH);
  timer = now + blinkDelay;
}
