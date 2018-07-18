#pragma once
#include <Arduino.h>

class DHT
{
  private:
    static uint8_t Pins[16][2];
    static uint8_t PinsIndex;
    static float Cache[16][2];
    static unsigned long LastMillies;
    static uint8_t GetPinIndex(uint8_t pin);
    static void PullSensors();
  public:
    static void ProcessLoop();
    static void Register(uint8_t package[], uint8_t packageLength);
};

