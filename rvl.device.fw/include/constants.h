#ifndef RVL_CONSTANTS_H
#define RVL_CONSTANTS_H

namespace Constants
{

  namespace DeviceInfo
  {
    static constexpr uint16_t VendorId = 0x5EED;
    static constexpr uint16_t ProductId = 0xFACE;
    static constexpr char VendorName[] = "denolk";
    static constexpr char ProductName[] = "rvl-display";
    static constexpr char SerialNumber[] = "R0666";
  }

  namespace Report
  {
    static constexpr uint8_t Length = 64;
    static constexpr uint8_t Null = 0x00;
    static constexpr uint8_t Ok = 0xAA;
    static constexpr uint8_t Error = 0xEE;

    namespace Type
    {
      static constexpr uint8_t Data = 0xDD;
      static constexpr uint8_t Command = 0xCC;
    }
    
    namespace Index
    {
      static constexpr uint8_t Type = 0;
    }

    namespace Command
    {
      static constexpr uint8_t MessageClear = 0x10;
      static constexpr uint8_t MessageAwait = 0x1A;
      static constexpr uint8_t MessageCheckHost = 0x1C;
      static constexpr uint8_t MessageHighTemp = 0x17;
      static constexpr uint8_t MessageLowRpm = 0x19;

      static constexpr uint8_t ClearDisplay = 0xD0;
      static constexpr uint8_t SetBrightnessOff = 0xB0;
      static constexpr uint8_t SetBrightnessLow = 0xB1;
      static constexpr uint8_t SetBrightnessMedium = 0xB2;
      static constexpr uint8_t SetBrightnessHigh = 0xB3;

      static constexpr uint8_t LedOff = 0xC0;
      static constexpr uint8_t LedOn = 0xC1;

      static constexpr uint8_t RestartDevice = 0xFF;
      static constexpr uint8_t EnterBootloader = 0x77;
    }
  }
}

#endif