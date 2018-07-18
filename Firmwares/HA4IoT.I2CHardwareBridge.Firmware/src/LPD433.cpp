#include <Arduino.h>
#include "SerialEx.h"
#include "LPD433.h"
#include "common.h"

RCSwitch LPD433::mySwitch = RCSwitch();

void LPD433::Init()
{
    LPD433::mySwitch.enableReceive(digitalPinToInterrupt(PIN_RC));
}

void LPD433::Send(uint8_t package[], uint8_t packageLength)
{
	if (packageLength != 7)
	{
		SerialEx::SendMessage(F("Received invalid 433MHz package."));
		return;
	}

    uint8_t length = package[4];
	uint8_t repeats = package[5];
	uint8_t pin = package[6];

	int send_code = (package[3] << 24) | (package[2] << 16) | (package[1] << 8) | (package[0]);

    LPD433::mySwitch.enableTransmit(pin);

	for(int i = 0; i < repeats; i++)
	{
		LPD433::mySwitch.send(send_code, length);
	}
}

void LPD433::ProcessLoop()
{
	if (LPD433::mySwitch.available()) 
	{
    	unsigned long value = LPD433::mySwitch.getReceivedValue();
        uint8_t bit = LPD433::mySwitch.getReceivedBitlength();
        uint8_t protocol = LPD433::mySwitch.getReceivedProtocol();
    	LPD433::mySwitch.resetAvailable();

		uint8_t messageSize = sizeof(value) + sizeof(bit) + sizeof(protocol);

		Serial.write(messageSize);
		Serial.write(I2C_ACTION_433MHz);
		Serial.write((byte*)&value, sizeof(value));
	    Serial.write(bit);
        Serial.write(protocol);
		Serial.flush();
  	}
}
