
#include "common.h"
#include "SerialEx.h"
#include "Current.h"

#define POLL_INTERVAL 1000UL

uint8_t Current::Pins[8][2];
float Current::Cache[8];
ACS712 Current::Readers[8];
uint8_t Current::PinsIndex = 0;

unsigned long Current::LastMillies = millis();

uint8_t Current::GetPinIndex(uint8_t pin)
{
	for (int i = 0; i < Current::PinsIndex; i++)
	{
		if (Current::Pins[i][0] == pin)
		{
			return i;
		}
	}
	return 255;
}

void Current::Register(uint8_t package[], uint8_t packageLength)
{
	if (packageLength != 1) return;
	
	uint8_t pin = package[0] + 14;  // A0 = 14, A1 = 15 so we add 14 to zero base pin number

	if (Current::GetPinIndex(pin) == 255)
	{
		Current::Pins[Current::PinsIndex][0] = pin;
		Current::Pins[Current::PinsIndex][1] = 0;
		Current::Cache[Current::PinsIndex] = 0.0;
        Current::Readers[Current::PinsIndex] = ACS712();
		Current::Readers[Current::PinsIndex].calibrate(ACS723, pin);

		Current::PinsIndex++;
	}
	else
	{
		// If Exists force check
		Current::Pins[Current::PinsIndex][1] = 1;
	}

}

void Current::ReadCurrent()
{
	union FloatToBytes converter;

	for (uint8_t i = 0; i < Current::PinsIndex; i++)
	{
		float readValue = Current::Readers[i].getCurrentAC();

		if(readValue < ZERO_LEVEL)    // current below 100mA will be treated as 0
		{
			readValue = 0.0;
		}

		float lastValue = Current::Cache[i];
		uint8_t forceCheck = Current::Pins[i][1];
		Current::Pins[i][1] = 0;        // Reset force pin

        if(lastValue == 0.00 && readValue > 0.00 || ((abs(readValue - lastValue) / lastValue) * 100.0) > MIN_DIFF || forceCheck)
		{
			Current::Cache[i] = readValue;
			converter.value = readValue;

			Serial.write(4 + 1);
			Serial.write(I2C_ACTION_Current);
			Serial.write(Current::Pins[i][0] - 14); // A0 = 14, A1 = 15 so we substract 14 to zero base pin number
			Serial.write(converter.bytes.b0);
			Serial.write(converter.bytes.b1);
			Serial.write(converter.bytes.b2);
			Serial.write(converter.bytes.b3);
			Serial.flush();
		} 
	}
}

void Current::ProcessLoop()
{
	unsigned long currentMillies = millis();
	unsigned long elapsedTime = currentMillies - Current::LastMillies;

	if (elapsedTime > POLL_INTERVAL)
	{
		Current::LastMillies = currentMillies;
		Current::ReadCurrent();
	}
}