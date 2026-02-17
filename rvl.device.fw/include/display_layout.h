#include "constants.h"
#include <TFT_eSPI.h>

static const unsigned char PROGMEM image_temperature[] = {0x1c, 0x00, 0x22, 0x02, 0x2b, 0x05, 0x2a, 0x02, 0x2b, 0x38, 0x2a, 0x60, 0x2b, 0x40, 0x2a, 0x40, 0x2a, 0x60, 0x49, 0x38, 0x9c, 0x80, 0xae, 0x80, 0xbe, 0x80, 0x9c, 0x80, 0x41, 0x00, 0x3e, 0x00};

void drawValues(TFT_eSPI &tft, SensorData data)
{
  char buffer[10];

  // cpuTempValue
  sprintf(buffer, "%2d", data.cpuTemp);
  tft.setTextColor(0x86DF, TFT_BLACK);
  tft.setFreeFont(&FreeSans18pt7b);
  tft.drawString(buffer, 56, 0);

  // cpuUtilVar
  buffer[0] = '\0';
  sprintf(buffer, "%3d %%", data.cpuUtilization);
  tft.setTextColor(TFT_SILVER, TFT_BLACK);
  tft.setFreeFont(&FreeSans9pt7b);
  tft.drawString(buffer, 56, 34);

  // gpuTempValue
  buffer[0] = '\0';
  sprintf(buffer, "%2d", data.gpuTemp);
  tft.setTextColor(0xF206, TFT_BLACK);
  tft.setFreeFont(&FreeSans18pt7b);
  tft.drawString(buffer, 56, 70);

  // gpuUtilVar
  buffer[0] = '\0';
  sprintf(buffer, "%3d %%", data.gpuUtilization);
  tft.setTextColor(TFT_SILVER, TFT_BLACK);
  tft.setFreeFont(&FreeSans9pt7b);
  tft.drawString(buffer, 56, 104);
}

void clearMessage(TFT_eSPI &tft)
{
  tft.fillRect(0, 142, 128, 20, TFT_BLACK);
}

void drawMessage(TFT_eSPI &tft, uint16_t color, const char *message)
{
  clearMessage(tft);
  tft.setTextColor(color, TFT_BLACK);
  tft.setFreeFont(&FreeSans9pt7b);
  tft.drawString(message, 0, 142);
}

// static text and logos
void drawStatic(TFT_eSPI &tft)
{
  tft.fillScreen(TFT_BLACK);

  // temperature gauge
  tft.drawEllipse(102, 5, 3, 3, 0x5FA);
  tft.drawBitmap(108, 11, image_temperature, 16, 16, 0xFF47);
  tft.drawEllipse(102, 75, 3, 3, 0xF206);
  tft.drawBitmap(108, 81, image_temperature, 16, 16, 0xCEE7);

  // cpu icon
  tft.drawRoundRect(9, 10, 36, 36, 4, 0x5FA);
  tft.fillRect(17, 18, 20, 20, 0x5FA);

  // gpu icon
  tft.drawRect(12, 77, 22, 38, 0xF206);
  tft.fillEllipse(23, 87, 6, 6, 0xF206);
  tft.fillEllipse(23, 105, 6, 6, 0xF206);
  tft.fillRect(34, 89, 6, 23, 0xF206);
  tft.drawLine(37, 117, 4, 117, 0xF206);
}
