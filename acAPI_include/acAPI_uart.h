//*****************************************************************************
//
// OneController uart functions.
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

#ifndef __OC_UART_H
#define __OC_UART_H


#define VIA_UART_UNITS           2
#define TIVA_UART_UNITS          8
#define TIVA_UART_TIMER          2

//UART data bits
#define UART_CONFIG_WLEN_8      0x03  // 8 bit data
#define UART_CONFIG_WLEN_7      0x02  // 7 bit data
#define UART_CONFIG_WLEN_6      0x01  // 6 bit data
#define UART_CONFIG_WLEN_5      0x00  // 5 bit data

//stop bits
#define UART_CONFIG_STOP_ONE    0x00  // One stop bit
#define UART_CONFIG_STOP_TWO    0x01  // Two stop bits

//parity
#define UART_CONFIG_PAR_NONE    0x00  // No parity
#define UART_CONFIG_PAR_EVEN    0x01  // Even parity:which checks for an even number of 1s. 
#define UART_CONFIG_PAR_ODD     0x02  // Odd parity: which checks for an odd number of 1s.

extern uint8_t g_uart_num   ; //default number of OneController
//logical UART map to Tiva UART table
extern uint8_t g_uart_map[TIVA_UART_UNITS];


#ifdef  __cplusplus
extern "C" {
#endif

/**
* This function is used to enable or disable an spi interface unit.
* @param handle An open handle for communications 
* @param unit The number of the unit to be enabled/disabled.
* @param enable Set true to enable, or false to disable, the I2C unit.
* @date 10/4/2015
*/
acAPI UART_Enable(AC_HANDLE handle, uint8_t unit, bool enable);

/**
* This function is used to configure an spi interface unit.
* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param BaudRate : uart baud rate 
* @param Parity : uart Parity 
* @param CharacterLength : data bits 
* @param StopBits : stop bits 
* @date 10/4/2015
*/
acAPI UART_Config(AC_HANDLE handle, uint8_t unit, 
	    uint32_t BaudRate,
        uint8_t Parity,
        uint8_t  CharacterLength,
        uint8_t StopBits
		);


/**
* This function is used to write data to uart .

* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param write_bytes The number of bytes to be written to spi bus. and will receive the data from uart bus
* @param write_data_buffer A pointer to the buffer that contains the data to be written. .
* @date 10/4/2015
*/
acAPI UART_Write(AC_HANDLE handle, uint8_t unit,  uint16_t write_bytes, uint8_t *write_data_buffer);

/**
* This function is used to read data from uart.

* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param read_bytes The number of bytes to be read from uart
* @param read_data_buffer A pointer to the buffer to store read data.
  return: read bytes  
* @date 10/4/2015
*/
acAPI UART_Read(AC_HANDLE handle, uint8_t unit,  uint16_t read_bytes, uint8_t *read_data_buffer);


/**
* This function is used to disable automatically read data from uart.
  no support anymore
* @param handle An open handle for communications 
* @param unit The number of the unit to be disabled.
* @date 5/4/2015
*/
acAPI UART_DisableReceiver(AC_HANDLE handle,uint8_t unit);

/**
added by LEE
*/
acAPI UART_Control(AC_HANDLE handle, uint8_t unit, uint32_t command, uint32_t arg);
acAPI UART_ConfigTimer(AC_HANDLE handle, uint8_t unit, uint32_t usecs);
acAPI UART_StartTimer(AC_HANDLE handle, uint8_t unit, uint8_t device);
acAPI UART_StopTimer(AC_HANDLE handle, uint8_t unit, uint8_t device);
acAPI UART_CloseTimer(AC_HANDLE handle, uint8_t unit);
acAPI UART_SpareOne(AC_HANDLE handle, uint8_t unit, uint32_t arg1, uint32_t arg2, uint8_t *p_read_data_buffer);
acAPI UART_SpareTwo(AC_HANDLE handle, uint8_t unit, uint32_t arg1, uint32_t arg2, uint8_t *p_read_data_buffer);
acAPI UART_ConfigWatchDog(AC_HANDLE handle, uint8_t unit, uint16_t bytes_to_write, uint8_t *p_command_data_);

#ifdef  __cplusplus
}
#endif


#endif // __OC_UART_H
