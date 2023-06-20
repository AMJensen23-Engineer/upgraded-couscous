
//*****************************************************************************
//
//  OneController spi functions.
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
#include <string.h>

#include "acctrl.h"
#include "acAPI_gpio.h"
#include "acAPI_spi.h"

static bool bInitialized = false;
static bool bEnabled[MAX_SUPPORT_OC][TIVA_SPI_UNITS];
static uint16_t nNumberOfBytesRead[MAX_SUPPORT_OC][TIVA_SPI_UNITS];

uint8_t g_spi_num  = VIA_SPI_UNITS; //default number of OneController

uint8_t g_spi_map[TIVA_SPI_UNITS] ={
	2, //log spi 0 = tiva spi 0
    3, //l1 = tiva spi 1
 };


void SPI_Initialize(OC_COMMAND *pCmd)
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

acAPI SPI_Enable(AC_HANDLE handle, uint8_t unit, bool enable)
{
    OC_COMMAND cmd;
    STATUS status = OC_ERR_OPERATION_FAILED;

	HANDLE_CHECK(handle);

	if(unit < g_spi_num)
		unit = g_spi_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;
   
	SPI_Initialize(&cmd);
    
    if (enable && !bEnabled[handle][unit])
    {
        status = (0 == acInitIF(handle, IF_SPI, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
    }
    else
    {
        status = bEnabled[handle][unit] ? STAT_OK : OC_ERR_NOT_ENABLED;
    }
    
	
    if (STAT_OK == status)
    {
        cmd.if_type  = SPI_Interface;
        cmd.if_unit  = unit;
        cmd.command  = acCmd_SPI_Enable;
        cmd.param[0] = unit;  
        cmd.param[1] = enable; 


        status = ocSendCommand(handle, &cmd);
        if (STAT_OK == status)
        {
            nNumberOfBytesRead[handle][unit] = 0;
            status = acWaitForStatus(handle, MAKE_IF(SPI_Interface, unit), 10255);

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
        status = (0 == acUnInitIF(handle, IF_SPI, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
        //should not care return status.
		bInitialized = false;
		bEnabled[handle][unit] = false;
	}
   
    return status;
}

acAPI SPI_Config(AC_HANDLE handle, uint8_t unit, SPI_CFG *p_spi_cfg )
{
    OC_COMMAND cmd;
    STATUS status;
	HANDLE_CHECK(handle);
	
	
	if(unit < g_spi_num)
		unit = g_spi_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;
   
    SPI_Initialize(&cmd);
    InitCommand(&cmd);
    cmd.if_type  = SPI_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_SPI_Config;
    cmd.param[0] = unit;
    cmd.param[1] = p_spi_cfg->bitrate;
    cmd.param[2] = p_spi_cfg->protocol;
	cmd.param[3] = p_spi_cfg->datawidth;
	cmd.param[4] = p_spi_cfg->cs_mode;
	cmd.param[5] = p_spi_cfg->cs_change;
	//	cmd.param[6] = spi_cfg.active_mode;
    //	cmd.param[7] = (spi_cfg.int_type << 16) | spi_cfg.data_ready_int ;


    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(SPI_Interface, unit), 10255);
    }

    return status;
}




acAPI SPI_WriteAndRead(AC_HANDLE handle, uint8_t unit, uint32_t cs_GPIO, uint16_t bytes_to_write, uint8_t *p_write_data_buffer)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;
	uint32_t time_out = 0;
	
	if(unit < g_spi_num)
		unit = g_spi_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;
   

	HANDLE_CHECK(handle);
    //check the gpio number out of range
	if(cs_GPIO & ~((1 << g_gpio_pin_num) -1))
		return OC_ERR_PARM_WRONG;

	if(p_write_data_buffer == NULL )
	 return OC_ERR_SPI_PARAM_ERROR ;

	SPI_Initialize(&cmd);

    cmd.if_type  = SPI_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_SPI_WriteAndRead;
    cmd.param[0] = unit;
     uint32_t bit =1;
	  for ( int gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin)
       {
         if (cs_GPIO & (bit << gpio_pin))     // enable/disable this pin?
         {
           strcpy((char*)&cmd.param[1],(char*)g_gpio_pin_name[gpio_pin]);
           break; 
		 }
       }

	//char *p = (char*)&cmd.param[1];
	  //   p[0]= 'P';
	//	 p[1]= 'A';

	cmd.param[2] = bytes_to_write;
    cmd.data_len = bytes_to_write;
    cmd.pdata    = p_write_data_buffer;

    if(bytes_to_write <= MAX_PAYLOAD)
	{ 
      status = ocSendCommand(handle, &cmd);
	} else
#if 0
	else
	  {
		  status = ocSendCommand_BigData(handle, &cmd);
	
    if (STAT_OK == status)
    {
           do{ 
               status = ocWaitForStatus(handle, MAKE_IF(SPI_Interface, unit), 255);
		       time_out +=255; 
		     }while(status == OC_ERR_SYS_COM_IN_PROCESSING && time_out < bytes_to_write);
		  
		     if(status == OC_ERR_SYS_COM_DONE)
				 status = STAT_OK;
		  } 
	
	}
#else
	{
	    DbgWriteText("bytes_to_write =%d too big \r\n", bytes_to_write);
		
	return OC_ERR_SPI_PARAM_ERROR ;
	} 
#endif	

	if (STAT_OK == status  )
    {
        uint32_t count = acPacket_ReadSync(handle, MAKE_IF(SPI_Interface, unit), 
            p_write_data_buffer, bytes_to_write, (uint16_t)(255+bytes_to_write));

        nNumberOfBytesRead[handle][unit] = count;
        if (count < bytes_to_write)
        {
            int32_t error_status;
			acPacket_GetIFStatus(handle, MAKE_IF(SPI_Interface, unit), &error_status);
		    status = error_status;
        }
    }

    return status;
}

acAPI SPI_CaptureSample (AC_HANDLE handle,uint8_t unit, uint16_t bytes_to_write, uint8_t *p_write_data_buffer, uint32_t samples, uint8_t *p_read_data_buffer, uint8_t sync) 
{
	OC_COMMAND cmd;
    STATUS status = STAT_OK;
	uint32_t time_out = 0;
	
	HANDLE_CHECK(handle);

	if(unit < g_spi_num)
		unit = g_spi_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;
   

    SPI_Initialize(&cmd);
    cmd.if_type  = SPI_Interface;
	cmd.if_unit  = unit;
    cmd.command  = acCmd_SPI_CaptureSample;
    cmd.param[0] = cmd.if_unit;
	cmd.param[1] = bytes_to_write;
	cmd.param[2] = samples;
    cmd.data_len = bytes_to_write;
    cmd.pdata    = p_write_data_buffer;

	if(p_write_data_buffer == NULL || p_read_data_buffer ==NULL)
		{
		    DbgWriteText(" p_write_data_buffer = %p, p_read_data_buffer= %p \r\n", p_write_data_buffer, p_read_data_buffer);
		return OC_ERR_SPI_PARAM_ERROR ;  
	    }
	//campture samples canot be 0 || output buffer size cannot be 0
	if(samples == 0 || bytes_to_write == 0)
	{
	     DbgWriteText(" samples = %d, bytes_to_write %d \r\n", samples,bytes_to_write);
	 return OC_ERR_SPI_PARAM_ERROR ;
	}

    if(bytes_to_write <= MAX_PAYLOAD)
	   status = ocSendCommand(handle, &cmd);
	else
      {
		 DbgWriteText("bytes_to_write =%d too big \r\n", bytes_to_write);
		
       return OC_ERR_SPI_PARAM_ERROR ;  
	   }
	if (STAT_OK == status && sync )
    {
        
		uint32_t timeout = samples * bytes_to_write;
		while(timeout)
		{
		status = acWaitForStatus(handle, MAKE_IF(SPI_Interface, 0), 255);
		 if(status == Capture_In_Progress)
				 ac_OS_Sleep(10);
		 else break;

		 timeout --;
		};
		
		if(!timeout)
         return OC_ERR_TIMEOUT;
		uint32_t count;
		if(status == Capture_Sample_Done)
		 {count = acPacket_ReadSync(handle, MAKE_IF(SPI_Interface, 0), 
            p_read_data_buffer, (bytes_to_write*samples), (uint16_t)(255+bytes_to_write));

        nNumberOfBytesRead[handle][0] = count;
        if (count < (samples * bytes_to_write))
            {
		        DbgWriteText("read data error \r\n");
		 	
            status = OC_ERR_DATA_READ_ERROR;
		    }
		 //  status = count;
		}
    } else if (STAT_OK == status )
	{
	    status = acWaitForStatus(handle, MAKE_IF(SPI_Interface, 0), 255);
	}


    return status;
}
acAPI SPI_GetNumberOfBytesRead(AC_HANDLE handle, uint8_t unit, uint16_t *numBytesRead)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;
	 
	HANDLE_CHECK(handle);

	if(unit < g_spi_num)
		unit = g_spi_map[unit];
	else 
	  return OC_ERR_PARAM_OUT_OF_RANGE;
   
    SPI_Initialize(&cmd);

    if (handle >= MAX_SUPPORT_OC && unit >= TIVA_SPI_UNITS)
    {
        status = CompositeStatus(OC_ERR_SPI, OC_ERR_PARAM_OUT_OF_RANGE);
    }
    else if (!bEnabled[handle][unit])
    {
        status = CompositeStatus(OC_ERR_SPI, OC_ERR_NOT_ENABLED);
    }
    else
    {
        *numBytesRead = nNumberOfBytesRead[handle][unit];
    }

    return status;
}