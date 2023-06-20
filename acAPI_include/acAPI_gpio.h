//*****************************************************************************
//
// OneController GPIO functions.
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

#ifndef __OC_GPIO_H
#define __OC_GPIO_H


#define GPIO_PIN_COUNT  16
#define PIN_NAME_LENGTH  4 
//how many logical GPIO pin
extern uint8_t g_gpio_pin_num;
//logical GPIO pin map to Tiva GPIO pin table
extern char g_gpio_pin_name[GPIO_PIN_COUNT][PIN_NAME_LENGTH];



/*****************************************************************************
 * GPIO Functions
 *
 * All API functions use a "C" calling convention.
 ****************************************************************************/
#ifdef  __cplusplus
extern "C" {
#endif

/**
* This function is used to enable or disable the GPIO interface.
* @author R. Turchik
* @param handle An open handle for communications 
* @param pin_mask A bit mask to specify which GPIO pins to operate on
* @param enable Set true to enable, or false to disable, the pins
* @date 4/22/2015
*/
acAPI GPIO_Enable(AC_HANDLE handle, uint32_t pin_mask, bool enable);

/**
* This function is used to set the mode of the GPIO pins
* @author R. Turchik
* @param handle An open handle for communications 
* @param pin_mask A bit mask to specify which GPIO pins to operate on
* @param mode The mode to which the pins will be set
* @date 4/22/2015
*/
acAPI GPIO_Config(AC_HANDLE handle, uint32_t pin_mask, uint8_t mode);

/**
* This function is used to write the state of the GPIO pins
* @author R. Turchik
* @param handle An open handle for communications 
* @param pin_mask A bit mask to specify which GPIO pins to operate on
* @param mode A bit mask to specify the state to which the pins will be set
* @date 4/22/2015
*/
acAPI GPIO_Write(AC_HANDLE handle, uint32_t pin_mask, uint32_t pin_values);

/**
* This function is used to read the state of the GPIO pins
* @author R. Turchik
* @param handle An open handle for communications 
* @param pin_mask A bit mask to specify which GPIO pins to operate on
* @param pData A pointer to a 32-bit variable to receive the data read
* @date 4/22/2015
*/
acAPI GPIO_Read(AC_HANDLE handle, uint32_t pin_mask, uint32_t *pData);

/**
* This function is used to gpio interrupt
* @author Bing
* @param handle An open handle for communications 
* @param pin_mask A bit mask to specify which GPIO pins to operate on
* @param IntType  three gpios interrupt type
* @param EnableInt enable/disable/notchange gpio interrupt
* @param int_callback interrupt callback
* @param pdata  the buffer to hold callback data
* @param data_size callback data buffer size
* @date 4/22/2015
*/

acAPI GPIO_RegisterInt(AC_HANDLE handle, uint32_t pin_mask,uint8_t IntType[3],uint8_t EnableInt[3],  void (*int_callback)(uint8_t *pcalbackdata, uint16_t callbac_data_size),uint8_t* pdata, uint32_t data_size);
#ifdef  __cplusplus
}
#endif

#endif // __OC_GPIO_H
