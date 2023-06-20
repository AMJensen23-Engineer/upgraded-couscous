//*****************************************************************************
//
// OneController I2C functions.
//
// Copyright (c) 2005-2015 Texas Instruments Incorporated.  All rights reserved.
// Software License Agreement
// 
//   Redistribution and use in source and binary forms, with or without
//   modification, are permitted provided that the following conditions
//   are met:
// 
//   Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.
// 
//   Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the  
//   distribution.
// 
//   Neither the name of Texas Instruments Incorporated nor the names of
//   its contributors may be used to endorse or promote products derived
//   from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
//
//*****************************************************************************
// Change History
//
//+ 2015-11-10 [RT] - Added prototype for ocSPI_GetNumberOfBytesRead
//

#ifndef __OC_SPI_H
#define __OC_SPI_H



#define VIA_SPI_UNITS           2
#define TIVA_SPI_UNITS          4

#define SSI_FRF_MOTO_MODE_0     0x00  // Moto fmt, polarity 0, phase 0
#define SSI_FRF_MOTO_MODE_1     0x02  // Moto fmt, polarity 0, phase 1
#define SSI_FRF_MOTO_MODE_2     0x01  // Moto fmt, polarity 1, phase 0
#define SSI_FRF_MOTO_MODE_3     0x03  // Moto fmt, polarity 1, phase 1
//#define SSI_FRF_TI              0x10  // TI frame format
//#define SSI_FRF_NMW             0x00000020  // National MicroWire frame format


extern uint8_t g_spi_num  ; //default number of OneController
//logical SPI map to Tiva SPI table
extern uint8_t g_spi_map[TIVA_SPI_UNITS];


//work for Windows and GCC 
#pragma pack(push,4) 
typedef struct _SPI_CFG
{
  uint32_t bitrate;//spi bitrate: unit 1k 
  uint8_t  protocol;// polarity, phase, spi type  
  uint8_t  datawidth;//data width from 4 to 16 bits
  uint8_t  cs_mode; //active high: 1 or active low
  uint8_t  cs_change; //between spi word. cs change 1: no: 0
  //? better in script because the parameters are related to chip instead of bus
 // uint8_t  active_mode; //how to get the data from spi. 1: master in active mode. 0: depend on data ready interrupt
 // uint16_t data_ready_int; //gpio pin as data ready interrupt
 // uint8_t  int_type; //interrupt type
 // uint8_t  start;    //1: need to set start pin 
 // uint8_t  start_pin; //gpio for start pin
  }SPI_CFG;
#pragma pack(pop)

#ifdef  __cplusplus
extern "C" {
#endif

/**
* This function is used to enable or disable an spi interface unit.
* @param handle An open handle for communications 
* @param unit The number of the unit to be enabled/disabled.
* @param enable Set true to enable, or false to disable, the I2C unit.
* @date 5/4/2015
*/
acAPI SPI_Enable(AC_HANDLE handle, uint8_t unit, bool enable);

/**
* This function is used to configure an spi interface unit.
* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param spi_cfg  spi configurations 
* @date 5/4/2015
*/
acAPI SPI_Config(AC_HANDLE handle, uint8_t unit, SPI_CFG *p_spi_cfg);


/**
* This function is used to write data to, then read data from, a slave device.

* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param cs_GPIO The chip select gpio number.
* @param bytes_to_write The number of bytes to be written to spi bus. and will receive the data from spi bus
* @param write_data_buffer A pointer to the buffer that contains the data to be written. the buffer will be as read buffer too.
* @date 5/4/2015
*/
acAPI SPI_WriteAndRead(AC_HANDLE handle, uint8_t unit, uint32_t cs_GPIO, 
    uint16_t bytes_to_write, uint8_t *p_write_data_buffer);

/**
* This function is used to capture sample.

* @param handle An open handle for communications 
* @param bytes_to_write The number of bytes to be written to spi bus. and will receive the data from spi bus
* @param p_write_data_buffer A pointer to the buffer that contains the data to be written for capturing a sample.
* @param bytes_to_read The number of bytes to be read
* @param p_read_data_buffer A pointer to the buffer that contains the data to be read from capturing a sample.
* @param sync read data mode, sync or async mode.
* @date 5/4/2015
*/
acAPI SPI_CaptureSample (AC_HANDLE handle,uint8_t unit, uint16_t bytes_to_write, uint8_t *p_write_data_buffer, uint32_t bytes_to_read, uint8_t *p_read_data_buffer, uint8_t sync) ;

/**
* This function is used to get the count of bytes received and ready to be read.

* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param numBytesRead A pointer to a uint16_t that will receive the byte count.
* @date 10/19/2015
*/
acAPI SPI_GetNumberOfBytesRead(AC_HANDLE handle, uint8_t unit, uint16_t *numBytesRead);


#ifdef  __cplusplus
}
#endif


#endif // __OC_SPI_H
