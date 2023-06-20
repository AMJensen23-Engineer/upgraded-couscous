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


//spi_write  unit, cs, v0,v1,…vn
static bool script_spi_write(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = true;
	uint32_t value;
	uint32_t num_value = 0;

	uint8_t *ppay_load =(uint8_t*)malloc(32*1024);
	if (ppay_load == NULL)
		return false;


	Script_SPI_Write *spi_write =(Script_SPI_Write *)ppay_load ;
	uint8_t *pwrite_data = (uint8_t*)&spi_write->data_buffer; 


	char *str = strchr(pcmd,' ');

	//get unit number
	if (get_next_parameter( str,&value,1 ))
		spi_write->unit = (uint8_t)value;
	else return false;

	//get spi chip select
	if (get_next_parameter( NULL,&value,1 ))
		spi_write->cs = (uint8_t)value;
	else return false;


	num_value = 0;
	while (1) {


		//initalize valure
		value =0xfffff;

		if (ret = get_next_parameter( NULL,&value,1 )) {
			*pwrite_data ++ = (uint8_t)value; 
			num_value ++;  
		} else {

			//check if the write value wrong
			if (value ==0xfffff)
				spi_write->bytes_to_write = num_value;
			else return false;

			break;
		}

	}

	if (!spi_write->bytes_to_write || spi_write->bytes_to_write > MAX_DATA_BUFF_SIZE )
		return false;

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(SPI_Interface,spi_write->unit), SCRIPT_SPI_WRITE, (uint8_t*)ppay_load, ((uint16_t)(sizeof(Script_SPI_Write) -1) + num_value) );

	free(ppay_load);

	return ret;
}

//spi_read  unit, cs, read_bytes
static bool script_spi_read(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = true;
	uint32_t value;
	uint32_t num_value = 0;



	Script_SPI_Read spi_read;


	char *str = strchr(pcmd,' ');

	//get unit number
	if (get_next_parameter( str,&value,1 ))
		spi_read.unit = (uint8_t)value;
	else return false;

	//get spi chip select
	if (get_next_parameter( NULL,&value,1 ))
		spi_read.cs = (uint8_t)value;
	else return false;
	//get read bytes
	if (get_next_parameter( NULL,&value,3 ))
		spi_read.bytes_to_read = (uint32_t)value;
	else return false;
	if (!spi_read.bytes_to_read || spi_read.bytes_to_read > MAX_DATA_BUFF_SIZE )
		return false;

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(SPI_Interface,spi_read.unit), SCRIPT_SPI_READ, (uint8_t*)&spi_read,sizeof(Script_SPI_Read));

	return ret;
}

//spi_writeandread  unit, cs, read_bytes, wv0,wv1,…wvn
static bool script_spi_writeandread(int8_t1 *pcmd,uint8_t* pscript_cmd_buff)
{
	bool ret = true;
	uint32_t value;
	uint32_t num_value = 0;

	uint8_t *ppay_load =(uint8_t*)malloc(32*1024);
	if (ppay_load == NULL)
		return false;


	Script_SPI_WriteAndRead *spi_writeandread =(Script_SPI_WriteAndRead *)ppay_load ;
	uint8_t *pwrite_data = (uint8_t*)&spi_writeandread->data_buffer; 


	char *str = strchr(pcmd,' ');

	//get unit number
	if (get_next_parameter( str,&value,1 ))
		spi_writeandread->unit = (uint8_t)value;
	else return false;

	//get spi chip select
	if (get_next_parameter( NULL,&value,1 ))
		spi_writeandread->cs = (uint8_t)value;
	else return false;

	//get spi chip select
	if (get_next_parameter( NULL,&value,3 ))
		spi_writeandread->bytes_to_read = (uint32_t)value;
	else return false;

	num_value = 0;
	while (1) {

		//initialize valure
		value = INVALID_VALUE;

		if (ret = get_next_parameter( NULL,&value,1 )) {
			*pwrite_data ++ = (uint8_t)value; 
			num_value ++;  
		} else {

			//check if the write value wrong
			if (value == INVALID_VALUE )
				spi_writeandread->bytes_to_write = num_value;
			else return false;

			break;       
		}

	}

	if (!spi_writeandread->bytes_to_write || spi_writeandread->bytes_to_write > MAX_DATA_BUFF_SIZE ||spi_writeandread->bytes_to_read > MAX_DATA_BUFF_SIZE 
		|| spi_writeandread->bytes_to_write < spi_writeandread->bytes_to_read )
		return false;

	ret = ocScript_AddCommand( pscript_cmd_buff, MAKE_IF(SPI_Interface,spi_writeandread->unit), SCRIPT_SPI_WRITEANDREAD, (uint8_t*)ppay_load, ((uint16_t)(sizeof(Script_SPI_WriteAndRead) -1) + num_value) );

	free(ppay_load);

	return ret;
}

bool processScript_SPI_funs(FUN_API_INDEX index,int8_t1 *pscript,uint8_t* pscript_cmd_buff)
{
	bool ret;
	switch (index) {
	
	case SPI_READ:
		ret = script_spi_read(pscript,pscript_cmd_buff);
		break;
	case SPI_WRITE:
		ret = script_spi_write(pscript,pscript_cmd_buff);
		break;
	case SPI_WRITEANDREAD:
		ret = script_spi_writeandread(pscript,pscript_cmd_buff);
		break;

	default:
		break;
	}

	return ret;

}
