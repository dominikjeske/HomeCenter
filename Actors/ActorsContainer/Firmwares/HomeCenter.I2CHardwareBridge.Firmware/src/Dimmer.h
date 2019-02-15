#pragma once
#include <Arduino.h>
#include "ACS712.h"

class Dimmer
{
  private:
    static uint8_t Pins[8][2];
    static float Cache[8];
    static uint8_t PinsIndex;
    static ACS712 Readers[8];
    static unsigned long LastMillies;
    static uint8_t GetPinIndex(uint8_t pin);
    static void ReadCurrent();
    static float RawValues[10];
    
  public:
    static void ProcessLoop();
    static void Register(uint8_t package[], uint8_t packageLength);
    static void Test();
};

