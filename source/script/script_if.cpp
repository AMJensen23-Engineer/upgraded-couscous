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

extern int8_t1 *pScript_Sys_Variable[];
#define GPIO_NAME_LEN   7

//get if condition
static bool script_get_con_status(int8_t1 *pcmd,uint8_t *pscript_cmd_buff)
{
	bool ret =false;
	Script_If_Start if_start;
	Script_GPIO_Status gpio_status;

	//remove sapce
	pcmd = remove_begin_space (pcmd);

	//assume only one condition GPIO_XX==
	char *pfun_name =strstr(pcmd,"GPIO_");

	if (pcmd == pfun_name) {
		char fun_name[10];
		memcpy(fun_name,pfun_name, GPIO_NAME_LEN);
		int index =0;

		//int gpio_name_len  = GPIO_NAME_LEN;

		while (pScript_Sys_Variable[index]) {
			if (!memcmp(fun_name, pScript_Sys_Variable[index], GPIO_NAME_LEN) ) {
				break;
			}
			index++;
		}

		//just support GPIO
		if (pScript_Sys_Variable[index] == NULL)
			return false;

		gpio_status.pin_mask = (GPIO_00 << index);

		//space between GPTO_xx"  "==
		pcmd = remove_begin_space ((pcmd + GPIO_NAME_LEN));

		if (*pcmd !='=' || *(pcmd+1) !='=')
			return false;

		//move '=='
		pcmd+=2;

#if 0

		con_or = strstr(pcmd,"||");

		if (con_or) {
			*con_or = 0;
			if_start.con_logic[0] =COND_OR;

		} else {

			con_and = strstr(pcmd,"&&");

			if (con_and) {
				*con_and = 0;
				if_start.con_logic[0] = COND_AND;

			}
		}
#endif	  
		uint32_t gpio_con ;
		//if(!StringToBinary(pcmd, &gpio_con ) || (gpio_con !=1 && gpio_con !=0))
		//  return false;

		if (stringTovalue((pcmd), &gpio_con )) {
			if (gpio_con !=1 && gpio_con !=0)
				return false;
		} else
			return false;


		//initailize
		if_start.condition_cmd_num = 0;
		if_start.con_logic[0] =0;
		if_start.con_logic[1] =0;

		gpio_status.condition_value = gpio_con;

		//push if pointer to statck
		push_left_curly_braces(B_IF_START, ocScript_GetCurCommandBuff(pscript_cmd_buff));

		if_start.condition_cmd_num ++;

		ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(Script_Interface,0), SCRIPT_IF_START, (uint8_t*)&if_start, sizeof(Script_If_Start));

		ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(GPIO_Interface,0), SCRIPT_GPIO_CONDITION, (uint8_t*)&gpio_status, sizeof(Script_GPIO_Status));   
	}
	return ret;
}

static bool script_if_start(int8_t1 *pcmd,uint8_t *pscript_cmd_buff)
{
	bool ret = true;
	//uint32_t gpio_mask;
	//uint32_t gpio_con_value;

	char *p_lstr;

	p_lstr = pcmd +2;

	char ch= *p_lstr;

	while (ch == ' ' || ch == '\t')
		ch = *(++p_lstr);

	if (*(p_lstr) !='(')
		return false;


	if (p_lstr) {

		char *p_rstr = strchr(p_lstr,')');

		if (!is_right_comment(p_rstr+1))
			return false;

		if (p_rstr) {
			//uint32_t value;

			if (!is_right_comment(p_rstr+1))
				return false;

			*p_rstr = 0;

			if (!script_get_con_status(p_lstr +1,pscript_cmd_buff))
				return false;


		}
	}

	//add gpio status as a condition, need to fix
	//gpio_status.pin_mask = gpio_mask;
	//gpio_status.condition_value = gpio_con_value;

	//ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(GPIO_Interface,0), GPIO_STATUS, (uint8_t*)&gpio_status, sizeof(Script_GPIO_Status));


	return ret;
}

static bool script_if_end(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = false;
//   Script_Loop_End loop_end;


	if (!is_right_comment(pcmd+1))
		return false;

	if (!check_push_item_flag(B_IF_START)) {
		ret = processScript_error("miss if",(uint8_t*)pcmd);
		return false;
	}

	LBRACE_TYPE *b_type = pop_left_curly_braces(REMOVE_STACK);


	if ((b_type && b_type->b_type == B_IF_START)) {

		Script_If_Start *p_if_start =(Script_If_Start *)(b_type->p_cmd + sizeof(Script_Action_Header));
		p_if_start->if_end_offset =(int16_t)(ocScript_GetCurCommandBuff(pscript_cmd_buff) - b_type->p_cmd);
		return(ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(Script_Interface,0), SCRIPT_IF_END, NULL,0));

	}


	return ret;
}

static bool script_else_start(int8_t1 *pcmd,uint8_t *pscript_cmd_buff)
{
	bool ret = false;
	Script_Else_Start else_start;

	push_left_curly_braces(B_ELSE_START, ocScript_GetCurCommandBuff(pscript_cmd_buff));

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(Script_Interface,0), SCRIPT_ELSE_START, (uint8_t*)&else_start, sizeof(Script_Else_Start));


	return ret;
}

static bool script_else_end(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = false;
//   Script_Loop_End loop_end;


	if (!is_right_comment(pcmd+1))
		return false;

	if (!check_push_item_flag(B_ELSE_START)) {
		ret = processScript_error("miss else ",(uint8_t*)pcmd);
		return false;
	}

	LBRACE_TYPE *b_type = pop_left_curly_braces(REMOVE_STACK);

	if ((b_type && b_type->b_type == B_ELSE_START)) {

		Script_Else_Start *p_else_start =(Script_Else_Start *)(b_type->p_cmd + sizeof(Script_Action_Header));
		p_else_start->else_end_offset =(int32_t) (ocScript_GetCurCommandBuff(pscript_cmd_buff)-b_type->p_cmd);
		return(ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(Script_Interface,0), SCRIPT_ELSE_END, NULL,0));

	}

	return ret;
}

bool processScript_cond(uint16_t cmd_index,int8_t1 *pcmd, int8_t1 **pscript_next, uint8_t *pscript_cmd_buff)
{
	bool ret = false; 
	switch (cmd_index) {
	
	case IF_START: 
		//remove the empty line and comment line 
		remove_empty_comment_line(pscript_next);

		//check "{"
		if (is_left_brace(*pscript_next)) {
			//move to next command line
			char* line_next  = strchr(*pscript_next + 1, 0x0a);
			if (line_next)
				*pscript_next = line_next+1;

			ret = script_if_start(pcmd,pscript_cmd_buff);

		} else {
			ret  = processScript_error("no { after if\n",(uint8_t *)pcmd);
		}

		break;
	case ELSE_START:
		remove_empty_comment_line(pscript_next);
		if (is_left_brace(*pscript_next)) {
			//move to next command line
			char* line_next  = strchr(*pscript_next + 1, 0x0a);
			if (line_next)
				*pscript_next = line_next+1;

			ret = script_else_start(pcmd,pscript_cmd_buff);

		} else {
			ret = processScript_error("no { after else \n",(uint8_t *)pcmd);
		}
		break;
	case IF_ELSE_END:
		{
			LBRACE_TYPE *b_type = pop_left_curly_braces(0);

			if (b_type && b_type->b_type == IF_START)
				ret = script_if_end(pcmd,pscript_cmd_buff);
			else if (b_type && b_type->b_type == ELSE_START)
				ret = script_else_end(pcmd,pscript_cmd_buff);

		}
		break;

	default:
		break;

	}
	return ret;
}
