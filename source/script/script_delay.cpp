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
#include "acAPI_script.h"
#include "script_util.h"
#include "script_add_cmds.h"

//delayuS ;
static bool script_delayuS(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = false;

	char fun_name[40];
	char delayuS_Value[40];
	uint32_t value;
	Script_Delay delayuS;

	int item_num = sscanf_s(pcmd, " %s %s", fun_name, (unsigned)_countof(fun_name), delayuS_Value,(unsigned)_countof(delayuS_Value));

	if ((item_num ==2)) {
		if (stringTovalue(delayuS_Value, &value )  )
			delayuS.delayuS = value;

	} else return false;

	return ocScript_AddCommand( pscript_cmd_buff, System_Interface, SCRIPT_DELAY, (uint8_t*)&delayuS, sizeof(Script_Delay));
}

bool processScript_Delay_fun(FUN_API_INDEX index, int8_t1 *p_cmd,uint8_t* pscript_cmd_buff)
{
	bool ret = false;


	switch (index) {
	case DELAY:

		ret= script_delayuS(p_cmd,pscript_cmd_buff);
		break;
	default:
		break;
	}

	return ret;
}

