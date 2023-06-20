//*****************************************************************************
//
// onectrl.h - Defines packet header.
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
//*****************************************************************************
#ifndef __PACKET_HEADER_H
#define __PACKET_HEADER_H


/* packet header signature*/
#define PACKET_SIGNATURE 0xD401  
#define PACKET_FIRST     1 
#define MAX_PKT_LEN      512

//system command
typedef enum{
	SYS_NOP_CMD = 2,
	SYS_RESET_CMD,
	SYS_ABORT_CMD,
	SYS_GETINFO_CMD,
	SYS_SETLED_CMD,

}SYS_COMMAND;


//packet type
typedef enum{
          COMMAND_PACKET,
          DATA_PACKET,
          STATUS_PACKET,
          INTERRUPT_PACKET,
          SCRIPT_DATA_PACKET,
          SCRIPT_STATUS_PACKET,
		  PACKET_TYPE_MAX
}PACKET_TYPE;


/* interface type definition*/
typedef  enum{
    IF_SYS = 0,  
	IF_MEM = 1,
	IF_PORT = 2,
	IF_I2C = 3,
	IF_GPIO = 4,
	IF_SPI = 5,
	IF_UART = 6,
	IF_ADC = 7,
	IF_DAC = 8,
	IF_SMBUS = 9,
	IF_RFFE = 0x0A,
	IF_PWM = 0x0B,
	IF_POWER = 0x0C,
	IF_FEC = 0x0D,
	IF_INT = 0x0E,
	IF_EASYSCALE = 0x0F,
	IF_SINGLE_WIRE = 0x10,
	IF_DIGIF = 0x11,
	IF_LED = 0x12,
	IF_STASUS = 0x13,
	IF_CLOCK = 0x14,
	IF_DISPLAY_SCALE = 0x15,
	IF_SCRIPT = 0x16,
	IF_PEGASUS = 0x17,
	IF_MAX
}IF_TYPE;

/* data struct definition*/
#ifdef _WIN32
#pragma pack(push, 1)
#endif
typedef struct
{
    uint16_t     Signature;     // Packet signature (always 0xD401)
    uint16_t     Type;          // Type of packet 
    uint16_t     Transfer_Len; // Total number of bytes in the transfer
    uint16_t     Packet_Num;    // Sequential packet number
    int16_t      Status;    // Status code returned by command function
    uint16_t     Payload_Len;   // Number of bytes in the payload
    uint16_t     If_type_Unit;  // Interface type and unit
    uint16_t     Command;       // Command code
    uint32_t     Param[8];      // eight 32-bit parameters
} PACKET_HEADER;
#ifdef _WIN32
#pragma pack(pop)
#endif


#endif
