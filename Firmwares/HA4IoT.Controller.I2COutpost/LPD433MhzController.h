#pragma once
#include <Arduino.h>


void LPD433MhzController_handleI2CWrite(uint8_t package[], uint8_t packageLength);
uint8_t LPD433MhzController_handleI2CRead(uint8_t response[]);
void LPD433MhzController_SetReadMode(uint8_t package[], uint8_t packageLength);
void LPD433MhzController_loop();

