//*****************************************************************************
//
// OneController LED functions.
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
//+ 2015-11-10 [RT] - Enabled ocLED_Write and ocLED_Read functions
//

#ifndef __OC_LED_H
#define __OC_LED_H

/*****************************************************************************
 * LED Functions
 *
 * All API functions use a "C" calling convention.
 ****************************************************************************/
#ifdef  __cplusplus
extern "C" {
#endif

/**
* This function is used to enable or disable the LED interface.
* @author R. Turchik
* @param handle An open handle for communications 
* @param enable Set true to enable, or false to disable
* @date 7/22/2015
*/
acAPI ocLED_Enable(AC_HANDLE handle, bool enable);

/**
* This function is used to configure the LED interface
* @author R. Turchik
* @param handle An open handle for communications 
* @param LEDs A bit mask of for the LEDs to be configured.
* @param operation The operation to be performed on the selected LEDs
* @date 7/22/2015
*/
acAPI ocLED_Config(AC_HANDLE handle, uint8_t LEDs, uint32_t operation);

/**
* This function is used to write data, using the LED interface
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be written.
* @param bytes_to_write The number of bytes to write
* @param pData A pointer to the buffer containing the data to write 
* @date 7/22/2015
*/
acAPI ocLED_Write(AC_HANDLE handle, uint8_t unit, uint16_t bytes_to_write, uint8_t *pData);

/**
* This function is used to read data, using the LED interface
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be read.
* @param bytes_to_read The number of bytes to read
* @param pData A pointer to the buffer to receive the data read
* @date 7/22/2015
*/
acAPI ocLED_Read(AC_HANDLE handle, uint8_t unit, uint16_t bytes_to_read, uint8_t *pData);

#ifdef  __cplusplus
}
#endif

#endif // __OC_LED_H
