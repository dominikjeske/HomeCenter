#pragma once
#include <Arduino.h>

class Current
{
  private:
    static uint8_t Pins[8][2];
    static uint8_t PinsIndex;
    static uint8_t Cache[8][2];
    static int Min[8];
    static int Max[8];
    static unsigned long LastMillies;
    static uint8_t GetPinIndex(uint8_t pin);
    static void ReadCurrent();

  public:
    static void ProcessLoop();
    static void Register(uint8_t package[], uint8_t packageLength);
};

