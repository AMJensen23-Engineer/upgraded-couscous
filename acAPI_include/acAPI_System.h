//*****************************************************************************
//
// ACController System functions.
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

#ifndef __OCAPI_SYSTEM_H
#define __OCAPI_SYSTEM_H

#include "acAPI_interface.h"

#define MAX_OC_CONNECTIONS      10
#define VIA_SYS_UNITS           6
#define SYS_INTERFACE_ID        0x00
#define MAX_SN_LEN              64
#define VALID_SN_LEN            8

typedef struct
{
    uint32_t    available;
    uint32_t    port;
    uint32_t    type;
    char        serial[MAX_SN_LEN];
} OC_CONNECTION_INFO;

/*****************************************************************************
 * System Functions
 *
 * All API functions MUST use a "C" calling convention.
 ****************************************************************************/
#ifdef  __cplusplus
extern "C" 
{
#endif

/**
* This function is used to enable or disable one System interface unit.
* Parameters:
*    handle     An open handle for communications 
*    unit       The number of the unit to be enabled/disabled.
*    enable     Set true to enable, or false to disable, the SYS unit.
*
* Returns:
*    ACController status code.
*/
acAPI Sys_Enable(AC_HANDLE handle, uint8_t unit, bool enable);

/**
* This function is used to configure one System interface unit.
* Parameters:
*    handle     An open handle for communications 
*    unit       The number of the unit to be configured.
*    enable     Set true to enable, or false to disable, the SYS unit.
*    param1     User-defined parameter
*    param2     User-defined parameter
*
* Returns:
*    ACController status code.
*/
acAPI Sys_Config(AC_HANDLE handle, uint8_t unit, uint32_t param1, uint32_t param2);

/**
* This function is used to find the number of ACController devices connected to the system.
*
* Returns:
*    The number of ACController devices found.
*/
acAPI Sys_FindACControllers();

/**
* This function is used to find the number of ACController devices connected to the system.
*
* Parameters:
*    count   The number OC_CONNECTION_INFO structures available 
*              in the array pointed to by pInfo.
*    pInfo   A pointer to an array of OC_CONNECTION_INFO structures.
*
* Returns:
*    The number of ACController devices found.
*
* Comments:
*    The function always returns the number of ACController devices found. Count can
*    be set to any positive number, with the following guidelines:
*
*    1) If 'count' is set to a number less than the number of devices found, the 
*       function still return the number of all ACController devices found, but only
*       the number of structures specified will be filled in with device information.
*    2) The maximum number of structures that will be filled in is 10.
*/
acAPI Sys_FindACControllersEx(int32_t count, OC_CONNECTION_INFO *pInfo);

/**
* This function is used to get the virtual COM port number of the ACController 
* device associated with the specified index.
*
* Parameters:
*    ACControllerIndex     The index of the ACController device. The index is a number in
*                           the range of 0 to n, where n is the number of devices found by the
*                           ocSys_FindACControllers() function.
*
* Returns:
*   The virtual COM port number of the ACController device associated with the specified index.
*   For example, 2 is returned for COM2, 15 for COM15, etc.
*/
acAPI Sys_GetComPortNumber(uint8_t ACControllerIndex);

/**
* This function is used to open communications on a virtual COM port connected 
* to a ACController device.
*
* Parameters:
*    ComPortNum     The virtual COM port number to which the ACController device
*                   is connected. This is normally obtained by a call to the 
*                   ocSys_GetComPortNumber function.
*
* Returns:
*   On success, returns a handle representing the open communications channel, which 
*   may be used in subsequent API function calls that involve the open port.
*/
acAPI_HANDLE Sys_Open(uint8_t ComPortNum);

/**
* This function is used to open communications on a virtual COM port connected 
* to a ACController device by specifying the serial number of the device.
*
* Parameters:
*    SerialNum      The 10-character serial number of the ACController device that
*                   is connected. This is normally obtained by a call to the 
*                   ocSys_FindACControllersEx function.
*
* Returns:
*   On success, returns a handle representing the open communications channel, which 
*   may be used in subsequent API function calls that involve the open port.
*/
acAPI_HANDLE Sys_OpenBySerialNumber(const char *SerialNum);

/**
* This function is used to close an open communications channel on a virtual COM port 
* connected to a ACController device.
*
* Parameters:
*    Handle     The handle of the open virtual COM port to be closed. The handle must
*               be a value returned by a successful call to ocSys_Open.
*
* Returns:
*   On success, returns OC_SUCCESS (0); otherwise, a negative error code is returned.
*/
acAPI Sys_Close(AC_HANDLE Handle);

/**
* This function is used to get the English text that corresponds to a status code.
*
* Parameters:
*    statusCode  The value of the status code for which to get text.
*    textBuf     A pointer to a buffer to receive the text. The size of the buffer 
*                must be at least 80 bytes. 
*
* Returns:
*   Returns 0 on success or OC_ERR_PARAM_OUT_OF_RANGE (-130) if the statusCode is invalid. 
*   If textBuf is not NULL, the text is copied to the buffer pointed to by textBuf.
*   If statusCode is not a valid value, the text will be "Unknown error message nnn" (nnn is
*   the value of statusCode).
*/
acAPI Sys_GetStatusText(STATUS statusCode, char *textBuf);

/**
* This function is used to get the error code and English text that corresponds to the
* status of the last API function called.
*
* Parameters:
*    handle     An open handle for communications 
*    textBuf    A pointer to a buffer to receive the text. The size of the buffer 
*                should be at least 80 bytes. 
*    len        The length of the buffer that will receive the status text.
*
* Returns:
*   On success, returns the returned status code from the last API function called; otherwise,
*   returns ERR_OPERATION_FAILED (-129).
*   If textBuf is not NULL, the text is copied to the buffer pointed to by textBuf.
*/
acAPI Sys_GetLastError(AC_HANDLE handle, char* textBuf, uint16_t Len);

/**
* This function is used to get information about the connected ACController.
*
* Parameters:
*    handle     An open handle for communications 
*    psInfo     A pointer to a ACControllerINFO structure that will receive the information.
*
* Returns:
*   On success, returns OC_SUCCESS (0) and the target structure will contain valid data. 
*   Otherwise, a negative error code is returned and the contents of the structure are unchanged.
*/
acAPI Sys_GetACControllerInfo(AC_HANDLE handle, ACCONTROLLERINFO *psInfo);


/**
* This function is used to send a command to firmware toreset resource .
*
* Parameters:
*    handle     An open handle for communications 
*
* Returns:
*   On success, returns OC_SUCCESS (0) and the target structure will contain valid data. 
*   Otherwise, a negative error code is returned and the contents of the structure are unchanged.
*/
acAPI Sys_ResetResource(AC_HANDLE handle);

/**
* This function is used to set a hook function for system plug/unplug events.
*
* Parameters:
*    SysEventCallback    Pointer to the function to call on a plug/unplug event.
*
* Returns:
*   On success, returns OC_SUCCESS (0); otherwise, a negative error code is returned.
*/
acAPI Sys_RegisterSysEventNotify( void(* SysEventCallback)( uint32_t EventID,  void *pData, uint16_t DataSize));


/**
* This function is used to set device control for capturing samples.
*
* Parameters:
* @param   handle An open handle for communications 
* @param   if_type_unit interface and unit
* @param   p_dev_ctrl A pointer to a DEV_CTRL structure that contains the parameters to control device .
*
* Returns:
*   On success, returns OC_SUCCESS (0) and the target structure will contain valid data. 
*   Otherwise, a negative error code is returned and the contents of the structure are unchanged.
*/

acAPI Sys_DevCtrl(AC_HANDLE handle, uint16_t if_type_unit,  DEV_CTRL *p_dev_ctrl );

/**
* This function is used to get capturing sample status.
*
* Parameters:
* @param   handle     An open handle for communications 
* @param   if_type_unit interface and unit
*
* Returns:
*   return the capturing sample status 
*/
acAPI Sys_GetCaptureSampleStatus(AC_HANDLE handle, uint16_t if_type_unit) ;

/**
* This function is used to get captured samples .
*
* Parameters:
* @param  handle An open handle for communications 
* @param  if_type_unit interface and unit
* @param  p_sample_buff the buffer to store the samples
* @param  bytes_to_read  read bytes
* Returns:
*   return the capturing sample status 
*/
acAPI Sys_GetSample(AC_HANDLE handle ,uint16_t if_type_unit, uint8_t * p_sample_buff, uint32_t bytes_to_read);

/** send abort command to the firmware to abort the current excuting command

    @param Handle: the handle  returned by ocSys_Open
    @param command: which command to abort
	return: 0, if success
*/
acAPI Sys_AbortCurCommand(AC_HANDLE  Handle, uint32_t command);

/** send reset command to the firmware to reset ACController

    @param Handle: the handle  returned by ocSys_Open
    @param command: reset command
	return:0, if success 
*/
acAPI Sys_ResetCommand(AC_HANDLE Handle);

/** Enable/Disable log out infomation 

    @param Enable: 1: enable log out, 0: disable
    return: 0, if success 
*/
acAPI Sys_EnableDebugLogging(bool Enable);

/** change log file path 

    @param pcLogFilePath: log file path
    return: 0, if success 
*/

acAPI Sys_SetLogFilePath(const char *pcLogFilePath);

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef  __cplusplus
}
#endif

#endif // __OCAPI_SYSTEM_H
