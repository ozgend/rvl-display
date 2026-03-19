#include "constants.h"
#include "models.h"
#include <TFT_eSPI.h>

const int COLOR_CPU = 0x5FA;
const int COLOR_GPU = 0xF206;

void drawValues(TFT_eSPI &tft, RvlMonitorDataStruct &data)
{
  char buffer[10];

  // cpuFanValue
  sprintf(buffer, "%4d", data.cpuFan);
  tft.setTextColor(TFT_CYAN);
  tft.setFreeFont(&FreeSans9pt7b);
  tft.fillRect(28, 6, 48, 16, TFT_BLACK);
  tft.drawString(buffer, 30, 7);

  // cpuTempValue
  buffer[0] = '\0';
  sprintf(buffer, "%2d", data.cpuTemp);
  tft.setTextColor(COLOR_CPU);
  tft.setFreeFont(&FreeSans18pt7b);
  tft.fillRect(77, 0, 44, 29, TFT_BLACK);
  tft.drawString(buffer, 82, 0);

  // cpuUtilVar
  buffer[0] = '\0';
  sprintf(buffer, "%3d %%", data.cpuUtilization);
  tft.setTextColor(TFT_SILVER);
  tft.setFreeFont(&FreeSans9pt7b);
  tft.fillRect(78, 33, 50, 18, TFT_BLACK);
  tft.drawString(buffer, 79, 34);

  // gpuFanValue
  buffer[0] = '\0';
  sprintf(buffer, "%4d", data.gpuFan);
  tft.setTextColor(TFT_PINK);
  tft.setFreeFont(&FreeSans9pt7b);
  tft.fillRect(28, 77, 48, 16, TFT_BLACK);
  tft.drawString(buffer, 30, 77);

  // gpuTempValue
  buffer[0] = '\0';
  sprintf(buffer, "%2d", data.gpuTemp);
  tft.setTextColor(COLOR_GPU);
  tft.setFreeFont(&FreeSans18pt7b);
  tft.fillRect(77, 70, 44, 29, TFT_BLACK);
  tft.drawString(buffer, 82, 70);

  // gpuUtilVar
  buffer[0] = '\0';
  sprintf(buffer, "%3d %%", data.gpuUtilization);
  tft.setTextColor(TFT_SILVER);
  tft.setFreeFont(&FreeSans9pt7b);
  tft.fillRect(78, 102, 50, 18, TFT_BLACK);
  tft.drawString(buffer, 79, 104);
}

void clearMessage(TFT_eSPI &tft)
{
  tft.fillRect(0, 152, 128, 8, TFT_BLACK);
}

void drawMessage(TFT_eSPI &tft, uint16_t color, const char *message)
{
  clearMessage(tft);
  tft.setTextColor(color, TFT_BLACK);
  tft.setFreeFont();
  tft.drawString(message, 0, 152);
}

// static text and logos
void drawStatic(TFT_eSPI &tft)
{
  tft.fillScreen(TFT_BLACK);
  tft.setFreeFont();
  tft.setTextColor(TFT_SILVER);

  // cpu
  // -- icon
  tft.fillRect(4, 10, 20, 20, COLOR_CPU);
  // -- fan unit
  tft.drawString("rpm", 30, 28);
  // -- temp unit
  tft.fillEllipse(124, 5, 3, 3, COLOR_CPU);

  // gpu
  // -- icon
  tft.drawRect(4, 77, 20, 38, COLOR_GPU);
  tft.fillRect(2, 79, 3, 23, COLOR_GPU);
  tft.fillEllipse(14, 105, 6, 6, COLOR_GPU);
  tft.fillEllipse(14, 87, 6, 6, COLOR_GPU);
  // -- fan unit
  tft.drawString("rpm", 30, 98);
  // -- temp unit
  tft.fillEllipse(124, 75, 3, 3, COLOR_GPU);
}
