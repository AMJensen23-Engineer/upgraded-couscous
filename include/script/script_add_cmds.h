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

#ifndef __SCRIPT_ADD_CMDS_H
#define __SCRIPT_ADD_CMDS_H

//script fun type in script parser
typedef enum {
	 SET_RETURN_DATA_MODE =0,
	 DELAY ,
	 GPIO_READ,
	 GPIO_WRITE, 
	 I2C_READ,
	 I2C_WRITE,
	 SPI_READ,
	 SPI_WRITE,
	 SPI_WRITEANDREAD,
	 UART_READ,
	 UART_WRITE,
     GPIO_STATUS, 
	 INVALUD_FUN
}FUN_API_INDEX;

//llop and if type
typedef enum {
	SCRIPT_LOOP_TYPE,
	SCRIPT_IF_TYPE,
	SCRIPT_ELSE_TYPE
 
}SCRIPT_COND_TYPE;

//loop command type
typedef enum {
	 LOOP_START,
	 LOOP_BREAK, 
	 LOOP_END
}LOOP_OPERATOR;

//if/else command type
typedef enum {
	 IF_START = 0,
	 ELSE_START,
	 IF_ELSE_END
}COND_OPERATOR;

//condition and loop type in the stack
typedef enum
{
  B_IF_START,
  B_ELSE_START,
  B_LOOP_START,
  B_LOOP_BREAK,
  B_INVALID
}BRACE_TYPE;

//add command header
bool ocScript_AddHeader(int8_t1 * pcmd,uint8_t* pscript_cmd_buff );

uint8_t *ocScript_GetCurCommandBuff(uint8_t* pscript_cmd_buff);
//add command
bool ocScript_AddCommand(uint8_t* pscript_cmd_buff, uint16_t if_type_uint, uint16_t script_command, uint8_t *ppayload, uint16_t payload_size );

#endif