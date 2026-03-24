#ifndef RVL_MODELS_H
#define RVL_MODELS_H

#include <cstdint>
#include <stdint.h>
#include <string.h>

struct __attribute__((packed)) RvlMonitorDataStruct
{
  uint8_t cpuTemp;
  uint8_t gpuTemp;
  uint8_t cpuUtilization;
  uint8_t gpuUtilization;
  uint16_t cpuFan;
  uint16_t gpuFan;
  uint16_t chasisFan;
};

struct __attribute__((packed)) RvlCommandDataStruct
{
  uint8_t commandName;
  uint8_t commandValue;
};

bool isMonitorDataDifferent(const RvlMonitorDataStruct &a, const RvlMonitorDataStruct &b)
{
  return a.cpuTemp != b.cpuTemp ||
         a.gpuTemp != b.gpuTemp ||
         a.cpuUtilization != b.cpuUtilization ||
         a.gpuUtilization != b.gpuUtilization ||
         a.cpuFan != b.cpuFan ||
         a.gpuFan != b.gpuFan ||
         a.chasisFan != b.chasisFan;
}

#endif