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
#include "script_gpio.h"
#include "script_i2c.h"
#include "script_spi.h"
#include "script_uart.h"
#include "script_if.h"
#include "script_loop.h"
#include "script_delay.h"

#include "acAPI_script.h"

#pragma warning( disable : 4996)
//Script variable name, function name and logical operator and condition name
char *pScript_Sys_Variable[]= {"GPIO_00","GPIO_01","GPIO_02","GPIO_03","GPIO_04","GPIO_05","GPIO_06","GPIO_07","GPIO_08","GPIO_09","GPIO_10",
	"GPIO_11","GPIO_12","GPIO_13","GPIO_14","GPIO_15",NULL}; 


char *pScript_Fun_Name[]={  "set_return_data_mode","delayuS","gpio_read","gpio_write", "i2c_read","i2c_write","spi_read","spi_write","spi_writeandread",
	"uart_read","uart_write", NULL};
char *pScript_Logic_Name[] = {"||", "'&&", "==",NULL};
char *pScript_Cond_Name[]  = {"if","else","}",NULL};
char *pScript_Loop_Name[]  = {"loop", "break","}",NULL};

char *pScript_Start_End[]  = {"ScriptStart:","ScriptEnd"};

extern uint8_t g_left_curly_braces_num;
extern uint8_t g_break_num ;




static bool found_ScriptStart = false;
static bool found_ScriptEnd = false;
uint8_t g_get_set_return_mode = 0;
bool g_overflow_pcmd_buff = false;

uint32_t g_cmd_buff_size;






//internal API
//Script function API


//check function apis
static bool is_funs(int8_t1 *pcmd,uint16_t *api_index )
{
	int index = 0;
	bool ret = false;

	char fun_name[40];

	sscanf(pcmd, " %s ", fun_name);
	int len = (int)strlen(fun_name);

	while (pScript_Fun_Name[index]) {

		if (!memcmp(fun_name, pScript_Fun_Name[index], len)) {
			*api_index =  index;
			// nCommandsNum++;
			break;    
		}
		index++;
	}

	if (pScript_Fun_Name[index]) {
		//get the end of string ';'.
		int8_t1 *p_sc = strchr( pcmd,';');

		if (p_sc) {
			//remove ';'
			if (is_right_comment(p_sc + 1)) {
				*p_sc =0;
				ret = true;
			}
		}

	}

	return ret;
}

//check loop 
static bool is_loop(int8_t1 *pscript,uint16_t *cmd_index)
{
	bool ret = false;
	int index = 0;



	while (pScript_Loop_Name[index]) {

		if (!memcmp(pscript, pScript_Loop_Name[index], strlen(pScript_Loop_Name[index]))) {
			*cmd_index =  index;
			break;    
		}
		index++;
	}

	if (pScript_Loop_Name[index] && index == LOOP_END  ) {

		LBRACE_TYPE *b_type=pop_left_curly_braces(NOMOVE_STACK);


		if (b_type->b_type == B_LOOP_START)
			return true;
		else return false;
	}

	if (pScript_Loop_Name[index])
		ret = true;

	return ret;

}


static bool is_condition(int8_t1 *pscript,uint16_t *cmd_index)
{
	bool ret = false;
	int index = 0;
	while (pScript_Cond_Name[index]) {

		if (!memcmp(pscript, pScript_Cond_Name[index], strlen(pScript_Cond_Name[index]))) {
			*cmd_index =  index;
			break;    
		}
		index++;
	}

	if (pScript_Loop_Name[index] && index == IF_ELSE_END  ) {

		LBRACE_TYPE *b_type=pop_left_curly_braces(NOMOVE_STACK);

		if (b_type->b_type == B_IF_START || b_type->b_type == B_ELSE_START)
			return true;
		else return false;
	}

	if (pScript_Cond_Name[index])
		ret = true;

	return ret;

}


static bool process_Return_Data_Mode_funs(int8_t1 *pscript,uint8_t* pscript_cmd_buff)
{

	return ocScript_AddHeader(pscript, pscript_cmd_buff );

}

static bool processScript_fun(uint16_t cmd_index,int8_t1 *pscript, uint8_t *pscript_cmd_buff)
{

	bool ret = false; 

	int len = (int)strlen(pscript);

	int8_t1 *p_cmd =(int8_t1*)malloc(len+4);

	if (p_cmd) {
		strcpy(p_cmd,pscript);

	} else return false;

	//last parameter is like ,param ; 
	if (!check_fun_last_param(p_cmd))
		return false;

	switch (cmd_index) {
	case SET_RETURN_DATA_MODE:
		ret = process_Return_Data_Mode_funs(p_cmd, pscript_cmd_buff);   
		break;
	case DELAY:
		ret = processScript_Delay_fun((FUN_API_INDEX)cmd_index, p_cmd,pscript_cmd_buff); 
		break;
	case GPIO_READ:
	case GPIO_WRITE:
		ret = processScript_GPIO_funs((FUN_API_INDEX)cmd_index, p_cmd,pscript_cmd_buff);
		break;
	case I2C_READ:
	case I2C_WRITE:
		ret = processScript_I2C_funs((FUN_API_INDEX)cmd_index, p_cmd,pscript_cmd_buff);
		break ;
	case SPI_READ:
	case SPI_WRITE:
	case SPI_WRITEANDREAD:
		ret = processScript_SPI_funs((FUN_API_INDEX)cmd_index, p_cmd,pscript_cmd_buff);
		break; 

	case UART_READ:
	case UART_WRITE:
		ret = processScript_UART_funs((FUN_API_INDEX)cmd_index, p_cmd,pscript_cmd_buff);
		break;
	default:

		break;
	}

	free(p_cmd);
	return ret;
}




static bool processScript_Boundary_check(int8_t1 *pcmd, uint8_t *pscript_cmd_buff)
{
	//check the script start point
	bool ret = false;       

	if ( found_ScriptStart == false && *pcmd=='S') {

		if (!memcmp(pcmd, "ScriptStart:",12)) {

			if (is_right_comment(pcmd + 12)) {
				found_ScriptStart = true;
				ret = true;
			}
		}
	}

	//check the first command and it must be set_return_data_mode
	if (g_get_set_return_mode == 0 && *pcmd =='s') {
		if (strstr(pcmd,"set_return_data_mode")) {
			g_get_set_return_mode = 1;

		}

	}

	//check script end 
	if ( *pcmd =='S' && found_ScriptEnd == false) {

		if (!memcmp(pcmd, "ScriptEnd",9)) {

			if (is_right_comment(pcmd + 9)) {

				found_ScriptEnd = true;
				ret = true;
			}

		}
	}
	return  ret;
}




static void script_init_parser()
{

	found_ScriptStart = false;
	found_ScriptEnd = false;
	g_get_set_return_mode = 0;
	g_overflow_pcmd_buff = false;
	g_left_curly_braces_num =0;
	g_break_num = 0;

}

//parser script
bool ecScript_Parser(char *pscript, uint8_t* pscript_cmd_buff, uint32_t cmd_buff_size, uint32_t  *used_cmd_buff_bytes)
{
	bool ret = false;
	uint16_t script_len;

	if (pscript ==NULL || pscript_cmd_buff ==NULL || cmd_buff_size < (sizeof(Script_Packet_Header) + sizeof(Script_Action_Header)) )
		return false;//OC_ERR_SCRIPT_PARAM_ERROR;

	Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd_buff;
	pHeader->script_cmd_buff_size = 0;

	script_len = (uint16_t)strlen(pscript);

	
	g_cmd_buff_size = cmd_buff_size;

	char  *pScript_cpy = (char*) malloc(script_len + 4);
	char *pNext;

	if (pScript_cpy) {
		memcpy(pScript_cpy,pscript, script_len + 2); 
		pNext = pScript_cpy;
	} else
		return false;//OC_ERR_SCRIPT_PARAM_ERROR;

	char * line_next = pNext;

	//initalize the parameters
	script_init_parser();


	while (line_next) {

		//remove front space
		pNext = remove_begin_space (pNext);

		line_next  = strchr(pNext, 0x0a);
		if (line_next)
			*line_next++ =  0;	//move to next line
		else if (strlen(pNext)) { //for the last line which doesn't end with \n
			line_next = NULL;
		} else break;

		ret = false;


		if ( *pNext ==0) {
			//remove empty line
			pNext =  line_next;
			continue;

		}
		if (processScript_Comment_check(pNext)) {
			//always go to next line
			pNext =  line_next;
			continue;
		}

		if (processScript_Boundary_check(pNext, pscript_cmd_buff)) {
			//always go to next line
			pNext =  line_next;
			ret = true;
			continue;
		}


		//after found the start of script and set return mode 
		if (found_ScriptStart && g_get_set_return_mode &&  found_ScriptEnd ==false ) {
			uint16_t cmd_index;
			//check function 
			if (is_funs(pNext, &cmd_index))
				ret = processScript_fun(cmd_index,pNext, pscript_cmd_buff);
			//check loop command	  
			else if (is_loop(pNext,&cmd_index))
				ret = processScript_loop(cmd_index,pNext, &line_next,pscript_cmd_buff);
			//check if/else command   	 
			else if (is_condition(pNext,&cmd_index))
				ret = processScript_cond(cmd_index,pNext, &line_next,pscript_cmd_buff);
			else
				ret	= false;


		}

		if (!ret && found_ScriptEnd ==false ) {
			if (g_overflow_pcmd_buff) {
				Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd_buff;
				*used_cmd_buff_bytes = sizeof(Script_Packet_Header) + pHeader->script_cmd_buff_size ;
				processScript_error("script_cmd_buff size is too small",pscript_cmd_buff);
			} else
				processScript_error(pNext,pscript_cmd_buff);
			return ret; 
		}
		ret = true;
		pNext =  line_next;


	} //end while

	//find the start of script
	if (!found_ScriptStart)
		ret = processScript_error("don't find ScriptStart:",pscript_cmd_buff);
	//find the return dat amode  
	else if (!g_get_set_return_mode)
		ret = processScript_error("don't find set_return_mode",pscript_cmd_buff);
	//find the end of script
	else if (!found_ScriptEnd)
		ret  = processScript_error("don't find ScriptEnd, must make sure ScriptEnd have change line(\n) ",pscript_cmd_buff);
	else {

		//the if/esle/loop stack must be empty
		LBRACE_TYPE *b_type = pop_left_curly_braces(REMOVE_STACK);

		if (b_type) {
			switch (b_type->b_type) {
			case B_IF_START:
				ret  = processScript_error("if miss '}'",pscript_cmd_buff);
				break;
			case  B_ELSE_START:
				ret  = processScript_error(" else miss '}'",pscript_cmd_buff);
				break;
			case B_LOOP_START:
				ret  = processScript_error( " loop  miss '}'",pscript_cmd_buff);
				break;
			default:
				ret  = processScript_error("miss '}'",pscript_cmd_buff);
			}
		}
	}


	if (ret)
		*used_cmd_buff_bytes = sizeof(Script_Packet_Header) + pHeader->script_cmd_buff_size ;
	else
		*used_cmd_buff_bytes = 0;


	free(pScript_cpy); 

	return ret;
}