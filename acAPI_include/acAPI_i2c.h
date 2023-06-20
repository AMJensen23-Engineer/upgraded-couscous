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

#ifndef __OC_I2C_H
#define __OC_I2C_H


//#define VIA_I2C_UNITS           6
#define TIVA_I2C_UNITS         10
extern uint8_t g_i2c_num; //default number of OneController
extern uint8_t g_i2c_map[TIVA_I2C_UNITS];


/*****************************************************************************
 * I2C Functions
 *
 * All API functions use a "C" calling convention.
 ****************************************************************************/
#ifdef  __cplusplus
extern "C" {
#endif

/**
* This function is used to enable or disable an I2C interface unit.
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be enabled/disabled.
* @param enable Set true to enable, or false to disable, the I2C unit.
* @date 5/4/2015
*/
acAPI I2C_Enable(AC_HANDLE handle, uint8_t unit, bool enable);

/**
* This function is used to configure an I2C interface unit.
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param bit_rate A constant indicating the speed at which to set the I2C unit.
* @param pullups Set true to enable, or false to disable, I2C pullups for the unit.
* @date 5/4/2015
*/
acAPI I2C_Config(AC_HANDLE handle, uint8_t unit, uint32_t bit_rate, bool pullups);

/**
* This function is used to read data from a slave device.
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be read.
* @param device_addr The 7-bit address of the target slave device.
* @param num_bytes The number of bytes to be read from the slave device.
* @param pdata A pointer to the buffer that is to receive the data read.
* @date 5/4/2015
*/
acAPI I2C_Read(AC_HANDLE handle, uint8_t unit, uint8_t device_addr, uint16_t num_bytes, uint8_t *pdata);

/**
* This function is used to read register data from a slave device.
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be read.
* @param device_addr The 7-bit address of the target slave device.
* @param register_addr The address of the register to be read.
* @param flags A flag to indicate whether the register address is 8-bits or 16-bits.
* @param num_bytes The number of register bytes to read from the slave device.
* @param pdata A pointer to the buffer that is to receive the register data read.
* @date 5/4/2015
*/
acAPI I2C_ReadRegister(AC_HANDLE handle, uint8_t unit, uint8_t device_addr, 
    uint32_t register_addr, uint16_t flags, uint16_t num_bytes, uint8_t *data_buffer);

/**
* This function is used to write data to a slave device.
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be written.
* @param device_addr The 7-bit address of the target slave device.
* @param num_bytes The number of bytes to be written to the slave device.
* @param pdata A pointer to the buffer that contains the data to be written.
* @date 5/4/2015
*/
acAPI I2C_Write(AC_HANDLE handle, uint8_t unit, uint8_t device_addr, uint16_t num_bytes, uint8_t *pdata);

/**
* This function is used to write register data to a slave device.
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be written.
* @param device_addr The 7-bit address of the target slave device.
* @param register_addr The address of the register to be written.
* @param flags A flag to indicate whether the register address is 8-bits or 16-bits.
* @param num_bytes The number of register bytes to be written to the slave device.
* @param pdata A pointer to the buffer that contains the register data to be written.
* @date 5/4/2015
*/
acAPI I2C_WriteRegister(AC_HANDLE handle, uint8_t unit, uint8_t device_addr, 
    uint32_t register_addr, uint16_t flags, uint16_t num_bytes, uint8_t *data_buffer);

/**
* This function is used to write data to, then read data from, a slave device.
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be written and read.
* @param device_addr The 7-bit address of the target slave device.
* @param write_bytes The number of bytes to be written to the slave device.
* @param write_data_buffer A pointer to the buffer that contains the data to be written.
* @param read_bytes The number of bytes to be read from the slave device.
* @param read_data_buffer A pointer to the buffer that is to receive the data read.
* @date 5/4/2015
*/
acAPI I2C_BlockWriteBlockRead(AC_HANDLE handle, uint8_t unit, uint8_t device_addr, 
    uint16_t write_bytes, uint8_t *write_data_buffer, uint16_t read_bytes, uint8_t *read_data_buffer);

/**
* This function is used to get the number of bytes read by the last called I2C read function.
* @author R. Turchik
* @param handle An open handle for communications 
* @param unit The number of the unit to be used.
* @param numBytesRead The number of bytes read by the las I2C read function.
* @date 5/4/2015
*/
acAPI I2C_GetNumberOfBytesRead(AC_HANDLE handle, uint8_t unit, uint16_t *numBytesRead);

#ifdef  __cplusplus
}
#endif

#endif // __OC_I2C_H
