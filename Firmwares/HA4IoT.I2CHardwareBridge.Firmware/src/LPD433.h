#pragma once
#include <Arduino.h>
#include <RCSwitch.h>

class LPD433
{
  private:
    static RCSwitch mySwitch;
  public:
    static void Init();
    static void ProcessLoop();
    static void Send(uint8_t package[], uint8_t packageLength);
};


