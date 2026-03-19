#ifndef RVL_DISPLAY_LAYOUT_H
#define RVL_DISPLAY_LAYOUT_H

#include "Free_Fonts.h"
#include "constants.h"
#include "models.h"
#include <TFT_eSPI.h>

const int COLOR_CPU = 0x5FA;
const int COLOR_GPU = 0xF206;

// -- sprite sizes
static const byte LAYOUT_SPR_FAN_W = 48;
static const byte LAYOUT_SPR_FAN_H = 16;
static const byte LAYOUT_SPR_TEMP_W = 44;
static const byte LAYOUT_SPR_TEMP_H = 29;
static const byte LAYOUT_SPR_UTIL_W = 50;
static const byte LAYOUT_SPR_UTIL_H = 18;

// -- sprite-internal text offsets
static const byte LAYOUT_SPR_FAN_TXT_X = 2;
static const byte LAYOUT_SPR_FAN_TXT_Y = 0;
static const byte LAYOUT_SPR_TEMP_TXT_X = 5;
static const byte LAYOUT_SPR_TEMP_TXT_Y = 0;
static const byte LAYOUT_SPR_UTIL_TXT_X = 1;
static const byte LAYOUT_SPR_UTIL_TXT_Y = 1;

// -- cpu value positions (screen)
static const byte LAYOUT_CPU_FAN_VAL_X = 30;
static const byte LAYOUT_CPU_FAN_VAL_Y = 7;
static const byte LAYOUT_CPU_TEMP_VAL_X = 80;
static const byte LAYOUT_CPU_TEMP_VAL_Y = 0;
static const byte LAYOUT_CPU_UTIL_VAL_X = 79;
static const byte LAYOUT_CPU_UTIL_VAL_Y = 34;

// -- gpu value positions (screen)
static const byte LAYOUT_GPU_FAN_VAL_X = 30;
static const byte LAYOUT_GPU_FAN_VAL_Y = 77;
static const byte LAYOUT_GPU_TEMP_VAL_X = 80;
static const byte LAYOUT_GPU_TEMP_VAL_Y = 70;
static const byte LAYOUT_GPU_UTIL_VAL_X = 79;
static const byte LAYOUT_GPU_UTIL_VAL_Y = 104;

// -- cpu static elements
static const byte LAYOUT_CPU_ICON_X = 4;
static const byte LAYOUT_CPU_ICON_Y = 10;
static const byte LAYOUT_CPU_ICON_W = 20;
static const byte LAYOUT_CPU_ICON_H = 20;
static const byte LAYOUT_CPU_FAN_UNIT_X = 30;
static const byte LAYOUT_CPU_FAN_UNIT_Y = 28;
static const byte LAYOUT_CPU_TEMP_DOT_X = 124;
static const byte LAYOUT_CPU_TEMP_DOT_Y = 5;
static const byte LAYOUT_CPU_TEMP_DOT_R = 3;

// -- gpu static elements
static const byte LAYOUT_GPU_ICON_X = 4;
static const byte LAYOUT_GPU_ICON_Y = 77;
static const byte LAYOUT_GPU_ICON_W = 20;
static const byte LAYOUT_GPU_ICON_H = 38;
static const byte LAYOUT_GPU_ICON_BAR_X = 2;
static const byte LAYOUT_GPU_ICON_BAR_Y = 79;
static const byte LAYOUT_GPU_ICON_BAR_W = 3;
static const byte LAYOUT_GPU_ICON_BAR_H = 23;
static const byte LAYOUT_GPU_ICON_SCREW_X = 14;
static const byte LAYOUT_GPU_ICON_SCREW_TOP_Y = 87;
static const byte LAYOUT_GPU_ICON_SCREW_BOT_Y = 105;
static const byte LAYOUT_GPU_ICON_SCREW_R = 6;
static const byte LAYOUT_GPU_FAN_UNIT_X = 30;
static const byte LAYOUT_GPU_FAN_UNIT_Y = 98;
static const byte LAYOUT_GPU_TEMP_DOT_X = 124;
static const byte LAYOUT_GPU_TEMP_DOT_Y = 75;
static const byte LAYOUT_GPU_TEMP_DOT_R = 3;

// -- message bar
static const byte LAYOUT_MSG_X = 0;
static const byte LAYOUT_MSG_Y = 152;
static const byte LAYOUT_MSG_W = 128;
static const byte LAYOUT_MSG_H = 8;

TFT_eSprite *sprTempCpu = nullptr;
TFT_eSprite *sprTempGpu = nullptr;
TFT_eSprite *sprFanCpu = nullptr;
TFT_eSprite *sprFanGpu = nullptr;
TFT_eSprite *sprUtilCpu = nullptr;
TFT_eSprite *sprUtilGpu = nullptr;

void initSprites(TFT_eSPI &tft)
{
  sprTempCpu = new TFT_eSprite(&tft);
  sprTempCpu->createSprite(LAYOUT_SPR_TEMP_W, LAYOUT_SPR_TEMP_H);
  sprTempGpu = new TFT_eSprite(&tft);
  sprTempGpu->createSprite(LAYOUT_SPR_TEMP_W, LAYOUT_SPR_TEMP_H);
  sprFanCpu = new TFT_eSprite(&tft);
  sprFanCpu->createSprite(LAYOUT_SPR_FAN_W, LAYOUT_SPR_FAN_H);
  sprFanGpu = new TFT_eSprite(&tft);
  sprFanGpu->createSprite(LAYOUT_SPR_FAN_W, LAYOUT_SPR_FAN_H);
  sprUtilCpu = new TFT_eSprite(&tft);
  sprUtilCpu->createSprite(LAYOUT_SPR_UTIL_W, LAYOUT_SPR_UTIL_H);
  sprUtilGpu = new TFT_eSprite(&tft);
  sprUtilGpu->createSprite(LAYOUT_SPR_UTIL_W, LAYOUT_SPR_UTIL_H);
}

void drawValues(RvlMonitorDataStruct &data)
{
  char buffer[10];

  // cpuFan
  sprFanCpu->fillSprite(TFT_BLACK);
  sprFanCpu->setTextColor(TFT_CYAN);
  sprFanCpu->setFreeFont(&FreeSans9pt7b);
  sprintf(buffer, "%4d", data.cpuFan);
  sprFanCpu->drawString(buffer, LAYOUT_SPR_FAN_TXT_X, LAYOUT_SPR_FAN_TXT_Y);
  sprFanCpu->pushSprite(LAYOUT_CPU_FAN_VAL_X, LAYOUT_CPU_FAN_VAL_Y);

  // cpuTemp
  sprTempCpu->fillSprite(TFT_BLACK);
  sprTempCpu->setTextColor(COLOR_CPU);
  sprTempCpu->setFreeFont(&FreeSans18pt7b);
  sprintf(buffer, "%2d", data.cpuTemp);
  sprTempCpu->drawString(buffer, LAYOUT_SPR_TEMP_TXT_X, LAYOUT_SPR_TEMP_TXT_Y);
  sprTempCpu->pushSprite(LAYOUT_CPU_TEMP_VAL_X, LAYOUT_CPU_TEMP_VAL_Y);

  // cpuUtil
  sprUtilCpu->fillSprite(TFT_BLACK);
  sprUtilCpu->setTextColor(TFT_SILVER);
  sprUtilCpu->setFreeFont(&FreeSans9pt7b);
  sprintf(buffer, "%3d %%", data.cpuUtilization);
  sprUtilCpu->drawString(buffer, LAYOUT_SPR_UTIL_TXT_X, LAYOUT_SPR_UTIL_TXT_Y);
  sprUtilCpu->pushSprite(LAYOUT_CPU_UTIL_VAL_X, LAYOUT_CPU_UTIL_VAL_Y);

  // gpuFan
  sprFanGpu->fillSprite(TFT_BLACK);
  sprFanGpu->setTextColor(TFT_PINK);
  sprFanGpu->setFreeFont(&FreeSans9pt7b);
  sprintf(buffer, "%4d", data.gpuFan);
  sprFanGpu->drawString(buffer, LAYOUT_SPR_FAN_TXT_X, LAYOUT_SPR_FAN_TXT_Y);
  sprFanGpu->pushSprite(LAYOUT_GPU_FAN_VAL_X, LAYOUT_GPU_FAN_VAL_Y);

  // gpuTemp
  sprTempGpu->fillSprite(TFT_BLACK);
  sprTempGpu->setTextColor(COLOR_GPU);
  sprTempGpu->setFreeFont(&FreeSans18pt7b);
  sprintf(buffer, "%2d", data.gpuTemp);
  sprTempGpu->drawString(buffer, LAYOUT_SPR_TEMP_TXT_X, LAYOUT_SPR_TEMP_TXT_Y);
  sprTempGpu->pushSprite(LAYOUT_GPU_TEMP_VAL_X, LAYOUT_GPU_TEMP_VAL_Y);

  // gpuUtil
  sprUtilGpu->fillSprite(TFT_BLACK);
  sprUtilGpu->setTextColor(TFT_SILVER);
  sprUtilGpu->setFreeFont(&FreeSans9pt7b);
  sprintf(buffer, "%3d %%", data.gpuUtilization);
  sprUtilGpu->drawString(buffer, LAYOUT_SPR_UTIL_TXT_X, LAYOUT_SPR_UTIL_TXT_Y);
  sprUtilGpu->pushSprite(LAYOUT_GPU_UTIL_VAL_X, LAYOUT_GPU_UTIL_VAL_Y);
}

void clearMessage(TFT_eSPI &tft)
{
  tft.fillRect(LAYOUT_MSG_X, LAYOUT_MSG_Y, LAYOUT_MSG_W, LAYOUT_MSG_H, TFT_BLACK);
}

void drawMessage(TFT_eSPI &tft, uint16_t color, const char *message)
{
  clearMessage(tft);
  tft.setTextColor(color, TFT_BLACK);
  tft.setFreeFont();
  tft.drawString(message, LAYOUT_MSG_X, LAYOUT_MSG_Y);
}

// static text and logos
void drawStatic(TFT_eSPI &tft)
{
  tft.fillScreen(TFT_BLACK);
  tft.setFreeFont();
  tft.setTextColor(TFT_SILVER);

  // cpu
  tft.fillRect(LAYOUT_CPU_ICON_X, LAYOUT_CPU_ICON_Y, LAYOUT_CPU_ICON_W, LAYOUT_CPU_ICON_H, COLOR_CPU);
  tft.drawString("rpm", LAYOUT_CPU_FAN_UNIT_X, LAYOUT_CPU_FAN_UNIT_Y);
  tft.fillEllipse(LAYOUT_CPU_TEMP_DOT_X, LAYOUT_CPU_TEMP_DOT_Y, LAYOUT_CPU_TEMP_DOT_R, LAYOUT_CPU_TEMP_DOT_R, COLOR_CPU);

  // gpu
  tft.drawRect(LAYOUT_GPU_ICON_X, LAYOUT_GPU_ICON_Y, LAYOUT_GPU_ICON_W, LAYOUT_GPU_ICON_H, COLOR_GPU);
  tft.fillRect(LAYOUT_GPU_ICON_BAR_X, LAYOUT_GPU_ICON_BAR_Y, LAYOUT_GPU_ICON_BAR_W, LAYOUT_GPU_ICON_BAR_H, COLOR_GPU);
  tft.fillEllipse(LAYOUT_GPU_ICON_SCREW_X, LAYOUT_GPU_ICON_SCREW_BOT_Y, LAYOUT_GPU_ICON_SCREW_R, LAYOUT_GPU_ICON_SCREW_R, COLOR_GPU);
  tft.fillEllipse(LAYOUT_GPU_ICON_SCREW_X, LAYOUT_GPU_ICON_SCREW_TOP_Y, LAYOUT_GPU_ICON_SCREW_R, LAYOUT_GPU_ICON_SCREW_R, COLOR_GPU);
  tft.drawString("rpm", LAYOUT_GPU_FAN_UNIT_X, LAYOUT_GPU_FAN_UNIT_Y);
  tft.fillEllipse(LAYOUT_GPU_TEMP_DOT_X, LAYOUT_GPU_TEMP_DOT_Y, LAYOUT_GPU_TEMP_DOT_R, LAYOUT_GPU_TEMP_DOT_R, COLOR_GPU);
}

#endif
