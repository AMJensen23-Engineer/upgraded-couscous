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

#ifndef __SCRIPT_UTIL_H
#define __SCRIPT_UTIL_H

#include "script_add_cmds.h"
//In script, no value should be large as, this value definition is for return status
#define INVALID_VALUE   0xEFFFFFEE

//the flag to remove the state from the satck
typedef enum {
	NOMOVE_STACK =0,
	REMOVE_STACK,
}STATCK_TYPE;
//
typedef struct braces {

	BRACE_TYPE b_type; //brace related command type
	uint8_t * p_cmd; //the command buffer pointer
}LBRACE_TYPE;

char * remove_begin_space (char *ptr);
char * remove_end_space (char *ptr);
bool stringTovalue(int8_t1 *szStr, uint32_t *value );
bool get_next_parameter(int8_t1 *str,uint32_t *value, uint8_t flag );
bool check_push_item_flag(BRACE_TYPE b_type);



void push_left_curly_braces(BRACE_TYPE b_type,uint8_t *pscript_cmd_buff);


LBRACE_TYPE *pop_left_curly_braces(uint8_t flag);


void push_loop_break(uint8_t *pscript_cmd_buff);

LBRACE_TYPE *pop_loop_break();

#if 0
bool is_right_curly_braces(int8_t1 *pscript)
{
	if (*pscript =='}') {
		return true;  
	}
	return false;
}
#endif

//check the right comment
bool is_right_comment(int8_t1 * pstr);

//, 0xYY 
bool check_fun_last_param(char* p_cmd);
//check "{,}"
bool is_left_brace(int8_t1 *pcmd);
bool is_righ_brace(int8_t1 **pcmd);
void remove_empty_comment_line(int8_t1 **pscript_next);
bool processScript_error(int8_t1 *pcmd, uint8_t *pscript_cmd_buff);
bool processScript_Comment_check(int8_t1 *pcmd);

#endif 