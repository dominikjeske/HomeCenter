#pragma once
#include <Arduino.h>

#include "IRLibDecodeBase.h"
#include <IRLibSendBase.h>   
#include "IRLib_P01_NEC.h"
#include "IRLib_P02_Sony.h"
#include "IRLib_P07_NECx.h"
#include "IRLib_P08_Samsung36.h"
#include "IRLibCombo.h"
#include "IRLibRecvPCI.h"
#include "Common.h"

class Infrared
{
  private:
    static IRdecode myDecoder;
    static IRrecvPCI  myReceiver; 
    static IRsend mySender;

  public:
    static void Init();
    static void ProcessLoop();
    static void Send(uint8_t package[], uint8_t packageLength);
};

