#include "Dht22Reader.h"
#include "Dht.h"
#include <Arduino.h>
#include "SerialEx.h"
#include "common.h"

#define POLL_INTERVAL 2500UL
#define RESPONSE_SIZE 9UL
#define DIFF_TEMP_MIN_VALUE 0.2
#define DIFF_HUMIDITY_MIN_VALUE 0.3

uint8_t DHT::Pins[16][2];
uint8_t DHT::PinsIndex = 0;
float DHT::Cache[16][2];
unsigned long DHT::LastMillies = millis();

uint8_t DHT::GetPinIndex(uint8_t pin)
{
	for (uint8_t i = 0; i < DHT::PinsIndex; i++)
	{
		if (DHT::Pins[i][0] == pin)
		{
			return i;
		}
	}

	return 255;
}

void DHT::Register(uint8_t package[], uint8_t packageLength)
{
	if (packageLength != 1) return;

	uint8_t pin = package[0];
	uint8_t pinIndex = DHT::GetPinIndex(pin);

	if (pinIndex == 255)
	{
		DHT::Pins[DHT::PinsIndex][0] = pin;
		DHT::Pins[DHT::PinsIndex][1] = 0;

		DHT::Cache[DHT::PinsIndex][0] = 0.0; 
		DHT::Cache[DHT::PinsIndex][1] = 0.0; 

		DHT::PinsIndex++;		
	}
	else
	{
		// If already exists force check on this pin
		DHT::Pins[pinIndex][1] = 1;
	}
}

void DHT::PullSensors()
{
	for (int i = 0; i < DHT::PinsIndex; i++)
	{
		Dht22Reader dht22Reader = Dht22Reader(DHT::Pins[i][0]);
		dht22Reader.setup();
		boolean success = dht22Reader.read();

		float temperature = 0.0F;
		float humidity = 0.0F;

		if (success)
		{
			temperature = dht22Reader.getTemperature();
			humidity = dht22Reader.getHumidity();

			float prev_temperature = DHT::Cache[i][0];
			float prev_humidity= DHT::Cache[i][1];
			uint8_t forceRead = DHT::Pins[i][1]; 

			union FloatToBytes converter;

			if(abs(temperature-prev_temperature) > DIFF_TEMP_MIN_VALUE || forceRead == 1)
			{	
				converter.value = temperature;
				Serial.write(4+1);
				Serial.write(I2C_ACTION_TEMPERATURE);
				Serial.write(DHT::Pins[i][0]);
				Serial.write(converter.bytes.b0);
				Serial.write(converter.bytes.b1);
				Serial.write(converter.bytes.b2);
				Serial.write(converter.bytes.b3);

				DHT::Cache[i][0] = temperature;
			}

			if(abs(humidity-prev_humidity) > DIFF_HUMIDITY_MIN_VALUE || forceRead == 1)
			{
				union FloatToBytes converter;
				converter.value = humidity;

				Serial.write(4+1);
				Serial.write(I2C_ACTION_HUMANITY);
				Serial.write(DHT::Pins[i][0]);
				Serial.write(converter.bytes.b0);
				Serial.write(converter.bytes.b1);
				Serial.write(converter.bytes.b2);
				Serial.write(converter.bytes.b3);

				DHT::Cache[i][1] = humidity;
			}

			if(forceRead)
			{
				DHT::Pins[i][1] = 0;
			}
		}
	}
}


void DHT::ProcessLoop()
{
	unsigned long currentMillies = millis();
	unsigned long elapsedTime = currentMillies - DHT::LastMillies;

	if (elapsedTime > POLL_INTERVAL)
	{
		DHT::LastMillies = currentMillies;
		DHT::PullSensors();
	}
}

