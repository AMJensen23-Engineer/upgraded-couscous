
//*****************************************************************************
//
//  OneController uart functions.
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
#include <memory.h>

#include "acctrl.h"
#include "acAPI_uart.h"

static bool bInitialized = false;
static bool bEnabled[MAX_SUPPORT_OC][VIA_UART_UNITS];

uint8_t g_uart_num  = VIA_UART_UNITS; //default number of OneController

uint8_t g_uart_map[TIVA_UART_UNITS] ={
	0, //log spi 0 = tiva spi 0
    1, //l1 = tiva spi 1
	0,
	6
 };

void UART_Initialize(OC_COMMAND *pCmd)
{
    if (!bInitialized)
    {
        memset(bEnabled, false, sizeof(bEnabled));
        bInitialized = true;
    }

    if (pCmd != NULL)
    {
        InitCommand(pCmd);
    }
}

acAPI UART_Enable(AC_HANDLE handle, uint8_t unit, bool enable)
{
    OC_COMMAND cmd;
    STATUS status = OC_ERR_OPERATION_FAILED;;

	HANDLE_CHECK(handle);
	if(unit >= VIA_UART_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;


	if(unit < g_uart_num)
		unit = g_uart_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;

    UART_Initialize(&cmd);
    
    if (enable && !bEnabled[handle][unit])
    {
        status = (0 == acInitIF(handle, IF_UART, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
	}
    else
    {
        status = bEnabled[handle][unit] ? STAT_OK : OC_ERR_NOT_ENABLED;
    }
    
	
    if (STAT_OK == status)
    {
        cmd.if_type  = UART_Interface;
        cmd.if_unit  = unit;
        cmd.command  = acCmd_UART_Enable;
        cmd.param[0] = unit;  
        cmd.param[1] = enable; 


        status = ocSendCommand(handle, &cmd);
        if (STAT_OK == status)
        {
         	bEnabled[handle][unit] = enable;
           status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 10255);
        }
    }

    // Disable only if the interface with the correct 
    // handle and unit is enabled.  (RT 467)
	if (!enable && bEnabled[handle][unit])
    {
        status = (0 == acUnInitIF(handle, IF_UART, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
        //should not care return status.
		bInitialized = false;
		bEnabled[handle][unit] = false;
	}
   
    return status;
}

acAPI UART_Config(AC_HANDLE handle, uint8_t unit, uint32_t BaudRate,uint8_t Parity,uint8_t  CharacterLength, uint8_t StopBits )
{
    OC_COMMAND cmd;
    STATUS status;

	
	
	HANDLE_CHECK(handle);
    UART_Initialize(&cmd);
    InitCommand(&cmd);
    cmd.if_type  = UART_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_UART_Config;
    cmd.param[0] = unit;
    cmd.param[1] = BaudRate;
    cmd.param[2] = Parity;
	cmd.param[3] = CharacterLength;
	cmd.param[4] = StopBits;

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 10255);
    }

    return status;
}

acAPI  UART_ConfigWatchDog(AC_HANDLE handle, uint8_t unit, uint16_t bytes_to_write, uint8_t *p_command_data_)
{
	OC_COMMAND cmd;
	STATUS status = STAT_OK;

	if (p_command_data_ == NULL)
		return OC_ERR_PARM_WRONG;

	HANDLE_CHECK(handle);
	UART_Initialize(&cmd);
	cmd.if_type = UART_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_UART_UART_ConfigWatchDog;
	cmd.param[0] = unit;

	cmd.param[1] = bytes_to_write;
	cmd.data_len = bytes_to_write;
	cmd.pdata = p_command_data_;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
	{
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);
	}

	return status;
}

acAPI UART_Write(AC_HANDLE handle, uint8_t unit, uint16_t bytes_to_write, uint8_t *p_write_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;

	if(p_write_data_buffer == NULL)
		return OC_ERR_PARM_WRONG;

	HANDLE_CHECK(handle);
    UART_Initialize(&cmd);
    cmd.if_type  = UART_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_UART_Write;
    cmd.param[0] = unit;
    
	cmd.param[1] = bytes_to_write;
    cmd.data_len = bytes_to_write;
    cmd.pdata    = p_write_data_buffer;

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
     {
        status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);
     }

    return status;
}

acAPI UART_Read(AC_HANDLE handle, uint8_t unit, uint16_t bytes_to_read, uint8_t *p_read_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;

	
	if(p_read_data_buffer == NULL)
		return OC_ERR_PARM_WRONG;

	HANDLE_CHECK(handle);
    UART_Initialize(&cmd);
    cmd.if_type  = UART_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_UART_Read;
    cmd.param[0] = unit;
    cmd.param[1] = bytes_to_read;
    

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        uint32_t count = acPacket_ReadSync(handle, MAKE_IF(UART_Interface, unit), p_read_data_buffer, bytes_to_read, 50000);
        status = count;
    }

    return status;
}

acAPI UART_Control(AC_HANDLE handle, uint8_t unit, uint32_t command, uint32_t arg)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);
    UART_Initialize(&cmd);
    InitCommand(&cmd);
    cmd.if_type  = UART_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_UART_Control;
	cmd.param[0] = unit;
	cmd.param[1] = command;
	cmd.param[2] = arg;	

    status = ocSendCommand(handle, &cmd);

	uint8_t bytesInUartBuff[1] = {0};

	if ((STAT_OK == status)  && (command < 3))  // only get data if we need to know how many bytes are in the Rx buffer
	{
		uint32_t count = acPacket_ReadSync(handle, MAKE_IF(UART_Interface, unit), bytesInUartBuff, 1, 50000);
		status = bytesInUartBuff[0];
	}

    return status;
}

acAPI UART_ConfigTimer(AC_HANDLE handle, uint8_t unit, uint32_t usecs)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit > TIVA_UART_TIMER)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	UART_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = UART_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_UART_ConfigTimer;
	cmd.param[0] = unit;
	cmd.param[1] = usecs;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);

	return status;
}

acAPI UART_StartTimer(AC_HANDLE handle, uint8_t unit, uint8_t device)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	UART_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = UART_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_UART_StartTimer;
	cmd.param[0] = unit;
	cmd.param[1] = device;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);

	return status;
}

acAPI UART_StopTimer(AC_HANDLE handle, uint8_t unit, uint8_t device)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	UART_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = UART_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_UART_StopTimer;
	cmd.param[0] = unit;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);

	return status;
}

acAPI UART_CloseTimer(AC_HANDLE handle, uint8_t unit)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	UART_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = UART_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_UART_CloseTimer;
	cmd.param[0] = unit;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);

	return status;
}

acAPI UART_SpareOne(AC_HANDLE handle, uint8_t unit, uint32_t arg1, uint32_t arg2, uint8_t *p_read_data_buffer)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	UART_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = UART_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_UART_SpareOne;
	cmd.param[0] = unit;
	cmd.param[1] = arg1;
	cmd.param[2] = arg2;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);

	return status;
}

acAPI UART_SpareTwo(AC_HANDLE handle, uint8_t unit, uint32_t arg1, uint32_t arg2, uint8_t *p_read_data_buffer)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	UART_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = UART_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_UART_SpareTwo;
	cmd.param[0] = unit;
	cmd.param[1] = arg1;
	cmd.param[2] = arg2;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);

	return status;
}

