//*****************************************************************************
//
// onectrl.h - Defines onecotrol API.
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
//
// Change History
//
//+ 2015-11-10 [RT] - Added HANDLE_CHECK() macro 
//+ 2015-11-10 [RT] - Moved API function prototypes to ocAPI_System.cpp
//

#ifndef __ONECONTROLLER_H
#define  __ONECONTROLLER_H


#include "global.h"
#include "ocf_common.h"
#include "acAPI_interface.h"
//#include "io_win\io_os.h"
#include "io_os.h"
#include "ac_sys.h"
#include "log.h"
#include "acAPI_command.h"



/*globa MACRO definition */
/* Maximum support 5 OneControllers*/
#define MAX_SUPPORT_OC  6 

/* global variable*/
//extern OC_SYS  *g_psOC_Sys[MAX_SUPPORT_OC];
extern SYS_Manager g_Sys_Manager;;
/**/

/*Macro definition*/
#define HANDLE_STATUS(handle) \
        (((handle < 1 || handle >= MAX_SUPPORT_OC) || !g_Sys_Manager.psOC_Sys[handle]) \
            ? OC_ERR_SYS_INVALID_HANDLE : STAT_OK)

#define HANDLE_CHECK(handle) \
    { \
        if (STAT_OK != HANDLE_STATUS(handle)) \
        { \
            DbgWriteText("oc_handle = %d, handle out of range\r\n", handle); \
            return OC_ERR_SYS_INVALID_HANDLE; \
        } \
    }  

#ifdef  __cplusplus
extern "C" {
#endif
/** @defgroup group_wvdll_api API

    All of the public functions in the OneControll DLL are documented in
    each of these submodules.  These submodules are divided into
    functional groups.

    
    @{
*/

/** Initalize the interface

@param Handle: the handle  returned by ocSys_Open
@param IF_TYPE: which interface, such I2c, spi
@param Uint: unit of interface, such as i2c bus number
return: 0, if success
*/
acAPI acInitIF(int32_t Handle, uint8_t IF_TYPE, uint8_t Uint);

/** UnInitalize the interface 

    @param Handle: the handle  returned by ocSys_Open
    @param IF_TYPE: which interface, such I2c, spi
	@param Uint: unit of interface, such as i2c bus number
	return: 0, if success 
*/
acAPI acUnInitIF(int32_t Handle, uint8_t IF_TYPE, uint8_t Uint);


/** send command to turn on/off led on OneController board 

    @param Handle: the handle  returned by ocSys_Open
    @param LedUnit: which Led
	@param BlinkStyle: blink times between on and off
	@param Color: turn color  
	return: 0, if success 
*/


acAPI ocSys_SetBoardLED(int32_t Handle , uint8_t  LedUnit,  uint8_t  BlinkStyle,  uint8_t Color);

 
/** @} */

/* ------------------------------------------------------------------- */ /**

    @defgroup packet read and write routines

    Functions called to send  packets to the firmware and read the data from firmware

    @{

*/
/* -------------------------------------------------------------------- */

/** flush write buffer 

    @param Handle: the handle  returned by ocSys_Open
   return: 0 
*/

acAPI acPacket_WriteFlush(int32_t Handle);

/** send data to the firmware 

    @param Handle: the handle  returned by ocSys_Open
    @param pData: write data buffer
	@param DataSize: data buffer bytes
	return: write bytes 
*/

acAPI acPacket_Write(int32_t Handle, uint8_t *pData, uint32_t  DataSize);

/** get the if status 

    @param Handle: the handle  returned by ocSys_Open
    @param IF: interface type and uint (IF_TYPE<<8)|Uint
	@param pStatus: the current interface status sent back by the firmware 
	return: write bytes 
*/
acAPI acPacket_GetIFStatus(int32_t  Handle, uint16_t IF, STATUS *pStatus);


/** reset the if status 
    bfor calling write/rad command, call this routine to reset the interface 
    @param Handle: the handle  returned by ocSys_Open
    @param IF: interface type and uint (IF_TYPE<<8)|Uint
	return: write bytes 
*/

acAPI acPacket_ResetIFStatus(int32_t  Handle, uint16_t IF);

/** Read data from the firmware 

    @param Handle: the handle  returned by ocSys_Open
    @param pData: read data buffer
	@param DataSize: bytes to read
	@param timeout: timeout to wait fro the data ready
	return: read bytes 
*/

acAPI acPacket_ReadSync(int32_t  Handle, uint16_t IF, uint8_t *pData, uint32_t DataSize, uint16_t Timeout);
/** send data to the firmware 

    @param Handle: the handle  returned by ocSys_Open
    @param pData: read data buffer, this buffer cannot be released if return is 0. the callback routine will use this buffer to hold the read data
	@param DataSize: data buffer bytes
	@param DataReadyCallback: 
	return: read bytes. If return is 0, the callback routine will be invoked when the data is ready. 
*/
acAPI acPacket_ReadASync(int32_t Handle, uint16_t IF, uint8_t* pData, uint32_t DataSize,  void (*DataReadyCallback)(uint8_t *pData, uint16_t  DataSize));

STATUS acWaitForStatus(AC_HANDLE handle, uint16_t IF, int32_t timeout);

STATUS acGetReceivedData(AC_HANDLE handle, uint8_t intf, uint8_t unit, uint8_t *pData, 
                            uint16_t bytes_to_read, uint8_t timeout);

STATUS acSys_GetIFLastError(int32_t Handle, char* ErrStr, uint16_t Len);

/** @} */


/* ------------------------------------------------------------------- */ /**

    @defgroup power routines

    Functions called to set ouput power

    @{

*/
/* -------------------------------------------------------------------- */

/** set output power 

    @param Handle: the handle  returned by ocSys_Open
    @param OutputP1: set pi volatge value
	@param p5v: enable 5v: 1: enable, 0:disable
	return: If return is 0 
*/
//ocAPI ocPower_EnableOutput(int32_t Handle, uint8_t OutputP1, uint8_t Enable5v0);

/** get output power 

    @param Handle: the handle  returned by ocSys_Open
    @param p1: p1 staus 
	@param p5v: 5v status
	return: If return is 0 
*/

//ocAPI ocPower_GetOutputStatus(int32_t Handle, uint8_t*p1, uint8_t *p5v);

/** @} */

/* ------------------------------------------------------------------- */ /**

    @defgroup i2c routines

    Functions to read/write i2c

    @{

*/
/* -------------------------------------------------------------------- */

/** @} */


/* ------------------------------------------------------------------- */ /**

    @defgroup gpio routines

    Functions to configurate get/set gpio

    @{

*/
/* -------------------------------------------------------------------- */
/*functions*/


/** @} */



/** @} */

#ifdef  __cplusplus
}
#endif
#endif
