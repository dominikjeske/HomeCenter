#include "Infrared.h"
#include "SerialEx.h"
#include "common.h"

IRrecvPCI Infrared::myReceiver = IRrecvPCI(PIN_IR);
IRdecode Infrared::myDecoder;
IRsend Infrared::mySender = IRsend();

void Infrared::Init()
{
	Infrared::myReceiver.enableIRIn();
}

void Infrared::ProcessLoop()
{
	if (Infrared::myReceiver.getResults()) 
	{
		uint8_t codeProtocol = UNKNOWN;
		uint32_t codeValue = 0;
		uint8_t codeBits = 0;

    	myDecoder.decode();
		codeProtocol = myDecoder.protocolNum;

		if(codeProtocol == UNKNOWN)
		{
			// uint8_t messageSize = 0;
			// for(bufIndex_t i=1; i<recvGlobal.recvLength; i++) 
			// {
			// 	messageSize += sizeof(recvGlobal.recvBuffer[i]);
			// }
			
			// Serial.write(messageSize);
			// Serial.write(I2C_ACTION_Infrared_RAW);
           
			// for(bufIndex_t i=1; i<recvGlobal.recvLength; i++) 
			// {
			// 	Serial.print(recvGlobal.recvBuffer[i]);
			// }
		}
		else 
		{
    		if (myDecoder.value == REPEAT_CODE) 
			{
      			// Don't record a NEC repeat value as that's useless.
    		}
			else 
			{
      			codeValue = myDecoder.value;
      			codeBits = myDecoder.bits;

				uint8_t messageSize = sizeof(codeProtocol)+sizeof(codeValue)+sizeof(codeBits);
				Serial.write(messageSize);
				Serial.write(I2C_ACTION_Infrared);
				Serial.write(codeProtocol);
				Serial.write((byte*)&codeValue, sizeof(codeValue));
				Serial.write(codeBits);
				Serial.flush();
    		}
  		}
    	myReceiver.enableIRIn();
  	}
}

void Infrared::Send(uint8_t package[], uint8_t packageLength)
{
	if (packageLength != 7)
	{
		SerialEx::SendMessage(F("Received invalid infrared package."));
		return;
	}

	uint8_t repeats = package[0];
	uint8_t sys = package[1];
	uint8_t bits = package[2];

    ArrayToInteger converter;
	converter.array[0] = package[3];
	converter.array[1] = package[4];
	converter.array[2] = package[5];
	converter.array[3] =package[6];

    myReceiver.disableIRIn();

	for(byte i=0; i<repeats; i++)
	{
		mySender.send(sys, converter.value, bits);
	}

	myReceiver.enableIRIn();
}