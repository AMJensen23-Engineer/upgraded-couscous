//*****************************************************************************
//
// ring_buff.h - implement ring buffer management .
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
#ifndef __RING_BUFF_H
#define __RING_BUFF_H

/* ring buffer status */
#define SUPPORT_MAX_OC     5

typedef  enum  _RING_BUFF_STATE{
             RING_BUFF_EMPTY,  // buffer not intialized 
             RING_BUFF_FILLED, // buffer intialized and read to receive data
             RING_BUFF_OVERFLOW
        } RING_BUFF_STATE;

typedef  enum  _SCRIPT_RUNNING_STATE {
             SCRIPT_IS_IDEL,
             SCRIPT_IS_RUNNING,
             SCRIPT_IS_STOP
        } SCRIPT_RUNNING_STATE;

typedef struct 
{
     RING_BUFF_STATE BuffState; 
     SCRIPT_RUNNING_STATE  RuningState; 
     uint16_t   status;  //script running status  
	 uint8_t   *pBuff_Start;  //the data buffer start pointer 
     uint8_t   *pBuff_End;  //the data buffer end pointer 
     uint8_t   *pBuff_Write;  //the data buffer write pointer 
     uint8_t   *pBuff_Read;  //the data buffer read pointer 
     uint32_t  BuffSize; //the buffer size
     uint32_t  DataSize; //the data size in the buffer
     uint16_t ScriptID;//the pattern ID returned by  ocLoad_SXCript  
 }RCV_RINGBUFF; 

#define OC_MAX_RCV_BUFF_NUM   5 

typedef struct{

 int32_t oc_handle;
 RCV_RINGBUFF rcv_buff[OC_MAX_RCV_BUFF_NUM]; 
 bool    isUsed;
}OC_RCV_RING_BUFF;



/* */
/*function type*/
extern OC_RCV_RING_BUFF oc_rcv_ring_buff[SUPPORT_MAX_OC];
void ocRingBuf_Init(RCV_RINGBUFF *psRingBuf, uint8_t *pBuf,uint32_t BuffSize);

RCV_RINGBUFF *ocRingBuff_Find(int32_t oc_handle, uint16_t scriptid);

#endif