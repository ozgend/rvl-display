# rvl-display

a custom hardware to monitor system sensors via 1.8" TFT LCD, powered by a Raspberry Pi Pico and communicating with a host application via USB HID.

## Hardware

- **Controller**: Raspberry Pi Pico (RP2040).
- **Display**: 1.8" TFT LCD (SPI).
- **Connection**: Direct mainboard USB 9 Pin 2.0 header
- **VID:PID**: `0x5EED:0xFACE` (denolk rvl-display)

## Project Structure

- [rvl.device.fw/](rvl.device.fw/): PlatformIO project for the Raspberry Pi Pico firmware.
  - Uses [rvl.device.fw/src/main.cpp](rvl.device.fw/src/main.cpp) for HID communication and display rendering.
  - Configuration is defined in [rvl.device.fw/platformio.ini](rvl.device.fw/platformio.ini).
- [rvl.host.app/](rvl.host.app/): c# dotnet project for the HID host application
  - Utilizes `LibreHardwareMonitor` to fetch system metrics.
  - Streams data via HID to the device.

## Getting Started

### Firmware

1. Open the [rvl.device.fw/](rvl.device.fw/) folder via VS Code + PlatformIO.
2. Build and upload the project to your Raspberry Pi Pico.
3. UI layout: https://lopaka.app/gallery/28702/59717

   ![tft layout](./docs/tft_layout.png)

### Host Application

1. Open the [rvl.host.app/](rvl.host.app/) project
2. Build and run the application (requires Administrator privileges for `LibreHardwareMonitor` to access sensors).
3. Simple cli interface commands & streaming data to the device:

   ```ini
   Rvl.Host.App
   | exit=       [CTRL+C]
   | bootloader= [F]
   | ledON=      [O]
   | ledOFF=     [P]
   | brightness= [B]right|[N]ormal|[M]in
   | clear=      [C]
   | restart=    [R]
   | messages=   [CTRL+1..4]
   ```

### TODO

- [ ] Add fan sensors
- [ ] Tidy up the HID host + win native service
- [ ] 3D print a custom case for the display and components
- [ ] display temp colors based on thresholds
