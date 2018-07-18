#include <Arduino.h>
#include "WirelessControler.h"
#include "Queue.h"

#define RESPONSE_SIZE_433 5 
#define ACTION_REGISTER_SENSOR 0

WirelessControler _rcs = WirelessControler();
Queue <long> _queue;

bool _recivePinDefinded = false;

union longToBytes
{
	long value;
	struct
	{
		uint8_t b0;
		uint8_t b1;
		uint8_t b2;
		uint8_t b3;
	} bytes;
};

#define DEBUG 1

void LPD433MhzController_handleI2CWrite(uint8_t package[], uint8_t packageLength)
{
	// Example package bytes:
	// 0 = CODE_1
	// 1 = CODE_2
	// 2 = CODE_3
	// 3 = CODE_4
	// 4 = LENGTH
	// 5 = REPEAT_COUNT
	// 6 = PIN
	if (packageLength != 7)
	{
#if (DEBUG)
		Serial.println(F("Received invalid 433MHz package."));
#endif
		return;
	}

	uint8_t data[] = { package[0], package[1], package[2], package[3] };
	uint8_t length = package[4];
	uint8_t repeats = package[5];
	uint8_t pin = package[6];

	int send_code = (package[3] << 24) | (package[2] << 16) | (package[1] << 8) | (package[0]);

	_rcs.SendSignal(send_code, 24, repeats, pin);
}

void LPD433MhzController_SetReadMode(uint8_t package[], uint8_t packageLength)
{
	// Example request bytes:
	// 0 = ACTION
	// 1 = PIN
	if (packageLength != 2) return;

	uint8_t action = package[0];

	switch (action)
	{
	case ACTION_REGISTER_SENSOR:
	{
		if (!_recivePinDefinded)
		{
			uint8_t pin = package[1];

			_rcs.StartListeningOnPin(pin);

			_recivePinDefinded = true;

#if (DEBUG)
			Serial.println("Endable to recive 433 PIN number " + String(pin));
#endif
		}
		break;
	}
	}


}


// Read first element in queue and return remaining elements
uint8_t LPD433MhzController_handleI2CRead(uint8_t response[])
{
	if (_queue.Count() > 0)
	{
		union longToBytes converter;
		converter.value = _queue.Dequeue();

		int remaining = _queue.Count();

#if (DEBUG)
		Serial.println("DNF");
#endif

		response[0] = converter.bytes.b0;
		response[1] = converter.bytes.b1;
		response[2] = converter.bytes.b2;
		response[3] = converter.bytes.b3;
		response[4] = remaining;

	}
	else
	{
		response[0] = 0;
		response[1] = 0;
		response[2] = 0;
		response[3] = 0;
		response[4] = 0;
	}

	return RESPONSE_SIZE_433;
}



void LPD433MhzController_loop()
{
	if (_recivePinDefinded)
	{
		if (_rcs.HaveValue())
		{
			long value = _rcs.GetValue();

			if (value != 0)
			{
				_queue.Enqueue(value);
			}

#if (DEBUG)
			Serial.print("Received 433 code: ");
			Serial.println(value);
#endif

		}

		_rcs.ResetValue();
	}
}
