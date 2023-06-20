//*****************************************************************************
//
// oc_api.cpp - implement onecontrol API.
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
//
// Change History
//
//+ 2015-11-10 [RT] - Moved HANDLE_CHECK() macro to onectrl.h
//+ 2015-11-10 [RT] - Moved API functions to ocAPI_System.cpp
//

#include <string.h>
#include <stdlib.h>

#include "acctrl.h"


//#pragma warning (disable:4996)

/* global variable  */
 //OC_SYS  *g_psOC_Sys[MAX_SUPPORT_OC];
 SYS_Manager g_Sys_Manager;

 /*get status functions*/
#if 0 // unused
 void ONECTRL_API WIN_API ocSys_GetFirmwareLastError(int32_t Handle, int8_t1* ErrStr, uint16_t Len)
 {
	 if((Handle>0 || Handle < MAX_SUPPORT_OC) && ErrStr && strlen(g_LastFirmwareErrStr[Handle]) < Len)
	 {
	   strcpy(ErrStr, g_LastFirmwareErrStr[Handle]);
	 }
 }
#endif

 
 
 /**/
 
 

 /*globa functions*/
 /* MOVED to ocAPI_System.cpp */
#if 0
int32_t ONECTRL_API WIN_API ocSys_FindOneControllers()
{
    int32_t onectrl_num = oc_OSCDC_FindOneControllers(); 
	DbgWriteText("ocSys_FindOneControllers: Find %d OneCOntroller \r\n",onectrl_num);
	return onectrl_num;
}

int32_t ONECTRL_API WIN_API ocSys_GetComPortNumber(uint8_t OneControllerIndex)
{
 return oc_OSCDC_GetComPortNumber(OneControllerIndex);  
}

int32_t ONECTRL_API WIN_API ocSys_Open(uint8_t ComPortNum)
{
  return oc_OSCDC_Open(ComPortNum);
}

int32_t ONECTRL_API WIN_API ocSys_Close(int32_t Handle)
{
	   
	HANDLE_CHECK(Handle);

	
	
	oc_OSCDC_Close(Handle);
   //	g_Sys_Manager.ocSys_UnInitOneController(Handle);
   	
	return 0;
}
#endif

acAPI acPacket_WriteFlush(int32_t Handle)
{
    ac_OSCDC_FlushWrite(Handle);
	return 0;
}

acAPI acPacket_Write(int32_t Handle, uint8_t *pData, uint32_t  DataSize)
{
	int32_t write_bytes; 

	HANDLE_CHECK(Handle);
	
	uint32_t write_data_len = DataSize;
	
	OCF_PACKET_HEADER *pspktHeader =(OCF_PACKET_HEADER *)pData;
	OCF_PACKET_HEADER *pNext_pktHeader;

	uint8_t header_len  = sizeof(OCF_PACKET_HEADER);
	uint16_t max_payload = MAX_PAYLOAD;  

	if(!pData)
	{
	    DbgWriteText("ocPacket_Write: write buffer pointer is null\r\n");
		return 0;
	}

	g_Sys_Manager.ocSysM_GetSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Write_Semaphore,1000);
	//one packet 
	if(  DataSize <= MAX_PKT_LEN)
	  { 
		  write_bytes=ac_OSCDC_Write(g_Sys_Manager.psOC_Sys[Handle]->Com_Handle, pData,  DataSize);
	       if(write_bytes != DataSize)
			 {
			        DbgWriteText("ocPacket_Write:write data failure: send size %d, real send size %d \r\n",DataSize,write_bytes);
				    g_Sys_Manager.ocSysM_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Write_Semaphore);
					return  write_bytes;
			  }
	   }
	 else //big data
	 {
		 if(pspktHeader->transfer_len > max_payload)
		 {
		     //int pkt_count = DataSize/MAX_PKT_LEN;
			 int send_len;
			
			 uint8_t*next_Pkt =(uint8_t*) pData;
			 pspktHeader->packet_num = 1;
			 pspktHeader->payload_len = max_payload;
			 pNext_pktHeader = pspktHeader; 
			 
			 //real data length
			 write_data_len -= header_len;  
			 
			 do{
             
				 if(write_data_len > max_payload)
				 
					 pNext_pktHeader->payload_len = max_payload;
			     
				 else pNext_pktHeader->payload_len = write_data_len;
			   
			     write_data_len  -= pNext_pktHeader->payload_len; 

			     send_len = pNext_pktHeader->payload_len + header_len;
            
			     write_bytes = ac_OSCDC_Write(g_Sys_Manager.psOC_Sys[Handle]->Com_Handle, next_Pkt,  send_len);
			     
				 if(write_bytes != send_len)
			     {
			        DbgWriteText("ocPacket_Write: write data failure: send size %d, real send size %d \r\n",send_len,write_bytes);
				    g_Sys_Manager.ocSysM_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Write_Semaphore);
					return 0;
			     }
			  
				 
				 if(write_data_len == 0)
				    break;
				 ac_OS_Sleep(2);
				 //get next data buffer pointer
			     next_Pkt += pNext_pktHeader->payload_len;
			     pNext_pktHeader =(OCF_PACKET_HEADER*)next_Pkt; 
				 
				 //add packet index number
				 pspktHeader->packet_num ++;
				 memcpy(pNext_pktHeader, pspktHeader, header_len);

			 }while(1);

		 
		 } 
		 else
            DbgWriteText("WritePacket error:don't set transfer_len\r\n");
	   
	 }

	 g_Sys_Manager.ocSysM_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Write_Semaphore);

	 return DataSize;
 }

static int32_t acPacket_RegsterIfReadQueue(int32_t Handle, uint16_t  IF)
{
   
	 HANDLE_CHECK(Handle);

	if(g_Sys_Manager.psOC_Sys[Handle]->prcv_queue->oc_Find_Rcv_Queue(IF))
		return 0;
	
	 DbgWriteText("ocPacket_RegsterIfReadQueue: create receiving queue fail\r\n");
       
	
	return -1;

	
}
static int32_t acPacket_UnRegsterIfReadQueue(int32_t Handle, uint16_t  IF)
{
   

     HANDLE_CHECK(Handle);

	 uint8_t if_type = (IF >> 8) & 0xFF;
	 uint8_t uint = (IF & 0xFF);

	RCV_QUEUE *rcv =g_Sys_Manager.psOC_Sys[Handle]->prcv_queue->oc_Find_Rcv_Queue(IF);
	if(rcv)
	{
		g_Sys_Manager.psOC_Sys[Handle]->prcv_queue->oc_Release_Rcv_Queue(rcv);
		free( rcv);
		g_Sys_Manager.psOC_Sys[Handle]->Rcv[if_type][uint] = NULL;
		return 0;
	}
	return -1;
}
acAPI acPacket_GetIFStatus(int32_t  Handle, uint16_t IF, STATUS *pStatus)
{
    HANDLE_CHECK(Handle);

    if(!pStatus)
    {
        DbgWriteText("ocPacket_GetIFStatus: read buffer pointer is null\r\n");
        return OC_ERR_OPERATION_FAILED;
    }

    RCV_QUEUE *rcv = g_Sys_Manager.psOC_Sys[Handle]->prcv_queue->oc_Find_Rcv_Queue(IF);

    if(rcv && rcv->State == QUEUE_STATE_FILLED  )
    {
        *pStatus = rcv->Status;
        return STAT_OK;
    }

    return OC_ERR_SYS_UNKNOWN_STATUS;
}

acAPI acPacket_ResetIFStatus(int32_t  Handle, uint16_t IF)
{
	HANDLE_CHECK(Handle);

	RCV_QUEUE *rcv = g_Sys_Manager.psOC_Sys[Handle]->prcv_queue->oc_Find_Rcv_Queue(IF);

	if(rcv)
	{
		 rcv->State = QUEUE_STATE_IDLE;
		return 0;
	}
	return -1;
}

acAPI acPacket_ReadSync(int32_t  Handle, uint16_t IF, uint8_t *pData, uint32_t DataSize, uint16_t Timeout)
{
	
	HANDLE_CHECK(Handle);

	
	if(!pData)
	{
	    DbgWriteText("ocPacket_ReadSync: read buffer pointer is null\r\n");
		return 0;
	}

	RCV_QUEUE *rcv =g_Sys_Manager.psOC_Sys[Handle]->prcv_queue->oc_Find_Rcv_Queue(IF);
/*** check for error & handle as necessary ***/

	//ASSERT(pData);
	  uint32_t  read_bytes = 0;
	
    
	if(rcv)
	{
        read_bytes =DataSize;
		
		//no data, 
    	if(rcv->State == QUEUE_STATE_IDLE || rcv->DataSize == 0 )
	          {
				  int count = Timeout + 1;
				  for(uint16_t i =0; i < count; i++)
				   {
					   ac_OS_Sleep(1);	 
				       if(rcv->State == QUEUE_STATE_FILLED && rcv->DataSize )
				           break;
				   } 
		       }
		 
		//still no data
		if(rcv->State == QUEUE_STATE_IDLE || rcv->DataSize == 0 )
	        {
			    DbgWriteText("ocPacket_ReadSync: Timeout\r\n");
				return 0;
		     }
		if(  rcv->DataSize < DataSize )
		    read_bytes = rcv->DataSize;
		
		g_Sys_Manager.ocSysM_GetSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Read_Semaphore,1000);
	    		//copy data to user buffer
		if((rcv->pData + rcv->ReadIndex + read_bytes) <= (rcv->pData + rcv->CurBuffSize)) 
		   memcpy(pData, (rcv->pData + rcv->ReadIndex),read_bytes);  
		else {
		    g_Sys_Manager.ocSysM_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Read_Semaphore); 
			return 0;
		}
		 rcv->ReadIndex += read_bytes;
		 rcv->DataSize -= read_bytes;
		 
		 if(rcv->DataSize == 0)
		   rcv->State = QUEUE_STATE_IDLE;
		 
		 //clear callback routine 
		 rcv->pfDataReadyCallBack = NULL;
		 g_Sys_Manager.ocSysM_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Read_Semaphore);
	} 
	
	 // DbgWriteText("ocPacket_ReadSync: Read size %d, real Read size %d \r\n",DataSize,read_bytes);
				  
	return read_bytes ;
}


acAPI acPacket_ReadASync(int32_t Handle, uint16_t IF, uint8_t* pData, uint32_t DataSize,  void (*DataReadyCallback)(uint8_t *pData, uint16_t  DataSize))
{
	
	HANDLE_CHECK(Handle);
	
	
	
	if(!pData)
	{
	    DbgWriteText("ocPacket_ReadASync: read buffer pointer is null\r\n");
		return 0;
	}

	RCV_QUEUE *rcv = g_Sys_Manager.psOC_Sys[Handle]->prcv_queue->oc_Find_Rcv_Queue(IF);
	
	uint32_t  read_bytes = 0;
	
	
	if(rcv == NULL)
	{
	    DbgWriteText("ocPacket_ReadASync: don't find receive queue\r\n");
		return 0;
	}
	    //uint32_t read_bytes =DataSize;
				
		if( rcv->DataSize >= DataSize && rcv->State == QUEUE_STATE_FILLED )
		 {
		 		
			read_bytes = rcv->DataSize;
		   g_Sys_Manager.ocSysM_GetSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Read_Semaphore,1000);
	     	
		   //copy data to user buffer
			if((rcv->pData + rcv->ReadIndex + read_bytes) <= (rcv->pData + rcv->CurBuffSize))
			  memcpy(pData, (rcv->pData + rcv->ReadIndex),read_bytes);  
		    else {
		          //should be here  
				  g_Sys_Manager.ocSysM_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Read_Semaphore); 
			      return 0;
		         }
		    
			rcv->ReadIndex += read_bytes;
		    rcv->DataSize -= read_bytes;
			if(rcv->DataSize == 0)
				rcv->State = QUEUE_STATE_IDLE;

		    g_Sys_Manager.ocSysM_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[Handle]->Read_Semaphore);
	     }else
		 {
			 rcv->pfDataReadyCallBack = DataReadyCallback;
			 rcv->pCallbackBuff = pData;
			 rcv->CallbackDataSize =  DataSize;
		 }
	

	return read_bytes ;
	
	
}




acAPI acInitIF(int32_t Handle, uint8_t IF_TYPE, uint8_t Uint)
{
    
	return acPacket_RegsterIfReadQueue(Handle, ((IF_TYPE<<8)|Uint));
	
	
}

acAPI acUnInitIF(int32_t Handle, uint8_t IF_TYPE, uint8_t Uint)
{
    
	acPacket_UnRegsterIfReadQueue(Handle, ((IF_TYPE<<8)|Uint));

	return 0;

}


#if 0 // RT 01-07-16: moved ocSys_GetOneControllerInfo() to ocAPI_System.cpp
int32_t ONECTRL_API WIN_API ocSys_GetOneControllerInfo(int32_t Handle, ONECONTROLLERINFO *psInfo)
{
  
	int read_headr = sizeof(OCF_PACKET_HEADER); 
    int32_t read_bytes;
	OCF_PACKET_HEADER spktHeader;

	spktHeader.signature = PACKET_SIGNATURE;
	spktHeader.type = COMMAND_PACKET;
	spktHeader.if_type_unit = IF_SYS;  
	spktHeader.command = ocCmd_Sys_GetInfo;
 	ocPacket_Write(Handle,(uint8_t*)&spktHeader,read_headr);
	read_bytes = ocPacket_ReadSync(Handle, (IF_SYS<<8), (uint8_t*)psInfo, sizeof(ONECONTROLLERINFO),100);
    if(read_bytes == sizeof(ONECONTROLLERINFO))
	return 0;

	DbgWriteText("ocSys_GetOneControllerInfo: Cannot get the right info from firmware\r\n");
	return -1;
}
#endif


#if 0

System Commands
ocGetOneControllerInfo
ocSetLogFilePath
ocEnableDebugLogging
ocRegiterSysEventNotify
ocAbortCurCommand
ocResetOneController
ocSetBoardLED
ocReadOneControllerStatus
ocUpdateFirmware
osInitIF
Packet Read/ Write command
ocWritePacket
ocReadPacketSync
ocReadPacketAsync
ocRegsterIfReadQueue
OS dependent I/O function(internal commands)
oc_OSCDC_FindOneControllers
 oc_OSCDC_GetComPortNumber
oc_OSCDC_Open
oc_OSCDC_Close
Oc_OSCDC_RegsterIfReadQueue
oc_OSCDC_ReadAsync
oc_OSCDC_ReadSync
oc_OSCDC_Write
SPI Commands
osSPI_Enable
ocSPI_Config
ocSPI_Write
ocSPI_ReadSync
ocSPI_ReadAsync
Stream API
ocStream_Config
ocStream_GetQueueStatus
ocStream_Start
ocStream_ReadData
ocStream_Stop
Power Commands
ocEnableOutputPower
ocGetOutputPowerStatus
#endif

#if 0
// This is an example of an exported function.
ocAPI ocPower_EnableOutput(int32_t Handle, uint8_t OutputP1, uint8_t Enable5v0)
{
	int read_headr = sizeof(OCF_PACKET_HEADER); 
    int32_t write_bytes;
	OCF_PACKET_HEADER spktHeader;

	spktHeader.signature = PACKET_SIGNATURE;
	spktHeader.type = COMMAND_PACKET;
	spktHeader.if_type_unit = IF_POWER;  
	spktHeader.command = 0;
	
 	write_bytes = ocPacket_Write(Handle,(uint8_t*)&spktHeader,read_headr);
	//read_bytes = ocPacket_ReadSync(Handle, (IF_SYS<<8), (uint8_t*)psInfo, sizeof(ONECONTROLLERINFO),100);
	return write_bytes;
}

ocAPI ocPower_GetOutputStatus(int32_t Handle, uint8_t*p1, uint8_t *p5v)
{
	int read_headr = sizeof(OCF_PACKET_HEADER); 
    int32_t write_bytes,read_bytes;
	OCF_PACKET_HEADER spktHeader;
	uint8_t pstatus[2];
	spktHeader.signature = PACKET_SIGNATURE;
	spktHeader.type = COMMAND_PACKET;
	spktHeader.if_type_unit = IF_POWER;  
	spktHeader.command = 1;
	
 	write_bytes = ocPacket_Write(Handle,(uint8_t*)&spktHeader,read_headr);
	//read_bytes = ocPacket_ReadSync(Handle, (IF_SYS<<8), (uint8_t*)psInfo, sizeof(ONECONTROLLERINFO),100);
	read_bytes = ocPacket_ReadSync(Handle, (IF_SYS<<8), (uint8_t*)pstatus, 2,100);
    if(read_bytes == 2)
	{
	  *p1 = pstatus[0];
	  *p5v = pstatus[1];

	}
	return read_bytes;
}
#endif


/**********************************************************
 * ocWaitForStatus
 *
 * Waits up to timeout milliseconds for a status packet to be 
 * received by the specified interface. 
 * 
 * If a status packet is returned before the timeout expires, 
 * the status code is returned; otherwise, OC_ERR_TIMEOUT is
 * returned.
 */
STATUS acWaitForStatus(AC_HANDLE handle, uint16_t IF, int32_t timeout)
{
    STATUS status = OC_ERR_SYS_UNKNOWN_STATUS;

    while (timeout > 0)
    {
        if (STAT_OK == acPacket_GetIFStatus(handle, IF, &status))
        {
            break;
        }

        ac_OS_Sleep(1);
        timeout -= 1;
    }

	
    if( timeout <= 0)
    {
        //time out, flush the write buffer 
        acPacket_WriteFlush(handle);
    }

    return (timeout > 0) ? status : OC_ERR_TIMEOUT;
}

/**********************************************************
 * ocGetReceivedData
 *
 * Receive data from a specific interface and unit. 
 * 
 * If data is ready within the specified timeout period, the 
 * data is copied to the specified buffer (pData) and the
 * number of bytes received is returned.
 *
 * If an error occurs, the error code is returned.
 */
STATUS acGetReceivedData(AC_HANDLE handle, uint8_t intf, uint8_t unit, uint8_t *pData, 
                            uint16_t bytes_to_read, uint8_t timeout)
{
    STATUS status;
    uint32_t count = acPacket_ReadSync(handle, MAKE_IF(intf, unit), pData, bytes_to_read, timeout);

    if (count == 0)
    {
        status = acWaitForStatus(handle, MAKE_IF(intf, unit), 255);
    }
    else
    {
        status = count;
    }

    return status;
}

