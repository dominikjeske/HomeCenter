#include "SerialEx.h"
#include "common.h"

#define ACTION_REGISTER_MODE 0

uint8_t SerialEx::DebugMode = 0;

void SerialEx::Init()
{
	Serial.begin(115200);
}

void SerialEx::SetMode(uint8_t package[], uint8_t packageLength)
{
	if (packageLength != 2) return;
	uint8_t action = package[0];

	switch (action)
	{
		case ACTION_REGISTER_MODE:
		{
			SerialEx::DebugMode = package[1];
		}
	}
}

void SerialEx::SendMessage(const String &message)
{
	if(SerialEx::DebugMode == 1)
	{
		String localMessage;
		if(message.length() > 256)
		{
			localMessage = message.substring(0, 252) + "...";
		}
		else
		{
			localMessage = message;
		}
		byte length = localMessage.length();

		Serial.write(length);
		Serial.write(I2C_ACTION_DEBUG);

		Serial.print(localMessage);
		Serial.flush();
	}
}




