//*****************************************************************************
//
//  OneController script functions.
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


#include <stdio.h>
#include <string.h>

#include "acctrl.h"
#include "acAPI_script.h"


//support download one script
#define VIA_SCRIPT_UNITS   1

static bool bInitialized = false;
static bool bEnabled[MAX_SUPPORT_OC][VIA_SCRIPT_UNITS] ={0};
static uint16_t bScriptMode[MAX_SUPPORT_OC][VIA_SCRIPT_UNITS] ={0};
static uint16_t nNumberOfBytesRead[MAX_SUPPORT_OC][VIA_SCRIPT_UNITS]= {0};


extern bool ecScript_Parser(char *pscript, uint8_t* pscript_cmd_buff, uint32_t cmd_buff_size, uint32_t  *used_cmd_buff_bytes);

//external functions


//parser a script and generate script command buffer
acAPI  Script_Parser(char *pscript, uint8_t* pscript_cmd_buff, uint32_t buff_bytes, uint32_t  *used_cmd_buff_bytes)
{

	bool ret = ecScript_Parser(pscript, pscript_cmd_buff, buff_bytes, used_cmd_buff_bytes); 

	//success return 0, else return OC_ERR_OPERATION_FAILED.
	if (ret == true)
		return STAT_OK;
	else return OC_ERR_OPERATION_FAILED;

}




void Script_Initialize(OC_COMMAND *pCmd)
{
	if (!bInitialized) {
		memset(bEnabled, false, sizeof(bEnabled));
		memset(nNumberOfBytesRead, 0, sizeof(nNumberOfBytesRead));
		bInitialized = true;
	}

	if (pCmd != NULL) {
		InitCommand(pCmd);
	}
}

acAPI Script_Enable(AC_HANDLE handle, uint8_t *unit, bool enable)
{
	//OC_COMMAND cmd;
	STATUS status = OC_ERR_OPERATION_FAILED;;
	int i;
	for (i =0; i < VIA_SCRIPT_UNITS; i++) {
		if (bEnabled[handle][i] == false) {
			*unit = i;
			break;
		}
	}

	if ( i < VIA_SCRIPT_UNITS) {
		status = (0 == acInitIF(handle, IF_SCRIPT, *unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;

		bEnabled[handle][*unit] = true; 

	}

#if 0

	ocScript_Initialize(&cmd);


	if (enable && !bEnabled[handle][unit]) {
		status = (0 == ocSys_InitIF(handle, IF_SCRIPT, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
	}


	if (STAT_OK == status) {
		cmd.if_type  = Script_Interface;
		cmd.if_unit  = unit;
		cmd.command  = ocCmd_Script_Load;
		cmd.param[0] = unit;  
		cmd.param[1] = enable; 


		status = ocSendCommand(handle, &cmd);
		if (STAT_OK == status) {
			nNumberOfBytesRead[handle][unit] = 0;
			status = ocWaitForStatus(handle, MAKE_IF(SPI_Interface, unit), 255);

			/* RT 01-07-16: change the status flag only if successful */
			if (IsSuccess(status)) {
				bEnabled[handle][unit] = enable;
			}
		}
	}

#endif
	return status;
}

//load a script to the frimware
acAPI Script_Load(AC_HANDLE handle,uint8_t *pscript_cmd,uint32_t script_cmd_size,uint8_t  *pscript_id)
{
	STATUS status = OC_ERR_OPERATION_FAILED;

	HANDLE_CHECK(handle);

	if (pscript_cmd == NULL || pscript_id == NULL || script_cmd_size == 0 ) {
		DbgWriteText(" pscript_cmd = %p, pscript_id= %p, script_cmd_size =%d \r\n", pscript_cmd, pscript_id, script_cmd_size);
		return OC_ERR_SCRIPT_PARAM_ERROR ;  
	}

	if (Script_Enable(handle, (uint8_t*)pscript_id, 1)!= OC_ERR_OPERATION_FAILED) {
		OC_COMMAND cmd;
		Script_Initialize(&cmd);

		cmd.if_type  = Script_Interface;
		cmd.if_unit  = *pscript_id;
		cmd.command  = acCmd_Script_Load;
		cmd.param[0] = *pscript_id;
		cmd.param[2] = script_cmd_size;
		cmd.data_len = script_cmd_size;
		cmd.pdata    = pscript_cmd;

		//record return data type
		Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd;
		bScriptMode[handle][*pscript_id] = pHeader->return_data_type; 


		if (script_cmd_size <= MAX_PAYLOAD) {

			status = ocSendCommand(handle, &cmd);
			if (STAT_OK == status) {
				uint16_t time_out=0;   

				do {

					status = acWaitForStatus(handle, MAKE_IF(Script_Interface, *pscript_id), 255);
					time_out +=55; 
				}while (status != STAT_OK && time_out < script_cmd_size);

			}
		}

		else {

			status = ocSendCommand_BigData(handle, &cmd);

			if (STAT_OK == status) {
				uint16_t time_out=0;   

				do {

					status = acWaitForStatus(handle, MAKE_IF(Script_Interface, *pscript_id), 255);
					time_out +=55; 
				}while (status != STAT_OK && time_out < script_cmd_size);

			}

		}

	}

	if (status == STAT_OK)
		bEnabled[handle][*pscript_id] = true;

	return status;
}

//unload the script from the firmware
acAPI Script_Unload(AC_HANDLE handle,uint8_t  script_id)
{
	STATUS status = OC_ERR_OPERATION_FAILED;;



	HANDLE_CHECK(handle);
	//send unload command

	if (script_id >= VIA_SCRIPT_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	if (!bEnabled[handle][script_id])
		return OC_ERR_INVALID_FUNCTION_CODE;

	OC_COMMAND cmd;


	Script_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type  = Script_Interface;
	cmd.if_unit  = script_id;
	cmd.command  = acCmd_Script_Unload;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status) {
		status = acWaitForStatus(handle, MAKE_IF(Script_Interface, script_id), 1255);
	}

	//release script interface
	if (bEnabled[handle][script_id]) {
		status = (0 == acUnInitIF(handle, IF_SPI, script_id)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
		//should not care return status.
		bInitialized = false;
		bEnabled[handle][script_id] = false;
	}

	return status;
}

//send an execute command to the firmware
acAPI Script_Execute(AC_HANDLE handle, uint8_t script_id)
{

	OC_COMMAND cmd;
	STATUS status;


	HANDLE_CHECK(handle);

	if (script_id >= VIA_SCRIPT_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	if (!bEnabled[handle][script_id])
		return OC_ERR_INVALID_FUNCTION_CODE;

	Script_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type  = Script_Interface;
	cmd.if_unit  = script_id;
	cmd.command  = acCmd_Script_Execute;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status) {
		status = acWaitForStatus(handle, MAKE_IF(Script_Interface, script_id), 1255);
	}

	return status;

}

//check the script running status
acAPI  Script_GetScript_RunningStatus (AC_HANDLE handle, uint8_t script_id)
{
	HANDLE_CHECK(handle);

	if (script_id >= VIA_SCRIPT_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	if (!bEnabled[handle][script_id])
		return OC_ERR_INVALID_FUNCTION_CODE;

	return acWaitForStatus(handle, MAKE_IF(Script_Interface, script_id), 255);;
}

//get the script running result
acAPI  Script_GetData(AC_HANDLE handle, uint8_t script_id, uint8_t *pdata, uint32_t read_to_bytes, uint32_t *read_bytes )
{
	HANDLE_CHECK(handle);

	if (script_id >= VIA_SCRIPT_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	if (!bEnabled[handle][script_id])
		return OC_ERR_INVALID_FUNCTION_CODE;

	if ( bScriptMode[handle][script_id] == BLOCK_MODE) {
		//
		*read_bytes = acPacket_ReadSync(handle, MAKE_IF(Script_Interface, script_id), (uint8_t*)pdata, read_to_bytes,100);
	} else {
		//stream mode
	}

	return STAT_OK;
}

//abort thr script execution
acAPI Script_Abort(AC_HANDLE handle, uint8_t script_id)
{
	uint8_t unit = 0;
	OC_COMMAND cmd;
	STATUS status = OC_ERR_OPERATION_FAILED;

	HANDLE_CHECK(handle);

	if (script_id >= VIA_SCRIPT_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	if (!bEnabled[handle][script_id])
		return OC_ERR_INVALID_FUNCTION_CODE;

	Script_Initialize(&cmd);
	InitCommand(&cmd);

	cmd.command = acCmd_Sys_AbortScript;

	status = ocSendCommand(handle, &cmd);

	if (STAT_OK == status) {
		status = acWaitForStatus(handle, MAKE_IF(Script_Interface, script_id), 1255);
	}


	return status;
}