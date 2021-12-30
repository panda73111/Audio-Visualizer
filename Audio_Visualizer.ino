#include <FastLED.h>
#define NUM_LEDS 100
#define DATA_PIN 3

CRGB leds[NUM_LEDS];

void setup() {
  //Serial.begin(2000000);
  //FastLED.addLeds<WS2812B, DATA_PIN, RGB>(leds, NUM_LEDS);
  pinMode(DATA_PIN, OUTPUT);
}

void loop() {
  /*
  // Turn the LED on, then pause
  leds[0] = CRGB::Red;
  FastLED.show();
  delay(500);
  // Now turn the LED off, then pause
  leds[0] = CRGB::Black;
  FastLED.show();
  delay(500);
  */

  for (int i = 0; i < 1000; i++) {
    digitalWrite(DATA_PIN, HIGH);
    digitalWrite(DATA_PIN, LOW);
  }
  delay(1000);
}
