
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


static bool script_loop_start(int8_t1 *pcmd,uint8_t *pscript_cmd_buff)
{
	bool ret = false;
	Script_Loop_Start loop_start;
	char *p_lstr,*p_rstr;

	p_lstr = pcmd+ 4;

	char ch= *p_lstr;

	while (ch == ' ' || ch == '\t')
		ch = *(++p_lstr);

	if (*(p_lstr) !='(')
		return false;


	if (p_lstr) {
		p_rstr = strchr(p_lstr,')');
		if (p_rstr) {
			uint32_t value;

			if (!is_right_comment(p_rstr+1))
				return false;

			*p_rstr = 0;

			if (stringTovalue((p_lstr+1), &value )) {
				if (value)
					loop_start.iteration =value;
				else return false;
			} else {
				char ch= *(p_lstr+1);
				char found_s =0;
				char found_1 =0;

				//-1 is infinite loop
				while (ch == ' ' || ch == '\t' || ch =='-' || ch == '1' ) {
					ch = *(++p_lstr);

					if ( ch =='-')
						found_s++;

					if ( ch =='1')
						found_1++;
				}

				if (!ch && found_s == 1 && found_1 == 1)
					loop_start.iteration = -1;
				else return false;

			}
		} else return false;
	}

	push_left_curly_braces(B_LOOP_START, ocScript_GetCurCommandBuff(pscript_cmd_buff));

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(Script_Interface,0), SCRIPT_LOOP_START, (uint8_t*)&loop_start, sizeof(Script_Loop_Start));


	return ret;
}

static bool script_loop_end(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = false;
	Script_Loop_End loop_end;

	if (!is_right_comment(pcmd+1))
		return false;

	if (!check_push_item_flag(B_LOOP_START)) {
		ret = processScript_error("miss loop start",(uint8_t*)pcmd);
		return false;
	}


	LBRACE_TYPE *bl_type = pop_loop_break();
	while (bl_type) {

		Script_Loop_Break *p_loop_break =(Script_Loop_Break *)(bl_type->p_cmd + sizeof(Script_Action_Header));
		p_loop_break->loop_break_to_end_offset =(int16_t)(ocScript_GetCurCommandBuff(pscript_cmd_buff)-bl_type->p_cmd);
		bl_type = pop_loop_break();

	}
	LBRACE_TYPE *b_type = pop_left_curly_braces(REMOVE_STACK);

	while (b_type) {

		if ((b_type && b_type->b_type == B_LOOP_START)) {

			Script_Loop_Start *p_loop_start =(Script_Loop_Start *)(b_type->p_cmd + sizeof(Script_Action_Header));
			p_loop_start->loop_start_to_end_offset =(int16_t)(ocScript_GetCurCommandBuff(pscript_cmd_buff) - b_type->p_cmd);
			loop_end.loop_end_to_start_offset =(int16_t)(b_type->p_cmd - ocScript_GetCurCommandBuff(pscript_cmd_buff)) ;
			return(ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(Script_Interface,0), SCRIPT_LOOP_END, (uint8_t*)&loop_end, sizeof(Script_Loop_End)));

		}


	}

	return ret;
}

static bool script_loop_break(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = false;
	Script_Loop_Break loop_break;

	char *p_str = strchr(pcmd,';');

	if (p_str == NULL)//no ; at the end line
		return false;

	//check comment after ;
	if (!is_right_comment(p_str +1))
		return false;

	while (1) {
		p_str--;

		if (*p_str ==' ' || *p_str =='\t')
			continue;

		if (*p_str =='k'&& *(--p_str) =='a')
			break;

		else return false;
	}

	//check the privous function
	if (!check_push_item_flag(B_LOOP_START)) {
		ret = processScript_error("break: miss loop start",(uint8_t*)pcmd);
		return false;
	}

	push_loop_break(ocScript_GetCurCommandBuff(pscript_cmd_buff));

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(Script_Interface,0), SCRIPT_LOOP_BREAK, (uint8_t*)&loop_break, sizeof(Script_Loop_Break));

	return ret;
}

//process loop 
bool processScript_loop(uint16_t cmd_index,int8_t1 *pcmd, int8_t1 **pscript_next, uint8_t *pscript_cmd_buff)
{
	bool ret =false; 
	switch (cmd_index) {
	
	case LOOP_START: 
		{

			remove_empty_comment_line(pscript_next);
			if (is_left_brace(*pscript_next)) {//check {
				//move to next command line
				char* line_next  = strchr(*pscript_next + 1, 0x0a);
				if (line_next)
					*pscript_next = line_next+1;

				ret = script_loop_start(pcmd,pscript_cmd_buff);

			} else { //miss { after loop
				ret = processScript_error("loop miss { \n",(uint8_t *)pcmd);
			}
		}
		break;
	case LOOP_END:
		ret = script_loop_end(pcmd,pscript_cmd_buff);
		break;
	case LOOP_BREAK:
		ret = script_loop_break(pcmd,pscript_cmd_buff);
		break;

	default:
		break;
	}
	return ret;
}
