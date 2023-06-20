//*****************************************************************************
//
// OneController oc_script.h functions.
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
//+ 2015-11-10 [RT] - Added prototype for ocSPI_GetNumberOfBytesRead
//

#ifndef __OC_SCRIPT_H
#define __OC_SCRIPT_H

//*****************************************************************************
// All API functions use a "C" calling convention.
//*****************************************************************************
#ifdef  __cplusplus
extern "C" {
#endif


/**
* This function parser script and convert script to command buffer.
* @param pscript script string   
* @param pscript_cmd_buff  the buffer to store script commnad buffer.
* @param buff_bytes   the maximum of pscript_cmd_buff buffer bytes.
* @param used_cmd_buff_size  used command buffer bytes
* @date 3/11/2016
*/
acAPI  Script_Parser(int8_t1 *pscript, uint8_t* pscript_cmd_buff, uint32_t buff_bytes, uint32_t  *used_cmd_buff_bytes);

/**
* This function load script command buffer to the firmware.
* @param handle An open handle for communications 
* @param pscript_cmd_buff  the buffer to store script commnad buffer.
* @param script_cmd_size    pscript_cmd_buff buffer size.
* @param pscript_id   return script ID
* return: 0: success; error: negative value
* @date 3/11/2016
*/

acAPI Script_Load(AC_HANDLE handle,uint8_t *pscript_cmd_buff,uint32_t script_cmd_bytes,uint8_t *pscript_id);

/**
* This function ask firmware to execute script command.
* @param handle An open handle for communications 
* @param pscript_id  script ID from ocScript_Load
* return : 0: sucess; error: negative
* @date 3/11/2016
*/
acAPI Script_Execute(AC_HANDLE handle, uint8_t script_id);

/**
* This function unload script command buffer in the firmware.
* @param handle An open handle for communications 
* @param pscript_id  script ID from ocScript_Load
* return status: 0 or 1 or 2: succsss. error: negative value
* @date 3/11/2016
*/

acAPI Script_Unload(AC_HANDLE handle,uint8_t  script_id);
/**
* This function get the running script status.
* @param handle An open handle for communications 
* @param pscript_id  script ID from ocScript_Load
* return: running script status
* @date 3/11/2016
*/
acAPI  Script_GetScript_RunningStatus (AC_HANDLE handle, uint8_t script_id);
/**
* This function gets the data from running script.
* @param handle An open handle for communications 
* @param pscript_id  script ID from ocScript_Load
* @param pdata buffer to store the data
* @param
* return: read bytes
* @date 3/11/2016
*/

acAPI  Script_GetData(AC_HANDLE handle, uint8_t script_id, uint8_t *pdata, uint32_t bytes_to_read, uint32_t *read_bytes);

/**
* This function send a abort command to stop running script.
* @param handle An open handle for communications 
* @param pscript_id  script ID from ocScript_Load
* @param
* return: send command status 
* @date 3/11/2016
*/
acAPI Script_Abort(AC_HANDLE handle, uint8_t script_id);

#ifdef  __cplusplus
}
#endif

#endif

