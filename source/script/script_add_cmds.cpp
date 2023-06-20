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
#include <stdlib.h>

#include "acctrl.h"
#include "script_add_cmds.h"
#include "script_util.h"

extern uint8_t g_get_set_return_mode;
extern bool g_overflow_pcmd_buff;
extern uint32_t g_cmd_buff_size;

//add script command header
bool ocScript_AddHeader(int8_t1 * pcmd,uint8_t* pscript_cmd_buff )
{

	char fun_name[40];
	char return_data_mode[40];
	Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd_buff;

	//header set already;
	if (g_get_set_return_mode>1 || pHeader->script_cmd_buff_size)
		return false;

	char *str = strchr(pcmd,',');
	if (str)
		*str =' ';

	//sscanf_s( tokenstring, "%s", s, (unsigned)_countof(s) );  
     //the following code crash 
	uint16_t item_num = sscanf_s(pcmd, " %s %s", fun_name,  (unsigned)_countof(fun_name),return_data_mode,(unsigned)_countof(return_data_mode));

	//uint16_t item_num = sscanf_s(pcmd, " %s %s", fun_name, 40, return_data_mode,);
	
	uint16_t len =(uint16_t) strlen(fun_name);

	if (memcmp(fun_name, "set_return_data_mode", len) )
		return false;

	len =(uint16_t)strlen(return_data_mode);


	if (!memcmp(return_data_mode, "BLOCK_MODE", len) )
		//if(strstr(return_data_mode, "BLOCK_MODE")) 
		pHeader->return_data_type = BLOCK_MODE;
	else if (!memcmp(return_data_mode, "STREAM_MODE", len)) {
		pHeader->return_data_type = STREAM_MODE;
	} else {
		return false;
	}

	if (str) {
		uint32_t value;    
		//get header 
		if (get_next_parameter( (str + 1) ,&value,1 ))
			pHeader->has_return_header = (uint16_t)value;
		else return false;

		//get pkt_size
		if (get_next_parameter( NULL,&value,2 )) {
			pHeader->return_pkt_size = (uint16_t)value;

			//make sure it is last value 
			if (get_next_parameter( NULL, &value, 1 ))
				return false;
		}

	} else return false;

	pHeader->script_cmd_buff_size = 0;

	g_get_set_return_mode ++;

	return true;
}

//get next command buffer pointer
uint8_t *ocScript_GetCurCommandBuff(uint8_t* pscript_cmd_buff)
{
	uint8_t *pcmd = pscript_cmd_buff;

	Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd_buff;

	//move to the right payload position
	pcmd  += sizeof(Script_Packet_Header) +  pHeader->script_cmd_buff_size;


	return pcmd;
}

//add a command data to command buffer
bool ocScript_AddCommand(uint8_t* pscript_cmd_buff, uint16_t if_type_uint, uint16_t script_command, uint8_t *ppayload, uint16_t payload_size )
{

	uint8_t *pcmd = pscript_cmd_buff;

	if (script_command >= SCRIPT_CMD_END)
		return false;

	Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd_buff;

	//move to the right payload position
	pcmd  += sizeof(Script_Packet_Header) +  pHeader->script_cmd_buff_size;

	//adjust cmd buffer size ( action header + payload)
	pHeader->script_cmd_buff_size  +=  sizeof(Script_Action_Header) + payload_size; 
	if ((pHeader->script_cmd_buff_size + sizeof(Script_Packet_Header)) > g_cmd_buff_size) {
		g_overflow_pcmd_buff = true;
		return false;
	}

	Script_Action_Header *pActHeader =(Script_Action_Header *)pcmd;

	//add action header
	pActHeader->if_type_unit = if_type_uint;
	pActHeader->command = script_command;
	pActHeader->payload_len = payload_size;

	//add payload  
	if (ppayload && pActHeader->payload_len) {
		pcmd  += sizeof(Script_Action_Header);
		memcpy(pcmd,ppayload, payload_size);

	}

	return true;
}

