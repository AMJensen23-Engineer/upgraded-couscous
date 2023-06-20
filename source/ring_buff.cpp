//*****************************************************************************
//
// ring_buff.cpp - implement ring buffer management.
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
#include <stdio.h>
#include <string.h> 
#include <malloc.h>

#include "acctrl.h"
#include "ring_buff.h"

//we support maximum 5 stream;
//#define MAX_STREAM_NUM    5
//we support maximum 5  OneController in the sam PC.
OC_RCV_RING_BUFF oc_rcv_ring_buff[SUPPORT_MAX_OC];

RCV_RINGBUFF *ocRingBuff_Find(int32_t oc_handle, uint16_t scriptid)
{
	RCV_RINGBUFF *rcv_buff =(RCV_RINGBUFF*) NULL;

	if( ((scriptid>>8) & 0xFF) == IF_SCRIPT) 
	{
	  if( scriptid && 0xFF < SUPPORT_MAX_OC)
		  {
			   // make sure the ring buffer is initialzed
			  for(int ring_buff_index = 0; ring_buff_index < SUPPORT_MAX_OC; ring_buff_index++)
				{
				  if(oc_rcv_ring_buff[ring_buff_index].oc_handle == oc_handle)
				  {
				  
				  }
			  }
			  //if(oc_rcv_ring_buff[IF && 0xFF].BuffState == RING_BUFF_FILLED)
			    // return &ring_buff[IF && 0xFF];
	      }
	} 
	return rcv_buff;
}

void ocRingBuf_Init(RCV_RINGBUFF *psRingBuf, uint16_t ScriptID, uint32_t BuffSize)
{
	//
    // Check the arguments.
    //
    ASSERT(psRingBuf);
    ASSERT(pBuf) ;
    ASSERT(BuffSize);
    
	return;
	memset(psRingBuf, 0, sizeof(RCV_RINGBUFF)); 
	psRingBuf->pBuff_Start =(uint8_t*) malloc(BuffSize);
	if(psRingBuf)
	{
	
		psRingBuf->pBuff_End = psRingBuf->pBuff_Start + BuffSize;
		psRingBuf->pBuff_Read = psRingBuf->pBuff_Start;
		psRingBuf->pBuff_Write = psRingBuf->pBuff_Start;
		psRingBuf->BuffSize = BuffSize;
		psRingBuf->DataSize = 0;
		psRingBuf->BuffState = RING_BUFF_FILLED;
		psRingBuf->ScriptID = ScriptID;
	}
#if 0

    //
    // Initialize the ring buffer object.
    //
    psUSBRingBuf->ui32Size = ui32Size;
    psUSBRingBuf->pui8Buf = pui8Buf;
    psUSBRingBuf->ui32WriteIndex = psUSBRingBuf->ui32ReadIndex = 0;
#endif
}

uint32_t ocRingBuf_GetContigSpace(RCV_RINGBUFF *psRingBuf, uint32_t DataSize)
{
    uint32_t ava_space;
    ava_space =(uint32_t)(psRingBuf->pBuff_End - psRingBuf->pBuff_Write) ;
    
	if((psRingBuf->pBuff_Read > psRingBuf->pBuff_Write) && ((psRingBuf->pBuff_Write + DataSize) > psRingBuf->pBuff_Read))
		{
		    
			psRingBuf->BuffState = RING_BUFF_OVERFLOW;
	   } 
	
	if(ava_space > DataSize)
	     ava_space = DataSize;

	return ava_space;
}


void ocRingBuf_Write(RCV_RINGBUFF *psRingBuf, const uint8_t *pData,
                uint32_t DataSize)
{
    
#if 0
	uint32_t ui32Temp;

    //
    // Check the arguments.
    //
    ASSERT(psUSBRingBuf != NULL);
    ASSERT(pui8Data != NULL);
    ASSERT(ui32Length != 0);

    //
    // Verify that space is available in the buffer.
    //
    ASSERT(ui32Length <= USBRingBufFree(psUSBRingBuf));

    //
    // Write the data into the ring buffer.
    //
    for(ui32Temp = 0; ui32Temp < ui32Length; ui32Temp++)
    {
        USBRingBufWriteOne(psUSBRingBuf, pui8Data[ui32Temp]);
    }
#endif
}

void ocRingBuf_WriteAdvance(RCV_RINGBUFF *psRingBuf, const uint8_t *pData,
                uint32_t DataSize)
{

}