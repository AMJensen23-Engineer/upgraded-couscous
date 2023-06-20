
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


static struct braces  l_brace[20] = { B_INVALID};
uint8_t g_left_curly_braces_num =0;
static struct braces loop_brk[20] = { B_INVALID};
uint8_t g_break_num =0;

//check "{,}"
bool is_left_brace(int8_t1 *pcmd)
{



	bool ret = false;

	pcmd = remove_begin_space (pcmd);

	if (*pcmd=='{') {
		char* line_next  = strchr(pcmd, 0x0a);
		if (line_next) {
			*line_next =0;

			if (is_right_comment((int8_t1*)pcmd+1)) {
				*line_next =0xa;
				ret = true;
			}
		}
	}

	return ret;
}

bool is_righ_brace(int8_t1 **pcmd)
{
	bool ret = false;
	*pcmd = remove_begin_space (*pcmd);

	if (**pcmd=='}') {
		char* line_next  = strchr(*pcmd, 0x0a);
		if (line_next)
			*line_next =0;

		if (is_right_comment((*pcmd+1))) {
			*pcmd = line_next +1; //move to next line
			ret = true;
		}
	}
	return ret;
}

//remove the the front of space of string
char * remove_begin_space (char *ptr)
{
	char ch = *ptr;

	while (ch == ' ' || ch == '\t')
		ch = *(++ptr);
	return ptr;
}

//remove the the end of space of string
char * remove_end_space (char *ptr)
{
	char ch = *ptr;

	while (ch)
		ch = *(++ptr);

	ptr--;
	ch = *ptr; 
	while (ch == ' ' || ch == '\t') {
		*(ptr) = 0;
		ch = *(--ptr);
	}
	return ptr;
}

//check if the string is a valid value format
bool is_value_str(const char *ptr)
{
	char ch = *ptr;
	int space_flag =0 ;

	while (ch == ' ' || ch == '\t')
		ch = *(++ptr);

	if (*ptr =='0' && (*(ptr+1) =='x' ||*(ptr+1) =='X')) {

		ptr=(ptr+2);

		while (ch) {

			if (ch == ' ' || ch == '\t') {
				space_flag = 1;
				ch = *(++ptr);
				continue;
			}

			if ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') ||(ch >= 'a' && ch <= 'f') && !space_flag )
				;
			else
				return false;
			ch = *(++ptr);

		}
	} else {
		while (ch) {

			if (ch == ' ' || ch == '\t') {
				space_flag = 1;
				ch = *(++ptr);
				continue;
			}

			if ((ch >= '0' && ch <= '9') && !space_flag )
				;
			else
				return false;
			ch = *(++ptr);

		}

	}
	return true;
}

//conver hex string to a value
unsigned int htoi (const char *ptr)
{
	uint32_t value = 0;
	char ch = *ptr;

	while (ch == ' ' || ch == '\t')
		ch = *(++ptr);

	for (;;) {

		if (ch >= '0' && ch <= '9')
			value = (value << 4) + (ch - '0');
		else if (ch >= 'A' && ch <= 'F')
			value = (value << 4) + (ch - 'A' + 10);
		else if (ch >= 'a' && ch <= 'f')
			value = (value << 4) + (ch - 'a' + 10);
		else
			return value;
		ch = *(++ptr);
	}
}

//conver a decimal string to a value
unsigned int uatoi (const char *ptr)
{
	unsigned int value = 0;
	char ch = *ptr;

/*--------------------------------------------------------------------------*/

	while (ch == ' ' || ch == '\t')
		ch = *(++ptr);

	for (;;) {

		if (ch >= '0' && ch <= '9')
			value = (value *10) + (ch - '0');
		else
			return value;
		ch = *(++ptr);
	}
}

//conver a string to a value
bool stringTovalue(int8_t1 *szStr, uint32_t *value )
{
	int8_t1 * src = szStr;

	if (is_value_str(src)==false)
		return false;
	char ch = *src;

	while (ch == ' ' || ch == '\t')
		ch = *(++src);

	if (*src =='0' && (*(src+1) =='x' ||*(src+1) =='X'))
		*value = htoi(src+2);
	else
		*value = uatoi(src);

	return true;
}


#if 0
bool push_cond_start(uint8_t*ref_p, SCRIPT_COND_TYPE type)
{
	if (stack_depth >= STACK_DEEPTH)
		return false;

	cond_start_ref[stack_depth].pcond_cmd = ref_p ;
	cond_start_ref[stack_depth].type = type;
	cond_start_ref[stack_depth].cur_cmd = nCommandsNum;
	stack_depth++;

	return true;

} 

COND_START *pop_cond_start()
{
	COND_START *pcond_ref = NULL;
	//check the sack is empty
	if (stack_depth) {
		stack_depth--;
		pcond_ref = &cond_start_ref[stack_depth];
	}

	return pcond_ref;
} 

#endif
//get N parameter

#if 0   // unused
bool get_next_parameter_32(int8_t1 *str,uint32_t *value, uint8_t flag )
{
	bool ret = true;

	uint32_t value_bytes[4]= {0xFF,0xFFFF,0xFFFFFFFF};
	const char s[3] = ",";

	static char *token;
	//static char *next_token1 = NULL;  
	/* get the first token */

#ifdef SAFE_STR
    token = strtok_s(str, s);
#else
    token = strtok(str, s);
#endif
	if (token) {
		ret = stringTovalue(token, value );

		if (*value & ~value_bytes[flag -1])
			ret = false;
	} else ret = false ;

	return ret;

}
#endif

bool get_next_parameter(int8_t1 *str,uint32_t *value, uint8_t flag )
{
	bool ret = true;

	uint32_t value_bytes[4]= {0xFF,0xFFFF,0xFFFFFFFF};
	const char s[3] = ",";

	static char *token;
	static char *next_token = NULL;  
	/* get the first token */

	token = strtok_s(str, s, &next_token);
	if (token) {
		ret = stringTovalue(token, value );

		if (*value & ~value_bytes[flag -1])
			ret = false;
	} else ret = false ;

	return ret;

}

//check the top item command type in the stack
bool check_push_item_flag(BRACE_TYPE b_type)
{

	bool ret =false;

	for (int i =0; i < g_left_curly_braces_num; i++) {
		if (l_brace[i].b_type == b_type)
			return true;
	}

	return ret;
}

//push a command type and command buffer 
void push_left_curly_braces(BRACE_TYPE b_type,uint8_t *pscript_cmd_buff)
{
	l_brace[g_left_curly_braces_num].p_cmd= pscript_cmd_buff; 
	l_brace[g_left_curly_braces_num++].b_type = b_type;


}


//pop a command type and command buffer 
LBRACE_TYPE *pop_left_curly_braces(uint8_t flag)
{
	uint8_t index; 
	if (g_left_curly_braces_num > 0) {
		index = g_left_curly_braces_num -1;

		g_left_curly_braces_num -= flag ;

		return &l_brace[index];

	}

	return(LBRACE_TYPE*)NULL;

}

//push a break command and buffer to a stack
void push_loop_break(uint8_t *pscript_cmd_buff)
{
	loop_brk[g_break_num++].p_cmd= pscript_cmd_buff; 


}

//pop break 
LBRACE_TYPE *pop_loop_break()
{
	uint8_t index; 
	if (g_break_num > 0) {
		index = --g_break_num ;


		return &loop_brk[index];

	}

	return(LBRACE_TYPE*)NULL;
}

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
bool is_right_comment(int8_t1 * pstr)
{

	char ch = *pstr;

	while (ch) {
		if (ch == ' ' || ch == '\t')
			ch = *(++pstr);
		else if ( ch =='/' && *(++pstr) == '/')
			return true;
		else return false;
	}
	return true;
}

//, 0xYY 
bool check_fun_last_param(char* p_cmd)
{

	remove_end_space (p_cmd);
	//the 
	char *p_lsr =strrchr(p_cmd,',');
	if (p_lsr) {
		//jump ','
		p_lsr++;

		while (*p_lsr) {
			if (*p_lsr++ !=' ')
				break;
		} 
		while (*p_lsr) {
			if (*p_lsr++ ==' ')
				return false;
		}
	}
	return true;
}

//remove the empty line or comment line from the buffer
void remove_empty_comment_line(int8_t1 **pscript_next)
{
	char *pNext;

	pNext = *pscript_next;


	char * line_next = pNext;



	while (line_next) {


		line_next  = strchr(pNext, 0x0a);
		if (line_next)
			*line_next++ =  0;
		else break;
		//remove front space
		pNext = remove_begin_space (pNext);


		if ( *pNext ==0) {
			//remove empty line
			pNext =  line_next;
			*pscript_next =line_next;
			continue;

		}
		if (processScript_Comment_check(pNext)) {
			//alway go to next line
			pNext =  line_next;
			*pscript_next =line_next;
			continue;
		}

		*(line_next-1) =  0x0a;
		break;
	}


}

//push a error string to the script buffer
bool processScript_error(int8_t1 *pcmd, uint8_t *pscript_cmd_buff)
{
	int8_t1 * psrc = pcmd;
	int8_t1 * pdest =(int8_t1*)pscript_cmd_buff;

	while (*psrc)
		*pdest++ = * psrc++;
	//set the line end
	*pdest = 0;

	return false;
}


//check if the following string is a comment
bool processScript_Comment_check(int8_t1 *pcmd)
{
	//front space is removed
	if (*pcmd == '/' &&  *(pcmd+1) =='/')
		return true;

	return false;
}