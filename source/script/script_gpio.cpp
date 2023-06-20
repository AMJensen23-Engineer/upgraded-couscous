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
#include "script_util.h"
#include "script_add_cmds.h"
#pragma warning( disable : 4996)

extern int8_t1 *pScript_Sys_Variable[];
//gpio_read GPIO_00;
static bool script_gpio_read(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = false;
	char fun_name[40];
	char gpio_pin[40];
	char junk[40];
	Script_GPIO_Read pin;
	//Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd_buff;

	int item_num = sscanf(pcmd, " %s %s %s", fun_name, gpio_pin,junk);
	//make sure one two parameters gpio_read GPIO_XX;
	if ((item_num ==2)) {
		int index = 0;
		int len  = (int)strlen(gpio_pin);
		while (pScript_Sys_Variable[index]) {
			if (!memcmp(gpio_pin, pScript_Sys_Variable[index], len) ) {
				pin.pin_mask = (GPIO_00 << index);
				break;
			}
			index++;
		}

		if (pScript_Sys_Variable[index] == NULL)
			return false;

		return ocScript_AddCommand( pscript_cmd_buff,  MAKE_IF(GPIO_Interface,0), SCRIPT_GPIO_READ, (uint8_t*)&pin, sizeof(Script_GPIO_Read));
	}

	return ret;
}


//gpio_write GPIO_00, 1 ;
static bool script_gpio_write(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
//	bool ret;
	char fun_name[40];
	char gpio_pin[40];
	uint32_t value;
	Script_GPIO_Write gpio_w;

	char *str = strchr(pcmd,',');
	if (str)
		*str =' ';
	else return false;

	int item_num = sscanf(pcmd, " %s %s", fun_name, gpio_pin);

	if ((item_num ==2)) {
		int index = 0;
		int len  = (int)strlen(gpio_pin);
		while (pScript_Sys_Variable[index]) {
			if (!memcmp(gpio_pin, pScript_Sys_Variable[index], len) ) {
				gpio_w.pin_mask = (GPIO_00 << index);
				break;
			}
			index++;
		}

		if (pScript_Sys_Variable[index] == NULL)
			return false;

	} else return false;


	if ( str && get_next_parameter( (str + 1) ,&value,1 )) {
		if (value ==0 || value ==1)
			gpio_w.value = (uint8_t)value;
		else return false;
	} else return false;

	//make sure only one value 
	if ( get_next_parameter(NULL,&value,1 ))
		return false;



	return ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(GPIO_Interface,0), SCRIPT_GPIO_WRITE, (uint8_t*)&gpio_w, sizeof(Script_GPIO_Write));

}

//GPIO_00 ==1 ;
static bool script_gpio_status(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	//int ret;

	Script_GPIO_Status gpio_status;
	//Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd_buff;

#if 0
	int item_num = sscanf(pcmd, " %s %s,%d", fun_name, gpio_pin, &value);

	if ((item_num == 3) && (value ==0 || value ==1)) {
		int index = 0;
		while (pScript_Sys_Variable[index]) {
			if (!memcmp(gpio_pin, pScript_Sys_Variable[index], strlen(pScript_Sys_Variable[index])) ) {
				gpio_w.pin_mask = (GPIO_00 << index);
				gpio_w.value = value;  
				break;
			}
		}

		if (pScript_Sys_Variable[index] == NULL)
			return -1;

	} else return -1;
#endif
	return ocScript_AddCommand( pscript_cmd_buff, GPIO_Interface, SCRIPT_GPIO_CONDITION, (uint8_t*)&gpio_status, sizeof(Script_GPIO_Status));

}

//process GPIO read/write functions
bool processScript_GPIO_funs(FUN_API_INDEX index, int8_t1 *p_cmd,uint8_t* pscript_cmd_buff)
{
	bool ret = false;


	switch (index) {
	case GPIO_READ:

		ret= script_gpio_read(p_cmd,pscript_cmd_buff);
		break;
	case GPIO_WRITE:

		ret = script_gpio_write(p_cmd,pscript_cmd_buff);
		break;
	case GPIO_STATUS:

		ret = script_gpio_write(p_cmd,pscript_cmd_buff);
		break;
	default:
		break;
	}

	return ret;
}

