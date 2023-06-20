
//*****************************************************************************
//
//  OneController i2c functions.
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
//+ 2016-01-12 [RT] - added DEFAULT_WRITE_TIMEOUT and DEFAULT_READ_TIMEOUT
//
//*****************************************************************************
#include <memory.h>

#include "acctrl.h"
#include "acAPI_i2c.h"

#define DEFAULT_TIMEOUT         255
#define DEFAULT_WRITE_TIMEOUT   (DEFAULT_TIMEOUT + bytes_to_write)
#define DEFAULT_READ_TIMEOUT    (DEFAULT_TIMEOUT + bytes_to_read)

static bool bInitialized = false;
static bool bEnabled[MAX_SUPPORT_OC][TIVA_I2C_UNITS];
static uint16_t nNumberOfBytesRead[MAX_SUPPORT_OC][TIVA_I2C_UNITS];



uint8_t g_i2c_num  = VIA_I2C_UNITS; //default number of OneController

#if 0
uint8_t g_i2c_map[TIVA_I2C_UNITS] ={
	5, //log I2c 0 = tiva i2c 5
    9, //l1 = t8
	7, //l2 = t7
	3, //l3 =t3
	8, //l4 = t8
	4  //l5 =t4
 };


uint8_t g_i2c_map[TIVA_I2C_UNITS] ={
	0, //log I2c 0 = msp432e 0  ,//PB2,PB3
    //1, //l1 =  = msp432e 1, PG0,PG1
	2, //l2 = = msp432e 2, PL1,PL0
	3, //l3 =msp432e 3, Pk4,PK5
	4, //l4 = msp432e 4,Pk6,Pk7
	5,  //l5 = msp432e 5,PB4,PB5
//	6,  //l5 = msp432e 6,PA6,PA7
	7,  //l5 = msp432e 7,PA4,PA5
	//8,  //l5 = msp432e 8,PA2,PA3
	//9,  //l5 = msp432e 9,PA0,PA1
 };
#endif

uint8_t g_i2c_map[TIVA_I2C_UNITS] ={
	0, //log I2c 0 = msp432e 0  ,//PB2,PB3
    //1, //l1 =  = msp432e 1, PG0,PG1
	2, //l1 = = msp432e 2, PL1,PL0
	3, //l2 =msp432e 3, Pk4,PK5
	4, //l3 = msp432e 4,Pk6,Pk7
	5,  //l4 = msp432e 5,PB4,PB5
//	6,  //lx = msp432e 6,PA6,PA7
	7,  //l5 = msp432e 7,PA4,PA5
	//8,  //l5 = msp432e 8,PA2,PA3
	//9,  //l5 = msp432e 9,PA0,PA1
 };
void I2C_Initialize(OC_COMMAND *pCmd)
{
    if (!bInitialized)
    {
        memset(bEnabled, false, sizeof(bEnabled));
        memset(nNumberOfBytesRead, 0, sizeof(nNumberOfBytesRead));
        bInitialized = true;
    }

    if (pCmd != NULL)
    {
        InitCommand(pCmd);
    }
}

acAPI I2C_Enable(AC_HANDLE handle, uint8_t unit, bool enable)
{
    OC_COMMAND cmd;
    STATUS status = OC_ERR_OPERATION_FAILED;

	HANDLE_CHECK(handle);

	//get Tiva I2C number
	
	if(unit < g_i2c_num)
		unit = g_i2c_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    I2C_Initialize(&cmd);

	if (enable && !bEnabled[handle][unit])
    {
        status = (0 == acInitIF(handle, IF_I2C, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
    }
    else
    {
        status = bEnabled[handle][unit] ? STAT_OK : OC_ERR_NOT_ENABLED;
    }
    

    if (STAT_OK == status)
    {
        cmd.if_type  = I2C_Interface;
        cmd.if_unit  = unit;
        cmd.command  = acCmd_I2C_Enable;
        cmd.param[0] = unit;  
        cmd.param[1] = enable; 


        status = ocSendCommand(handle, &cmd);
        if (STAT_OK == status)
        {
            bEnabled[handle][unit] = enable;
            nNumberOfBytesRead[handle][unit] = 0;

            status = acWaitForStatus(handle, MAKE_IF(I2C_Interface, unit), DEFAULT_TIMEOUT);
        }
    }


	  // Disable only if the interface with the correct 
    // handle and unit is enabled.  (RT 467)
	if (!enable && bEnabled[handle][unit])
    {
        status = (0 == acUnInitIF(handle, IF_I2C, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
        //should not care return status.
		bInitialized = false;
		bEnabled[handle][unit] = false;
	}
  
    return status;
}

acAPI I2C_Config(AC_HANDLE handle, uint8_t unit, uint32_t bit_rate, bool pullups)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

	//get Tiva I2C number
	
	if(unit < g_i2c_num)
		unit = g_i2c_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    I2C_Initialize(&cmd);
    InitCommand(&cmd);			// NECESSARY????
    cmd.if_type  = I2C_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_I2C_Config;
    cmd.param[0] = unit;
    cmd.param[1] = bit_rate;
    cmd.param[2] = pullups;

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(I2C_Interface, unit), DEFAULT_TIMEOUT);
    }

    return status;
}


acAPI I2C_Write(AC_HANDLE handle, uint8_t unit, uint8_t device_address, uint16_t bytes_to_write, uint8_t *p_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

	//get Tiva I2C number
	
	if(unit < g_i2c_num)
		unit = g_i2c_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    I2C_Initialize(&cmd);
    cmd.if_type  = I2C_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_I2C_Write;
    cmd.param[0] = unit;
    cmd.param[1] = device_address;
    cmd.data_len = bytes_to_write;
    cmd.pdata    = p_data_buffer;

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(I2C_Interface, unit), DEFAULT_WRITE_TIMEOUT);
    }
    return status;
}

acAPI I2C_Read(AC_HANDLE handle, uint8_t unit, uint8_t device_address, uint16_t bytes_to_read, uint8_t *p_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

	//get Tiva I2C number
	
	if(unit < g_i2c_num)
		unit = g_i2c_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    I2C_Initialize(&cmd);
    cmd.if_type  = I2C_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_I2C_Read;
    cmd.param[0] = unit;
    cmd.param[1] = device_address;
    cmd.param[2] = bytes_to_read;

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        uint32_t count = acPacket_ReadSync(handle, MAKE_IF(I2C_Interface, unit), p_data_buffer, bytes_to_read, DEFAULT_READ_TIMEOUT);

        nNumberOfBytesRead[handle][unit] = count;
        if (count < bytes_to_read)
        {
            status = OC_ERR_NOT_ENOUGH_DATA;
        }
        else
        {
            status = count;
        }
    }

    return status;
}

acAPI I2C_WriteRegister(AC_HANDLE handle, uint8_t unit, uint8_t device_address, uint32_t register_address, 
    uint16_t flags, uint16_t bytes_to_write, uint8_t *p_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;

	HANDLE_CHECK(handle);

	//get Tiva I2C number
	
	if(unit < g_i2c_num)
		unit = g_i2c_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    I2C_Initialize(&cmd);
    cmd.if_type  = I2C_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_I2C_WriteRegister;
    cmd.param[0] = unit;
    cmd.param[1] = device_address;
    cmd.param[2] = register_address;
    cmd.param[3] = flags;
    cmd.data_len = bytes_to_write;
    cmd.pdata    = p_data_buffer;

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(I2C_Interface, unit), DEFAULT_WRITE_TIMEOUT);
    }

    return status;
}

acAPI I2C_ReadRegister(AC_HANDLE handle, uint8_t unit, uint8_t device_address, uint32_t register_address, uint16_t flags, uint16_t bytes_to_read, uint8_t *p_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;

	HANDLE_CHECK(handle);

	//get Tiva I2C number
		if(unit < g_i2c_num)
		unit = g_i2c_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    I2C_Initialize(&cmd);
    cmd.if_type  = I2C_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_I2C_ReadRegister;
    cmd.param[0] = unit;
    cmd.param[1] = device_address;
    cmd.param[2] = register_address;
    cmd.param[3] = flags;
    cmd.param[4] = bytes_to_read;
    cmd.pdata    = p_data_buffer;

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        uint32_t count = acPacket_ReadSync(handle, MAKE_IF(I2C_Interface, unit), p_data_buffer, bytes_to_read, DEFAULT_READ_TIMEOUT);

        nNumberOfBytesRead[handle][unit] = count;
        if (count < bytes_to_read)
        {
            status = OC_ERR_NOT_ENOUGH_DATA;
        }
        else
        {
            status = count;
        }
    }

    return status;
}

acAPI I2C_BlockWriteBlockRead(AC_HANDLE handle, uint8_t unit, uint8_t device_address, uint16_t bytes_to_write, 
    uint8_t *p_write_data_buffer, uint16_t bytes_to_read, uint8_t *p_read_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;

	HANDLE_CHECK(handle);
	
	//get Tiva I2C number
	if(unit < g_i2c_num)
		unit = g_i2c_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    I2C_Initialize(&cmd);
    cmd.if_type  = I2C_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_I2C_BlkWriteBlkRead;
    cmd.param[0] = unit;
    cmd.param[1] = device_address;
    cmd.param[2] = bytes_to_read;
    cmd.data_len = bytes_to_write;
    cmd.pdata    = p_write_data_buffer;

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        uint32_t count = acPacket_ReadSync(handle, MAKE_IF(I2C_Interface, unit), 
            p_read_data_buffer, bytes_to_read, DEFAULT_WRITE_TIMEOUT + DEFAULT_READ_TIMEOUT);

        nNumberOfBytesRead[handle][unit] = count;
        if (count < bytes_to_read)
        {
            status = OC_ERR_NOT_ENOUGH_DATA;
        }
        else
        {
            status = count;
        }
    }

    return status;
}

acAPI I2C_GetNumberOfBytesRead(AC_HANDLE handle, uint8_t unit, uint16_t *numBytesRead)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;

	HANDLE_CHECK(handle);

	if(unit < g_i2c_num)
		unit = g_i2c_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    I2C_Initialize(&cmd);

    if (handle >= MAX_SUPPORT_OC && unit >= TIVA_I2C_UNITS)
    {
        status = CompositeStatus(OC_ERR_I2C, OC_ERR_PARAM_OUT_OF_RANGE);
    }
    else if (!bEnabled[handle][unit])
    {
        status = CompositeStatus(OC_ERR_I2C, OC_ERR_NOT_ENABLED);
    }
    else
    {
        *numBytesRead = nNumberOfBytesRead[handle][unit];
    }

    return status;
}