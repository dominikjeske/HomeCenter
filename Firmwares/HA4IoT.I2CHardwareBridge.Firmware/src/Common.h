#pragma once
#include <Arduino.h>

#define I2C_SLAVE_ADDRESS 50

#define PIN_LED 13
#define PIN_IR 2
#define PIN_RC 3

#define I2C_ACTION_TEMPERATURE 1
#define I2C_ACTION_433MHz 2
#define I2C_ACTION_Infrared 3
#define I2C_ACTION_Infrared_RAW 4
#define I2C_ACTION_Current 5
#define I2C_ACTION_HUMANITY 6
#define I2C_ACTION_DEBUG 10

union FloatToBytes
{
	float value;
	struct
	{
		uint8_t b0;
		uint8_t b1;
		uint8_t b2;
		uint8_t b3;
	} bytes;
};

union ArrayToInteger
{
  byte array[4];
  uint32_t value;
};
