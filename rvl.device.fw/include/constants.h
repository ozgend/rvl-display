#ifndef RVL_CONSTANTS_H
#define RVL_CONSTANTS_H

#include <cstdint>

struct SensorData
{
  uint8_t cpuTemp;
  uint8_t cpuUtilization;
  uint16_t cpuFan;
  uint8_t gpuTemp;
  uint8_t gpuUtilization;
  uint16_t gpuFan;
  uint16_t chasisFan;
};

bool isSensorDataDifferent(const SensorData &a, const SensorData &b)
{
  return a.cpuTemp != b.cpuTemp ||
         a.cpuUtilization != b.cpuUtilization ||
         a.gpuTemp != b.gpuTemp ||
         a.gpuUtilization != b.gpuUtilization ||
         a.cpuFan != b.cpuFan ||
         a.gpuFan != b.gpuFan ||
         a.chasisFan != b.chasisFan;
}

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

    namespace PayloadType
    {
      static constexpr uint8_t TypeIndex = 0;
      static constexpr uint8_t Data = 0xDD;
      static constexpr uint8_t Command = 0xCC;
    }

    namespace CommandPayloadIndex
    {
      static constexpr uint8_t CommandName = 1;
      static constexpr uint8_t CommandValue = 2;
    }

    namespace ValuePayloadIndex
    {
      static constexpr uint8_t CpuTemp = 1;
      static constexpr uint8_t CpuUtilization = 2;
      static constexpr uint8_t CpuFan = 3; // +1 for int16_t
      static constexpr uint8_t GpuTemp = 5;
      static constexpr uint8_t GpuUtilization = 6;
      static constexpr uint8_t GpuFan = 7;    // +1 for int16_t
      static constexpr uint8_t ChasisFan = 9; // +1 for int16_t
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