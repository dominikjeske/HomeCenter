#pragma once
#include <Arduino.h>

class SerialEx
{
  private:
    static uint8_t DebugMode;
  public:
    static void Init();
    static void SetMode(uint8_t package[], uint8_t packageLength);
    static void SendMessage(const String &message);
};