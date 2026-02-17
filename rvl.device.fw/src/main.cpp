#include "constants.h"
#include <Arduino.h>
#include "hardware/watchdog.h"
#include <stdarg.h>
#include <Adafruit_TinyUSB.h>
#include <SPI.h>
#include <TFT_eSPI.h>
#include "Free_Fonts.h"
#include "display_layout.h"

#define PIN_TFT_LIGHT 22

uint8_t const desc_hid_report[] = {TUD_HID_REPORT_DESC_GENERIC_INOUT(64)};
bool ledState = false;
bool willBlinkLed = true;
unsigned long lastLedToggle = 0;

TFT_eSPI tft = TFT_eSPI();
Adafruit_USBD_HID usb_hid;
SensorData dataPrevious{0, 0, 0, 0};
SensorData dataCurrent{0, 0, 0, 0};

void set_report_callback(uint8_t report_id, hid_report_type_t report_type, uint8_t const *buffer, uint16_t bufsize)
{
  if (bufsize < Constants::Report::Length)
  {
    drawMessage(tft, TFT_RED, "invalid buffer");
    return;
  }

  digitalWrite(PIN_LED, HIGH);

  uint8_t payloadType = buffer[Constants::Report::Index::Type];

  if (payloadType == Constants::Report::DataPayload)
  {
    dataCurrent.cpuTemp = buffer[Constants::Report::Index::CpuTemp];
    dataCurrent.cpuUtilization = buffer[Constants::Report::Index::CpuUtilization];
    dataCurrent.gpuTemp = buffer[Constants::Report::Index::GpuTemp];
    dataCurrent.gpuUtilization = buffer[Constants::Report::Index::GpuUtilization];

    if (!isSensorDataDifferent(dataPrevious, dataCurrent))
    {
      return;
    }

    drawValues(tft, dataCurrent);
    dataPrevious = dataCurrent;
  }
  else if (payloadType == Constants::Report::CommandPayload)
  {
    clearMessage(tft);

    uint8_t commandName = buffer[Constants::Report::Index::CommandName];
    uint8_t commandValue = buffer[Constants::Report::Index::CommandValue];

    if (commandName == Constants::Report::Command::MessageAwait)
    {
      drawMessage(tft, TFT_SKYBLUE, "awaiting...");
    }
    else if (commandName == Constants::Report::Command::MessageCheckHost)
    {
      drawMessage(tft, TFT_ORANGE, "check host");
    }
    else if (commandName == Constants::Report::Command::MessageHighTemp)
    {
      drawMessage(tft, TFT_RED, "high temp!");
    }
    else if (commandName == Constants::Report::Command::MessageLowRpm)
    {
      drawMessage(tft, TFT_RED, "low rpm!");
    }
    else if (commandName == Constants::Report::Command::ClearDisplay)
    {
      tft.fillScreen(TFT_BLACK);
    }
    else if (commandName == Constants::Report::Command::RestartDevice)
    {
      drawMessage(tft, TFT_YELLOW, "restarting...");
      delay(1000);
      watchdog_reboot(0, 0, 1);
    }
    else if (commandName == Constants::Report::Command::SetBrightnessOff)
    {
      analogWrite(PIN_TFT_LIGHT, 0);
    }
    else if (commandName == Constants::Report::Command::SetBrightnessLow)
    {
      analogWrite(PIN_TFT_LIGHT, 64);
    }
    else if (commandName == Constants::Report::Command::SetBrightnessMedium)
    {
      analogWrite(PIN_TFT_LIGHT, 128);
    }
    else if (commandName == Constants::Report::Command::SetBrightnessHigh)
    {
      analogWrite(PIN_TFT_LIGHT, 255);
    }
    else if (commandName == Constants::Report::Command::LedOff)
    {
      willBlinkLed = false;
      digitalWrite(PIN_LED, LOW);
    }
    else if (commandName == Constants::Report::Command::LedOn)
    {
      willBlinkLed = true;
      digitalWrite(PIN_LED, HIGH);
    }
    else if (commandName == Constants::Report::Command::EnterBootloader)
    {
      drawMessage(tft, TFT_YELLOW, "flash...");
      TinyUSBDevice.detach();
      delay(1000);
      // rp2040.reboot();
      reset_usb_boot(0, 0);
    }
  }
  digitalWrite(PIN_LED, LOW);
}

void setup()
{
  pinMode(PIN_LED, OUTPUT);
  pinMode(PIN_TFT_LIGHT, OUTPUT);
  digitalWrite(PIN_TFT_LIGHT, HIGH);

  SPI.begin();
  tft.init();
  tft.setRotation(4);
  tft.setFreeFont(FF18);
  drawStatic(tft);
  drawValues(tft, dataCurrent);
  drawMessage(tft, TFT_CYAN, "init...");

  TinyUSBDevice.setID(Constants::DeviceInfo::VendorId, Constants::DeviceInfo::ProductId);
  TinyUSBDevice.setManufacturerDescriptor(Constants::DeviceInfo::VendorName);
  TinyUSBDevice.setProductDescriptor(Constants::DeviceInfo::ProductName);

  usb_hid.setPollInterval(10);
  usb_hid.setReportDescriptor(desc_hid_report, sizeof(desc_hid_report));
  usb_hid.setStringDescriptor(Constants::DeviceInfo::ProductName);
  usb_hid.setReportCallback(NULL, set_report_callback);
  usb_hid.begin();

  if (!TinyUSBDevice.isInitialized())
  {
    TinyUSBDevice.begin(0);
  }

  if (TinyUSBDevice.mounted())
  {
    TinyUSBDevice.detach();
    delay(10);
    TinyUSBDevice.attach();
  }

  drawMessage(tft, TFT_SKYBLUE, "awaiting...");
  // watchdog_enable(8000, 1);
}

void loop()
{
  TinyUSBDevice.task();
  // watchdog_update();
  yield();
}