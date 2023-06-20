
//*****************************************************************************
//
//  ACController System functions.
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
#include <Windows.h>
#include <string.h>
#include <stdio.h>

#include "acctrl.h"
#include "acctrl_api.h"
#include "Version.h"


#define DEFAULT_TIMEOUT         1000

static bool bInitialized = false;
static bool bEnabled[VIA_SYS_UNITS];
static uint16_t nNumberOfBytesRead[VIA_SYS_UNITS];

static void Sys_Initialize(OC_COMMAND *pCmd, uint8_t unit)
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
        pCmd->if_type = SYS_INTERFACE_ID;
        pCmd->if_unit = unit;
    }
}

acAPI Sys_Enable(AC_HANDLE handle, uint8_t unit, bool enable)
{
    STATUS status = OC_ERR_OPERATION_FAILED;

	HANDLE_CHECK(handle);

    if (enable && !bEnabled[handle])
    {
        status = (0 == acInitIF(handle, IF_SYS, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
    }
    else
    {
        status = bEnabled[handle] ? STAT_OK : OC_ERR_NOT_ENABLED;
    }

    return status;
}

acAPI Sys_FindACControllers() 
{
    STATUS onectrl_num = ac_OSCDC_FindACControllers(); 

	DbgWriteText("Sys_FindACControllers: Find %d ACController \r\n", onectrl_num);
    return onectrl_num;
}

acAPI Sys_GetComPortNumber(uint8_t ACControllerIndex)
{
    return ac_OSCDC_GetComPortNumber(ACControllerIndex);  
}

acAPI_HANDLE Sys_Open(uint8_t ComPortNum)
{
    //oc_OSCDC_FindACControllers();
    return ac_OSCDC_Open(ComPortNum);
}

acAPI_HANDLE Sys_OpenBySerialNumber(const char *serial)
{
    OC_CONNECTION_INFO info[MAX_OC_CONNECTIONS];
    STATUS port;
    STATUS status;

    if (NULL == serial)
    {
        return OC_ERR_PARAM_OUT_OF_RANGE;
    }
    else if (strlen(serial) > MAX_SN_LEN)
    {
        return OC_ERR_PARAM_OUT_OF_RANGE;
    }

    port = ac_OSCDC_GetComPortFromSerial(serial);
    if (IsError(port))
    {
        STATUS status = Sys_FindACControllersEx(MAX_OC_CONNECTIONS, info);
       
        if (IsSuccess(status))  // on success, status will be the number of controllers found
        {
            int32_t ix;

            for (ix = 0; ix < status; ++ix)
            {
                if (strcmp(serial, info[ix].serial) == 0)
                {
                    port = info[ix].port;
                    break;
                }
            }

            if (ix == status)   // serial number not found?
            {
                status = OC_ERR_DEVICE_NOT_FOUND;
            }
        }
    }

    if (IsSuccess(port))
    {
        status = ac_OSCDC_Open(port);
    }
    
    return (AC_HANDLE) status;
}

acAPI Sys_Close(AC_HANDLE Handle)
{
	HANDLE_CHECK(Handle);
	
	return ac_OSCDC_Close(Handle);
   //	g_Sys_Manager.Sys_UnInitACController(Handle);
}

/* RT 01-07-16: Changed Sys_GetStatusText() to conform to ocAPI */
acAPI Sys_GetStatusText(STATUS statusCode, char *textBuf)
{
    static char staticBuf[80];

    strcpy_s(staticBuf, sizeof (staticBuf), GetStatusText(statusCode));
    if (NULL != textBuf)
    {
        strcpy_s(textBuf, 80, staticBuf);
    }

    return ('?' == staticBuf[0]) ? OC_ERR_PARAM_OUT_OF_RANGE : OC_SUCCESS;
}

STATUS Sys_GetIFLastError(AC_HANDLE Handle, char *ErrStr, uint16_t Len);
acAPI Sys_GetLastError(AC_HANDLE Handle, char* ErrStr, uint16_t Len)
{
	char buf[128];
	STATUS status = Sys_GetIFLastError(Handle, buf, sizeof(buf));

	if (ErrStr != NULL)
	{
		strncpy_s(ErrStr, Len, buf, sizeof(buf));
	}

	return status;
}

acAPI Sys_GetACControllerInfo(AC_HANDLE handle, ACCONTROLLERINFO *psInfo)
{
    uint8_t unit = 0;
    OC_COMMAND cmd;
    STATUS status = OC_ERR_SYS_INVALID_HANDLE;
    ACCONTROLLERINFO info;
    uint32_t bytes_to_read = sizeof(info);

	memset(&info, 0, sizeof(info));
	memset(psInfo, 0, sizeof(info));

    if (STAT_OK == HANDLE_STATUS(handle))
    {
        // the SYS interface must be enabled to receive data
        Sys_Enable(handle, unit, true);

        Sys_Initialize(&cmd, unit);
        cmd.command = acCmd_Sys_GetInfo;
        status = ocSendCommand(handle, &cmd);
        if (STAT_OK == status)
        {
            uint32_t count = acPacket_ReadSync(handle, MAKE_IF(SYS_INTERFACE_ID, unit), (uint8_t *) &info, bytes_to_read, 255);

            nNumberOfBytesRead[handle] = count;
            if (count < bytes_to_read)
            {
                status = OC_ERR_NOT_ENOUGH_DATA;
            }
            else
            {
                status = count;
            }
        }
    }

    if (IsError(status))
    {
	    DbgWriteText("Sys_GetACControllerInfo: Failed to get info from firmware\r\n");

        strcpy_s(info.BoardName, sizeof(info.BoardName), "N/A");
        strcpy_s(info.BoardRevStr, sizeof(info.BoardRevStr), "N/A");
        strcpy_s(info.BoardSerialNumber, sizeof(info.BoardSerialNumber), "N/A");
    }

	memcpy(psInfo, &info, sizeof(ACCONTROLLERINFO));

	// set the DLL version info 
	psInfo->DLLVersion[0] = VER_MAJOR;
	psInfo->DLLVersion[1] = VER_MINOR;
	psInfo->DLLVersion[2] = VER_REVISION;
	psInfo->DLLVersion[3] = VER_BUILD;

    return status;
}

acAPI Sys_ResetResource(AC_HANDLE handle)
{
    uint8_t unit = 0;
    OC_COMMAND cmd;
    STATUS status = OC_ERR_SYS_INVALID_HANDLE;
		
	
    if (STAT_OK == HANDLE_STATUS(handle))
    {
        // the SYS interface must be enabled to receive data
      
		  // the SYS interface must be enabled to receive data
        Sys_Enable(handle, unit, true);

        Sys_Initialize(&cmd, unit);
        cmd.command = acCmd_Sys_ResetResource;

		status = ocSendCommand(handle, &cmd);
     
		if (STAT_OK == status)
        {
          status = acWaitForStatus(handle, MAKE_IF(SYS_INTERFACE_ID, unit), 255);
        }
	}
    return status;
}

acAPI Sys_DevCtrl(AC_HANDLE handle, uint16_t if_type_unit,  DEV_CTRL *p_dev_ctrl )
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;
	uint32_t time_out = 0;
	
	 HANDLE_CHECK(handle);

    Sys_Initialize(&cmd, 0);
    cmd.if_type  = (if_type_unit>>8);
	cmd.if_unit  = (if_type_unit & 0xFF);
    cmd.command  = acCmd_Sys_DevCtrl;
    cmd.param[0] = cmd.if_unit;
	cmd.param[1] = sizeof(DEV_CTRL);
	cmd.data_len = sizeof(DEV_CTRL);
    cmd.pdata    =(uint8_t*) p_dev_ctrl;
	
	if(p_dev_ctrl->active_mode == ACTIVE_MODE)
	{
		// a pin must be a trigger pin
		if(p_dev_ctrl->ctrl_pin[0].pin_type != TRIGGER_PIN_TYPE && p_dev_ctrl->ctrl_pin[1].pin_type != TRIGGER_PIN_TYPE)
			return OC_ERR_PARM_WRONG;
	    //only allow one pin a trigger pin
		if(p_dev_ctrl->ctrl_pin[0].pin_type == TRIGGER_PIN_TYPE && p_dev_ctrl->ctrl_pin[1].pin_type == TRIGGER_PIN_TYPE)
			return OC_ERR_PARM_WRONG;
	    
		//must set at lease a pin    
		 if(strstr(g_sInfo.BoardName,"ACController"))//if the board is one controller
		{
			
			if((p_dev_ctrl->ctrl_pin[0].gpio_pin == GPIO_10 || p_dev_ctrl->ctrl_pin[0].gpio_pin == GPIO_11) || (p_dev_ctrl->ctrl_pin[1].gpio_pin == GPIO_10 || p_dev_ctrl->ctrl_pin[1].gpio_pin == GPIO_11) )
		   ;
		else
			return OC_ERR_PARM_WRONG;
		}

#if 1	
//	uint8_t pin_num = 0;
	uint8_t gpio_pin;
	uint16_t bit =1;
	
	if(p_dev_ctrl->ctrl_pin[0].pin_type != INVALID_PIN )
	{	

		//change gpio pin from logical pin number to physica pin name
	  for (gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin, bit <<= 1)
       {
         if (p_dev_ctrl->ctrl_pin[0].gpio_pin & bit)     // enable/disable this pin?
         {
          
			 p_dev_ctrl->ctrl_pin[0].gpio_pin = (g_gpio_pin_name[gpio_pin][1]<<8);
			 p_dev_ctrl->ctrl_pin[0].gpio_pin |= g_gpio_pin_name[gpio_pin][2];
			 break;
         }
       }

	    if(gpio_pin == g_gpio_pin_num)
			return OC_ERR_PARM_WRONG;
	}	
		
	if(p_dev_ctrl->ctrl_pin[1].pin_type != INVALID_PIN )
	{	
	  //change gpio pin from logical pin number to physica pin name
	
	  bit =1;
	  for (gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin, bit <<= 1)
       {
         if (p_dev_ctrl->ctrl_pin[1].gpio_pin & bit)     // enable/disable this pin?
         {
            p_dev_ctrl->ctrl_pin[1].gpio_pin = (g_gpio_pin_name[gpio_pin][1]<<8);
		    p_dev_ctrl->ctrl_pin[1].gpio_pin |= g_gpio_pin_name[gpio_pin][2];
			break;
         }
       }
       
	    if(gpio_pin == g_gpio_pin_num)
			return OC_ERR_PARM_WRONG;
  
	}	
		
	if(p_dev_ctrl->ctrl_pin[2].pin_type != INVALID_PIN )
	{	
	
	  //change gpio pin from logical pin number to physica pin name
	  bit =1;
	  for (gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin, bit <<= 1)
       {
         if (p_dev_ctrl->ctrl_pin[2].gpio_pin & bit)     // enable/disable this pin?
         {
          p_dev_ctrl->ctrl_pin[2].gpio_pin = (g_gpio_pin_name[gpio_pin][1]<<8);
		  p_dev_ctrl->ctrl_pin[2].gpio_pin |= g_gpio_pin_name[gpio_pin][2];
          break;
         }
       }
      
	    if(gpio_pin == g_gpio_pin_num)
			return OC_ERR_PARM_WRONG;
 
	}	


		
	  
#endif	
		//sample rate and sample bit must be larger than 0
		if(p_dev_ctrl->sample_rate == 0 || p_dev_ctrl->sample_bits ==0)
			return OC_ERR_PARM_WRONG;

	}else
	{//pasive mode
	
	}

	
	status = ocSendCommand(handle, &cmd);
	
	if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, if_type_unit, 255);
    }

    return status;
	
}

acAPI  Sys_GetCaptureSampleStatus(AC_HANDLE handle, uint16_t if_type_unit) 
{

    uint8_t unit = 0;
    STATUS status;
    
    HANDLE_CHECK(handle);
     
	status = acWaitForStatus(handle, if_type_unit, 255);

	return status;
}

acAPI Sys_GetSample(AC_HANDLE handle ,uint16_t if_type_unit, uint8_t * p_sample_buff, uint32_t bytes_to_read)
{
    
    HANDLE_CHECK(handle);
   
	return acPacket_ReadSync(handle, if_type_unit, p_sample_buff, bytes_to_read, 255);
	
}

acAPI Sys_FindACControllersEx(int32_t count, OC_CONNECTION_INFO *pInfo) 
{
	OC_CONNECTION_INFO info;
	STATUS devcount = Sys_FindACControllers();

	
    // reset count if it's greater than the number of devices present
    count = min(count, devcount);

	 for (int i = 1; i <= count; ++i)
    {
        memset(&info, 0, sizeof(OC_CONNECTION_INFO));

        info.port = g_Sys_Manager.psOC_Sys[i]->Com_Port_Num;
        strcpy_s(info.serial, sizeof(info.serial), g_Sys_Manager.psOC_Sys[i]->oc_UsbAttr);
        if(pInfo != NULL)
       		memcpy(&pInfo[i-1], &info, sizeof(OC_CONNECTION_INFO));
		else return OC_ERR_PARAM_OUT_OF_RANGE;  

		STATUS hnd = ac_OSCDC_OpenComPort(pInfo[i-1].port);

        if (IsSuccess(hnd))
         {
            ac_OSCDC_CloseComPort(hnd);
            pInfo[i-1].available = true;
         }
         else
          {
                pInfo[i-1].available = false;
          }
	 }

#if 0
    // do this only if no error occurred, there are devices, and pInfo is not NULL
    if (devcount > 0 && pInfo != NULL) 
    {
        // make sure we don't overflow the user's buffer
        devcount = min(devcount, (int32_t) count);

        for (int i = 0; i < devcount; ++i)
        {
            // check to see if device is available
            OC_STATUS hnd = oc_OSCDC_OpenComPort(pInfo[i].port);

            if (IsSuccess(hnd))
            {
                oc_OSCDC_CloseComPort(hnd);
                pInfo[i].available = true;
            }
            else
            {
                pInfo[i].available = false;
            }
        }
    }
#endif
    return count;
}

acAPI Sys_EnableDebugLogging(bool Enable)
{
	ocSet_EnableLog(Enable);

	return OC_SUCCESS;
}

acAPI Sys_SetBoardLED_unsupport(AC_HANDLE Handle, uint8_t  LedUnit, uint8_t  BlinkStyle, uint8_t Color)
{

	int read_headr = sizeof(OCF_PACKET_HEADER);
	int32_t write_bytes;
	OCF_PACKET_HEADER spktHeader;

	spktHeader.signature = PACKET_SIGNATURE;
	spktHeader.type = COMMAND_PACKET;
	spktHeader.if_type_unit = IF_SYS;
	spktHeader.command = acCmd_LED_Config;
	spktHeader.param[0] = LedUnit;
	spktHeader.param[1] = BlinkStyle;
	spktHeader.param[1] = Color;
	write_bytes = acPacket_Write(Handle, (uint8_t*)&spktHeader, read_headr);
	//read_bytes = ocPacket_ReadSync(Handle, (IF_SYS<<8), (uint8_t*)psInfo, sizeof(ACControllerINFO),100);
	if (write_bytes == read_headr)
		return 0;

	return -1;
}

acAPI Sys_ResetCommand(AC_HANDLE Handle)
{
	int read_headr = sizeof(OCF_PACKET_HEADER);
	int32_t write_bytes;
	OCF_PACKET_HEADER spktHeader;

	HANDLE_CHECK(Handle);
	spktHeader.signature = PACKET_SIGNATURE;
	spktHeader.type = COMMAND_PACKET;
	spktHeader.if_type_unit = 0;
	spktHeader.command = acCmd_Sys_Restart;
	write_bytes = acPacket_Write(Handle, (uint8_t*)&spktHeader, read_headr);
	DbgWriteText("Sys_ResetCommand:Send Reset command\r\n");
	return 0;
}
acAPI Sys_AbortCurCommand(AC_HANDLE  Handle, uint32_t command)
{

	int read_headr = sizeof(OCF_PACKET_HEADER);
	int32_t write_bytes;
	OCF_PACKET_HEADER spktHeader;

	HANDLE_CHECK(Handle);
	spktHeader.signature = PACKET_SIGNATURE;
	spktHeader.type = COMMAND_PACKET;
	spktHeader.if_type_unit = command;
	spktHeader.command = acCmd_Sys_Abort;
	write_bytes = acPacket_Write(Handle, (uint8_t*)&spktHeader, read_headr);
	DbgWriteText("Sys_AbortCurCommand: Send abort command:%d\r\n", command);
	return 0;
}

void acSet_Notify(void(*SysEventCallback)(uint32_t EventID, void *pData, uint16_t DataSize));
acAPI Sys_RegisterSysEventNotify(void(*SysEventCallback)(uint32_t EventID, void *pData, uint16_t DataSize))
{
	if (SysEventCallback)
	{
		acSet_Notify(SysEventCallback);
	}
	else
	{
		DbgWriteText("Sys_RegiterSysEventNotify:callback pointer is null\r\n");
		return OC_ERR_OPERATION_FAILED;
	}

	return OC_SUCCESS;
}
STATUS Sys_GetIFLastError(AC_HANDLE Handle, char *ErrStr, uint16_t Len)
{
	if ((Handle> 0 || Handle < MAX_SUPPORT_OC) && ErrStr && strlen(g_LastFirmwareErrStr[Handle]) < Len)
	{
		if (ErrStr != NULL)
		{
#ifdef SAFE_STR
			strncpy_s(ErrStr, Len, g_LastFirmwareErrStr[Handle], Len);
#else
			strncpy(ErrStr, g_LastFirmwareErrStr[Handle], Len);
#endif
		}

		return g_LastFirmwareErrCode[Handle];
	}

	return OC_ERR_OPERATION_FAILED;
}

void ONECTRL_API WIN_API Sys_GetError(int8_t1* ErrStr, uint16_t Len)
{
	if (ErrStr && strlen(g_LastErrStr) < Len)
	{
#ifdef SAFE_STR
		strcpy_s(ErrStr, Len, g_LastErrStr);
#else
		strcpy(ErrStr, g_LastErrStr);
#endif
	}
}
acAPI Sys_SetLogFilePath(const char *pcLogFilePath)
{
	if (ocLog_SetLogFileName(pcLogFilePath))
	{
		DbgWriteText("Sys_SetLogFilePath:Cannot Set log file : %s\r\n", pcLogFilePath);
		return OC_ERR_OPERATION_FAILED;
	}

	return OC_SUCCESS;
}
