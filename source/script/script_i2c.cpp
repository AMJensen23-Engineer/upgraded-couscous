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

//i2c_read unit,devAddr,registerAddr,flags, read_bytes
static bool script_i2c_read(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	//bool ret;
	uint32_t value;

	Script_I2C_Read i2c_read;
	//Script_Packet_Header *pHeader =(Script_Packet_Header*)pscript_cmd_buff;


	char *str = strchr(pcmd,' ');

	//get unit number
	if (get_next_parameter( str,&value,1 ))
		i2c_read.unit = (uint8_t)value;
	else return false;

	//get i2c device address
	if (get_next_parameter( NULL,&value,1 ))
		i2c_read.deviceAddr = (uint8_t)value;
	else return false;

	//get registerAddr
	if (get_next_parameter( NULL,&value,3 ))
		i2c_read.registerAddr = (uint32_t)value;
	else return false;

	//get flags
	if (get_next_parameter( NULL,&value,1 ) &&(value== 0 || value == 1))
		i2c_read.flags = (uint8_t)value;
	else return false;

	//get bytes_to_read
	if (get_next_parameter( NULL,&value,3 )) {

		i2c_read.bytes_to_read = (uint32_t)value; 
		if (!i2c_read.bytes_to_read && i2c_read.bytes_to_read > MAX_DATA_BUFF_SIZE )
			return false;
	} else return false;

	//more data 
	if (get_next_parameter( NULL,&value,1 ))
		return false;

	if (i2c_read.flags == 0  &&  (i2c_read.registerAddr & ~0xFF))
		return false;

	return ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(I2C_Interface,i2c_read.unit), SCRIPT_I2C_READ, (uint8_t*)&i2c_read, sizeof(Script_I2C_Read));
}

//i2c_write unit,devAddr,registerAddr,flags,v0,v1,v2,...vn
static bool script_i2c_write(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = true;
	uint32_t value;
	uint32_t num_value = 0;

	uint8_t *ppay_load =(uint8_t*)malloc(64*1024);
	if (ppay_load == NULL)
		return false;


	Script_I2C_Write *i2c_write =(Script_I2C_Write *)ppay_load ;
	uint8_t *pwrite_data = (uint8_t*)&i2c_write->data_buffer; 


	char *str = strchr(pcmd,' ');

	//get unit number
	if (get_next_parameter( str,&value,1 ))
		i2c_write->unit = (uint8_t)value;
	else return false;

	//get i2c device address
	if (get_next_parameter( NULL,&value,1 ))
		i2c_write->deviceAddr = (uint8_t)value;
	else return false;

	//get registerAddr
	if (get_next_parameter( NULL,&value,3 ))
		i2c_write->registerAddr = (uint32_t)value;
	else return false;

	//get flags
	if (get_next_parameter( NULL,&value, 1 ) &&(value==0 || value ==1))
		i2c_write->flags = (uint8_t)value;
	else return false;

	num_value = 0;
	while (1) {

		//initalize the value
		value = INVALID_VALUE;
		if (ret = get_next_parameter( NULL,&value,1 )) {

			*pwrite_data ++ = (uint8_t)value; 

			num_value ++;  
		} else {
			//check if the write value wrong
			if (value == INVALID_VALUE)
				i2c_write->bytes_to_write = num_value;
			else return false;

			break;
		}

	}

	if (!i2c_write->bytes_to_write || i2c_write->bytes_to_write > MAX_DATA_BUFF_SIZE )
		return false;

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(I2C_Interface,i2c_write->unit), SCRIPT_I2C_WRITE, (uint8_t*)ppay_load,( ((uint16_t)sizeof(Script_I2C_Write) -1) + num_value) );

	free(ppay_load);

	return ret;

}


//process script i2c read/write functions
bool processScript_I2C_funs(FUN_API_INDEX index,int8_t1 *pscript,uint8_t* pscript_cmd_buff)
{

	switch (index) {
	case I2C_READ:

		return script_i2c_read(pscript,pscript_cmd_buff);

	case I2C_WRITE:

		return script_i2c_write(pscript,pscript_cmd_buff);
	default:
		break;

	}
	return false;
}
