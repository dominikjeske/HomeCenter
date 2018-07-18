
#include "common.h"
#include "SerialEx.h"
#include "Current.h"

#define POLL_INTERVAL 1000UL
#define SAMPLE_TIME 50	
#define VALUE_ON_LEVEL 0.18

uint8_t Current::Pins[8][2];
uint8_t Current::PinsIndex = 0;
uint8_t Current::Cache[8][2];
int Current::Min[8];
int Current::Max[8];
unsigned long Current::LastMillies = millis();

uint8_t Current::GetPinIndex(uint8_t pin)
{
	for (int i = 0; i < Current::PinsIndex; i++)
	{
		if (Current::Pins[i] == pin)
		{
			return i;
		}
	}
	return 255;
}

void Current::Register(uint8_t package[], uint8_t packageLength)
{
	if (packageLength != 1) return;
	
	uint8_t pin = package[0];

	if (Current::GetPinIndex(pin) == 255)
	{
		Current::Pins[Current::PinsIndex][0] = pin;
		Current::Pins[Current::PinsIndex][1] = 0;

		Current::Cache[Current::PinsIndex][0] = 0;
		Current::Cache[Current::PinsIndex][1] = 0; 
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
	for (int i = 0; i < Current::PinsIndex; i++)
	{
		Current::Min[i] = 1024;
		Current::Max[i] = 0;
	}

	uint32_t start_time = millis();
	while ((millis() - start_time) < SAMPLE_TIME)
	{
		for (int i = 0; i < Current::PinsIndex; i++)
		{
			int readValue = analogRead(Current::Pins[i][0]);

			if (readValue > Current::Max[i])
			{
				Current::Max[i] = readValue;
			}
			if (readValue < Current::Min[i])
			{
				Current::Min[i] = readValue;
			}
		}
	}

	for (uint8_t i = 0; i < Current::PinsIndex; i++)
	{
		int current = Current::Max[i] - Current::Min[i];
		float voltage = (current * 5.0) / 1024.0;
		uint8_t value = 0;
		uint8_t forceCheck = 0;
		int last_value = Current::Cache[i][0];
		int last_prop = Current::Cache[i][1];
		
		if(voltage > VALUE_ON_LEVEL)
		{
			value = 1;
		}

		Current::Cache[i][1] = value;
		forceCheck = Current::Pins[i][1];
		
		if((value == last_prop && value != last_value) || forceCheck)
		{
			Current::Cache[i][0] = value;

			Serial.write(sizeof(value) + sizeof(Current::Pins[i][0]));
			Serial.write(I2C_ACTION_Current);
			Serial.write(Current::Pins[i][0]);
			Serial.write(value);
			Serial.flush();

			Current::Pins[i][1] = 0;
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