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

#pragma warning( disable : 4996)

static bool script_uart_read(int8_t1 *pcmd,uint8_t * pscript_cmd_buff)
{
	bool ret = true;
	uint32_t value;
	uint32_t num_value = 0;



	Script_UART_Read uart_read;


	char *str = strchr(pcmd,' ');

	//get unit number
	if (get_next_parameter( str,&value,1))
		uart_read.unit = (uint8_t)value;
	else return false;

	//get read bytes
	if (get_next_parameter( NULL,&value,3 ))
		uart_read.bytes_to_read = (uint32_t)value;
	else return false;
	if (!uart_read.bytes_to_read || uart_read.bytes_to_read > MAX_DATA_BUFF_SIZE )
		return false;

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(UART_Interface,uart_read.unit), SCRIPT_UART_READ, (uint8_t*)&uart_read,sizeof(Script_UART_Read));

	return ret;
}

//spi_write  unit, cs, v0,v1,…vn
static bool script_uart_write(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = true;
	uint32_t value;
	uint32_t num_value = 0;

	uint8_t *ppay_load =(uint8_t*)malloc(32*1024);
	if (ppay_load == NULL)
		return false;


	Script_UART_Write *uart_write =(Script_UART_Write *)ppay_load ;
	uint8_t *pwrite_data = (uint8_t*)&uart_write->data_buffer; 


	char *str = strchr(pcmd,' ');

	//get unit number
	if (get_next_parameter( str,&value,1 ))
		uart_write->unit = (uint8_t)value;
	else return false;



	num_value = 0;
	while (1) {


		//initalize valure
		value = INVALID_VALUE;

		if (ret = get_next_parameter( NULL,&value,1 )) {
			*pwrite_data ++ = (uint8_t)value; 
			num_value ++;  
		} else {

			//check if the write value wrong
			if (value == INVALID_VALUE)
				uart_write->bytes_to_write = num_value;
			else return false;

			break;
		}

	}

	if (!uart_write->bytes_to_write || uart_write->bytes_to_write > MAX_DATA_BUFF_SIZE )
		return false;

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(UART_Interface,uart_write->unit), SCRIPT_UART_WRITE, (uint8_t*)ppay_load, (uint16_t)(sizeof(Script_UART_Write) -1) + num_value );

	free(ppay_load);

	return ret;
}
bool processScript_UART_funs(FUN_API_INDEX index,int8_t1 *pscript,uint8_t* pscript_cmd_buff)
{
	bool ret;
	switch (index) {
	
	case UART_READ:
		ret = script_uart_read(pscript,pscript_cmd_buff);
		break;
	case UART_WRITE:
		ret = script_uart_write(pscript,pscript_cmd_buff);
		break;
	}

	return ret;
}

