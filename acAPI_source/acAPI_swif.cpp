//*****************************************************************************
//
//  OneController swif functions.
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
//+ 2015-11-10 [RT] - Added ocSPI_GetNumberOfBytesRead
//
#include <stdlib.h>
#include <memory.h>

#include "acctrl.h"
#include "acAPI_swif.h"


static bool bInitialized = false;
static bool bEnabled[MAX_SUPPORT_OC][VIA_SWIF_UNITS];
static uint16_t nNumberOfBytesRead[MAX_SUPPORT_OC][VIA_SWIF_UNITS];

#define DEFAULT_TIMEOUT         255
#define DEFAULT_WRITE_TIMEOUT   (DEFAULT_TIMEOUT + bytes_to_write)
#define DEFAULT_READ_TIMEOUT    (DEFAULT_TIMEOUT + bytes_to_read)
#define MAX_REG_BITS   16

void SWIF_Initialize(OC_COMMAND *pCmd)
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

acAPI SWIF_Enable(AC_HANDLE handle, uint8_t unit, bool enable)
{
    OC_COMMAND cmd;
    STATUS status = OC_ERR_OPERATION_FAILED;;

	if(unit >= VIA_SWIF_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	HANDLE_CHECK(handle);

    SWIF_Initialize(&cmd);
    
    if (enable && !bEnabled[handle][unit])
    {
        status = (0 == acInitIF(handle, IF_SWIF, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
    }
    else
    {
        status = bEnabled[handle][unit] ? STAT_OK : OC_ERR_NOT_ENABLED;
    }
    
	
    if (STAT_OK == status)
    {
        cmd.if_type  = SWIF_Interface;
        cmd.if_unit  = unit;
        cmd.command  = acCmd_SWIF_Enable;
        cmd.param[0] = unit;  
        cmd.param[1] = enable; 


        status = ocSendCommand(handle, &cmd);
        if (STAT_OK == status)
        {
            nNumberOfBytesRead[handle][unit] = 0;
            status = acWaitForStatus(handle, MAKE_IF(SWIF_Interface, unit),  DEFAULT_TIMEOUT  );

            /* RT 01-07-16: change the status flag only if successful */
			if (IsSuccess(status))
            {
			    bEnabled[handle][unit] = enable;
            }
        }
    }

    // Disable only if the interface with the correct 
    // handle and unit is enabled.  (RT 467)
	if (!enable && bEnabled[handle][unit])
    {
        status = (0 == acUnInitIF(handle, IF_SWIF, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
        //should not care return status.
		bInitialized = false;
		bEnabled[handle][unit] = false;
	}
   
    return status;
}

acAPI SWIF_Config(AC_HANDLE handle, uint8_t unit, SWIF_CFG *p_swif_cfg )
{
    OC_COMMAND cmd;
    STATUS status;
	HANDLE_CHECK(handle);
	if(unit >= VIA_SWIF_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

    SWIF_Initialize(&cmd);
    InitCommand(&cmd);
    cmd.if_type  =  SWIF_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_SWIF_Config;
    cmd.param[0] = unit;
    cmd.param[1] = p_swif_cfg->gpio;
    cmd.param[2] = p_swif_cfg->gpio_input_mode;
	cmd.param[3] = p_swif_cfg->gpio_output_mode;
	cmd.param[4] = p_swif_cfg->swif_mode;
	cmd.param[5] = p_swif_cfg->state_short_period;
	cmd.param[6] = p_swif_cfg->state_long_period;
	cmd.param[7] = p_swif_cfg->read_wait_start_time;
	

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(SWIF_Interface, unit), 10255);
    }

    return status;
}




acAPI SWIF_WriteReg(AC_HANDLE handle, uint8_t unit, uint16_t reg_addr, uint8_t reg_addr_bits, 
    uint8_t bytes_to_write,uint8_t *p_write_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;
	uint32_t time_out = 0;
	if(unit >= VIA_SWIF_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;


	HANDLE_CHECK(handle);
    if(p_write_data_buffer == NULL || reg_addr_bits > MAX_REG_BITS || !reg_addr_bits)
	  return OC_ERR_SWIF_PARAM_ERROR ;

	SWIF_Initialize(&cmd);

    cmd.if_type  = SWIF_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_SWIF_Write;
    cmd.param[0] = unit;
    cmd.param[1] = reg_addr;
    cmd.param[2] = reg_addr_bits;
    cmd.param[3] = bytes_to_write;
    cmd.data_len = bytes_to_write;
    cmd.pdata    = p_write_data_buffer;

    status = ocSendCommand(handle, &cmd);

	if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(SWIF_Interface, unit), DEFAULT_WRITE_TIMEOUT);
    }

	
    return status;
}



acAPI SWIF_ReadReg(AC_HANDLE handle, uint8_t unit, uint16_t reg_addr, uint8_t reg_addr_bits, 
    uint8_t bytes_to_read, uint8_t *p_read_data_buffer )
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;
	uint32_t time_out = 0;
	if(unit >= VIA_SWIF_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;


	HANDLE_CHECK(handle);
    
	if(p_read_data_buffer == NULL || reg_addr_bits > MAX_REG_BITS || !reg_addr_bits)
	 return OC_ERR_SWIF_PARAM_ERROR ;

	SWIF_Initialize(&cmd);

    cmd.if_type  = SWIF_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_SWIF_Read;
    cmd.param[0] = unit;
    cmd.param[1] = reg_addr;
    cmd.param[2] = reg_addr_bits;
    cmd.param[3] = bytes_to_read;
    
		
      status = ocSendCommand(handle, &cmd);
	
	if (STAT_OK == status  )
    {
        uint32_t count = acPacket_ReadSync(handle, MAKE_IF(SWIF_Interface, unit), 
            p_read_data_buffer, bytes_to_read, DEFAULT_READ_TIMEOUT);

        nNumberOfBytesRead[handle][unit] = count;
        if (count < bytes_to_read)
        {
            int32_t error_status;
			acPacket_GetIFStatus(handle, MAKE_IF(SWIF_Interface, unit), &error_status);
		    status = error_status;
        }
    }

    return status;
}
