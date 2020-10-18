
#include "common.h"
#include "SerialEx.h"
#include "Dimmer.h"

#define POLL_INTERVAL 500UL

uint8_t Dimmer::Pins[8][2];
float Dimmer::Cache[8];
ACS712 Dimmer::Readers[8];
uint8_t Dimmer::PinsIndex = 0;
float Dimmer::RawValues[10];


unsigned long Dimmer::LastMillies = millis();

uint8_t Dimmer::GetPinIndex(uint8_t pin)
{
	for (int i = 0; i < Dimmer::PinsIndex; i++)
	{
		if (Dimmer::Pins[i][0] == pin)
		{
			return i;
		}
	}
	return 255;
}

void Dimmer::Test()
{
	Dimmer::Pins[Dimmer::PinsIndex][0] = A0;
	Dimmer::Pins[Dimmer::PinsIndex][1] = 0;
	Dimmer::Cache[Dimmer::PinsIndex] = 0.0;
	Dimmer::Readers[Dimmer::PinsIndex] = ACS712();
	Dimmer::Readers[Dimmer::PinsIndex].calibrate(ACS723, A0);

	Dimmer::PinsIndex++;
}

void Dimmer::Register(uint8_t package[], uint8_t packageLength)
{
	if (packageLength != 1) return;
	
	uint8_t pin = package[0] + 14;  // A0 = 14, A1 = 15 so we add 14 to zero base pin number

	if (Dimmer::GetPinIndex(pin) == 255)
	{
		Dimmer::Pins[Dimmer::PinsIndex][0] = pin;
		Dimmer::Pins[Dimmer::PinsIndex][1] = 0;
		Dimmer::Cache[Dimmer::PinsIndex] = 0.0;
        Dimmer::Readers[Dimmer::PinsIndex] = ACS712();
		Dimmer::Readers[Dimmer::PinsIndex].calibrate(ACS723, pin);

		Dimmer::PinsIndex++;
	}
	else
	{
		// If Exists force check
		Dimmer::Pins[Dimmer::PinsIndex][1] = 1;
	}

}

void Dimmer::ReadCurrent()
{
	union FloatToBytes converter;

	for (uint8_t i = 0; i < Dimmer::PinsIndex; i++)
	{
		float lastValue = Dimmer::Cache[i];
		float maxDiff = 0.0;
        uint8_t forceCheck = Dimmer::Pins[i][1];
		Dimmer::Pins[i][1] = 0;                      // Reset force pin

		// Read 10 samples for getting mean
		for(int j = 0; j < 10; j++)
		{
			float readValue = Dimmer::Readers[i].getCurrentAC();
			Dimmer::RawValues[j] = readValue;
			if(j > 0)
			{
				// Search for maximum diffrences between current and prevous sample
				// This value is needed becouse on diffrent currents diffrents beetween readings have diffrent scale
				float d = ((abs(readValue - Dimmer::RawValues[j-1]) / Dimmer::RawValues[j-1]) * 100.0);
            	maxDiff = max(maxDiff, d);
			}
		}
		
		// Calculate mean as current value
		float mean = 0;
		for(int j = 0; j < 10; j++)
		{
			mean += Dimmer::RawValues[j]; 
		}
		mean = mean / 10.0;
        Dimmer::Cache[i] = mean;
		
		// calculate diffrence between current value and prevous
		float currentDiff = ((abs(mean - lastValue) / lastValue) * 100.0);
		// To check if current signal change is actual change we have to comper it with maximum
		// when value is below one we have standard changes that are result of inaccuracy of measurements
		// When the value is above 1 we know that we have real change
		float changeFactor = currentDiff / maxDiff;

		if(changeFactor > 1 || forceCheck)
		{
			converter.value = mean;

			Serial.write(4 + 1);
			Serial.write(I2C_ACTION_Dimmer);
			Serial.write(Dimmer::Pins[i][0] - 14); // A0 = 14, A1 = 15 so we substract 14 to zero base pin number
			Serial.write(converter.bytes.b0);
			Serial.write(converter.bytes.b1);
			Serial.write(converter.bytes.b2);
			Serial.write(converter.bytes.b3);
			Serial.flush();
		}
	}
}

void Dimmer::ProcessLoop()
{
	unsigned long currentMillies = millis();
	unsigned long elapsedTime = currentMillies - Dimmer::LastMillies;

	if (elapsedTime > POLL_INTERVAL)
	{
		Dimmer::LastMillies = currentMillies;
		Dimmer::ReadCurrent();
	}
}