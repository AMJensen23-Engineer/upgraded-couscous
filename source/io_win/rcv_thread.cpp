//*****************************************************************************
//
// rcv_thread.cpp - thread to receive data from the OneController firmware.
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
#include <windows.h>
#include <stdio.h>


#include "acctrl.h"
#include "ring_buff.h"
#include "rcv_thread.h"
#include "rcv_queue.h"

/*** this should really be an inline function ***/

#define Clean_Com_RCV_Buff   { PurgeComm((HANDLE)Com_Handle, PURGE_RXCLEAR);  DbgWriteText("somthing wrong, clear the receive buffer\r\n"); continue; }

/* receive queue*/


 void Init_Com(int32_t handle)
{
	 int result;	
	 DWORD dwEvtMask;
	 COMMTIMEOUTS CommTimeouts;
	 CommTimeouts.ReadTotalTimeoutConstant =10;
	 CommTimeouts.ReadTotalTimeoutMultiplier = 0;
	 CommTimeouts.ReadIntervalTimeout =0;
	 CommTimeouts.WriteTotalTimeoutMultiplier = 10;
	 dwEvtMask = EV_RXCHAR;
	 PurgeComm((HANDLE)handle, PURGE_RXCLEAR|PURGE_TXCLEAR|PURGE_RXABORT|PURGE_TXABORT);
	 result = SetupComm((HANDLE)handle, 64*1024, 10*1024);
/*** ERROR CHECK - next 3 lines***/
	 SetCommMask((HANDLE) handle,   dwEvtMask);
	 SetCommTimeouts((HANDLE)handle, &CommTimeouts);
	 PurgeComm((HANDLE)handle, PURGE_RXCLEAR|PURGE_TXCLEAR|PURGE_RXABORT|PURGE_TXABORT);
	

	 
}

#if 0
int32_t  ReadData(int32_t oc_handle,HANDLE Com_Handle)
{
   int read_headr = sizeof(OCF_PACKET_HEADER); 
    DWORD read_bytes;
	uint32_t buff_length;
	OCF_PACKET_HEADER spktHeader;
	DWORD dwEvtMask;
	 OVERLAPPED o;
	RCV_QUEUE *prcv_queue;
	
	  ReadFile(Com_Handle,&spktHeader,(DWORD)read_headr,(DWORD*)&read_bytes,NULL); 
	 
	  if(spktHeader.signature != PACKET_SIGNATURE)
	   {
	      Clean_Com_RCV_Buff
		  //return;
	    }
	 	  switch(spktHeader.type)
		  {
		  
		  case STATUS_PACKET:
			  
			     prcv_queue = oc_Find_Rcv_Queue(oc_handle,spktHeader.if_type_unit);
			     if(!prcv_queue)
				  {
				   Clean_Com_RCV_Buff
				   break;
				   }

				  prcv_queue->Status = spktHeader.Status;
				  prcv_queue->Command = spktHeader.command;
				  prcv_queue->State = QUEUE_STATE_FILLED;
				  
				  //there is payload
				  if(spktHeader.payload_len)
				  {
				    ReadFile(Com_Handle,prcv_queue->ErrorStr,(DWORD)spktHeader.payload_len,(DWORD*)&read_bytes,NULL); 
	                if( spktHeader.payload_len != read_bytes)
					   Clean_Com_RCV_Buff
                   //keep the last error string
                   memcpy( g_LastFirmwareErrStr[oc_handle],prcv_queue->ErrorStr,read_bytes);
				  }
			  
			    break;
		  case INTERRUPT_PACKET:
			   break;
		  case DATA_PACKET:
			
			   buff_length = spktHeader.payload_len;
		        
			   if(spktHeader.packet_num == PACKET_FIRST )
		       {
				 // there is a payload
		          if(spktHeader.transfer_len)
				    buff_length = spktHeader.transfer_len;
				
			       prcv_queue = g_psOC_Sys[oc_handle]->rcv_queue->oc_Find_Rcv_Queue(spktHeader.if_type_unit);
		   
				  if(prcv_queue)
		           {
		             if(!g_psOC_Sys[oc_handle]->rcv_queue->oc_Alloc_Rcv_Queue(prcv_queue,buff_length))
					   Clean_Com_RCV_Buff
				   
				   }else
				    Clean_Com_RCV_Buff
				  
				
			     }//end first pack check
			   
			   ReadFile(Com_Handle,(prcv_queue->pData + prcv_queue->WriteIndex),(DWORD)spktHeader.payload_len,(DWORD*)&read_bytes,NULL); 
	           if(spktHeader.payload_len == read_bytes)
			   {
				   //need to protect by smphone
				   if(!ocSys_GetSemaphore(g_psOC_Sys[oc_handle]->Read_Semaphore, 10000 ))
					       Clean_Com_RCV_Buff

				   
					   prcv_queue->WriteIndex += read_bytes; 
				       prcv_queue->DataSize += read_bytes;
				       prcv_queue->State = QUEUE_STATE_FILLED;
			   	       ocSys_ReleaseSemaphore(g_psOC_Sys[oc_handle]->Read_Semaphore ); 
				   
					
			   }else
			   {
			      Clean_Com_RCV_Buff
			   }
			     
			   if(prcv_queue->pfDataReadyCallBack && prcv_queue->pCallbackBuff)
			   {
				   if(ocSys_GetSemaphore(g_psOC_Sys[oc_handle]->Read_Semaphore, 1000)) 
                      Clean_Com_RCV_Buff

				   uint32_t sent_datasize = prcv_queue->CallbackDataSize; 				   
				   
				   if(prcv_queue->DataSize < prcv_queue->CallbackDataSize)
					   sent_datasize = prcv_queue->DataSize;
				  
				   memcpy(prcv_queue->pCallbackBuff,prcv_queue->pData, sent_datasize);  
				   prcv_queue->pfDataReadyCallBack(prcv_queue->pCallbackBuff,sent_datasize);
				   
				   prcv_queue->ReadIndex += prcv_queue->DataSize; 
				   prcv_queue->DataSize -= sent_datasize;
				   
				   if(prcv_queue->DataSize == 0)
				    prcv_queue->State = QUEUE_STATE_IDLE;
				   
				   //callback must be reset by every call
				   prcv_queue->pfDataReadyCallBack = NULL;
			   	   ocSys_ReleaseSemaphore(g_psOC_Sys[oc_handle]->Read_Semaphore ); 
			   }
			  break;
		  case SCRIPT_DATA_PACKET:
          case SCRIPT_STATUS_PACKET:
			    break;
		  default:
			   Clean_Com_RCV_Buff
					 
			  break;
		  
		   
		  
		  }

 return 0;
}
#endif
#if 1

/*** return type should be OC_STATUS ***/
void send_Status(int32_t oc_handle)
{
	int read_headr = sizeof(OCF_PACKET_HEADER); 
    DWORD write_bytes;

	OCF_PACKET_HEADER spktHeader;

	spktHeader.signature = PACKET_SIGNATURE;
	spktHeader.type = STATUS_PACKET;
	spktHeader.if_type_unit =(IF_I2C<<8);  
	spktHeader.packet_num =1 ;
	write_bytes =acPacket_Write(oc_handle,(uint8_t*)&spktHeader,read_headr);
/*** should return status ***/
}

/*** return type should be OC_STATUS ***/
void send_Data(int32_t oc_handle)
{
  int read_headr = sizeof(OCF_PACKET_HEADER); 
	uint8_t buff[512];
    OCF_PACKET_HEADER *spktHeader=(OCF_PACKET_HEADER *)buff ;

	spktHeader->signature = PACKET_SIGNATURE;
	spktHeader->type = COMMAND_PACKET;
	spktHeader->if_type_unit =(IF_I2C<<8);  
	spktHeader->packet_num =1 ;
	spktHeader->payload_len = 512- sizeof(OCF_PACKET_HEADER);
	spktHeader->transfer_len =0;
	spktHeader->param[0]=6;
	//char buff[215];


	int i =acPacket_Write(oc_handle,buff,512);
/*** should return status ***/
}

/*** return type should be OC_STATUS ***/
void ask_Data(int32_t oc_handle)
{
   int read_headr = sizeof(OCF_PACKET_HEADER); 
	uint8_t buff[512];
    OCF_PACKET_HEADER *spktHeader=(OCF_PACKET_HEADER *)buff ;

	spktHeader->signature = PACKET_SIGNATURE;
	spktHeader->type = COMMAND_PACKET;
	spktHeader->if_type_unit =(IF_I2C<<8);  
	spktHeader->packet_num =1 ;
	spktHeader->payload_len = 512- sizeof(OCF_PACKET_HEADER);
	spktHeader->transfer_len =0;
	spktHeader->param[0]=5096;
	int i =acPacket_Write(oc_handle,buff,512);
/*** should return status ***/
}

#endif

#define THROUGHPUT_TEST 
void startTime();

double GetElapsedTime();

void ac_Rcv_Thread(  void* lpParam)
{
  
	OC_SYS  *ponectrl;
	// get the OneController handle
	//int32_t oc_handle =*((int32_t*)lpParam);
	
	ponectrl = ((OC_SYS*)lpParam);
	//get the OneCOntroller Com handle
	int32_t Com_Handle =(int32_t)ponectrl->pm_onectrl->onectrl_Get_OpenComDupHandle();
/*** check for error & handle as necessary ***/
    
	int read_header = sizeof(OCF_PACKET_HEADER); 
    DWORD read_bytes;
	uint32_t buff_length;
	OCF_PACKET_HEADER spktHeader;
	
	RCV_QUEUE *prcv_queue;
	
	//initalize virtual Com port
	Init_Com(Com_Handle);
/*** ERROR CHECK! -- fatal error if failure ***/

	
 
	 SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_ABOVE_NORMAL);
   

	//send_Data(ponectrl->oc_handle);
	

	while(ponectrl->pm_onectrl->onectrl_Get_OCOpen())
	{
	
	
	 // send_Data(ponectrl->oc_handle);
	  
	 
	 ReadFile((HANDLE)Com_Handle,&spktHeader,(DWORD)read_header,(DWORD*)&read_bytes,NULL); 
/*** check for error & handle as necessary ***/
	 if(read_bytes != read_header)
	 {
		// DWORD dwEvtMask;
	   //WaitCommEvent(Com_Handle,&dwEvtMask,NULL);
	     Sleep(1);
		 continue;
	 }	  
	  
	  
	  if(spktHeader.signature != PACKET_SIGNATURE)
	   {
	      Clean_Com_RCV_Buff
		  continue;
	    }
	 	  switch(spktHeader.type)
		  {
		  
		  case STATUS_PACKET:
			  
			     prcv_queue = ponectrl->prcv_queue->oc_Find_Rcv_Queue(spktHeader.if_type_unit);
			     if(!prcv_queue)
				  {
				   Clean_Com_RCV_Buff
				   break;
				   }

				  prcv_queue->Status = spktHeader.status;
				  prcv_queue->Command = spktHeader.command;
				  prcv_queue->State = QUEUE_STATE_FILLED;
				  
				  //there is payload
				  if(spktHeader.payload_len &&  spktHeader.payload_len < MAX_STATUS_STR_LEN )
				  {
				    ReadFile((HANDLE)Com_Handle,prcv_queue->ErrorStr,(DWORD)spktHeader.payload_len,(DWORD*)&read_bytes,NULL); 
	                if( spktHeader.payload_len != read_bytes)
					   Clean_Com_RCV_Buff

                   //keep the last error string
                   if(read_bytes <  MAX_ERRROR_STR_LEN )
				      {
                   	memcpy( g_LastFirmwareErrStr[ponectrl->oc_handle],prcv_queue->ErrorStr,read_bytes);
				    g_LastFirmwareErrCode[ponectrl->oc_handle] = prcv_queue->Status; // save the error code 
				    g_LastFirmwareErrStr[ponectrl->oc_handle][read_bytes] = 0; //make sure the 0 end as string 
				      
				  }else
					  {
						  Clean_Com_RCV_Buff
				          DbgWriteText("Status string size too big %d\r\n",spktHeader.payload_len);
				     }		  
				  }
			  
			    break;
		  case INTERRUPT_PACKET:
		

			   buff_length = spktHeader.payload_len;
	       	        
			   prcv_queue = ponectrl->prcv_queue->oc_Find_Rcv_Queue(spktHeader.if_type_unit);
		       if(prcv_queue ==NULL)
				   Clean_Com_RCV_Buff

               if(prcv_queue->pfIntCallBack && prcv_queue->pIntCallbackBuff && prcv_queue->IntCallbackDataSize)
			      {
				   
				   if(spktHeader.payload_len < prcv_queue->IntCallbackDataSize)
				     {
						 
						 ReadFile((HANDLE)Com_Handle,prcv_queue->pIntCallbackBuff,(DWORD)spktHeader.payload_len,(DWORD*)&read_bytes,NULL); 
						 
						 //get the whole packet
						 if(read_bytes == spktHeader.payload_len)
						   prcv_queue->pfIntCallBack(prcv_queue->pIntCallbackBuff,(uint16_t)read_bytes);
						 else //get the partial packet
						 {
						    uint32_t left_bytes_to_read = spktHeader.payload_len - read_bytes;
							ReadFile((HANDLE)Com_Handle,(prcv_queue->pIntCallbackBuff + read_bytes),left_bytes_to_read,(DWORD*)&read_bytes,NULL); 
						 
							if(read_bytes == left_bytes_to_read)
							  prcv_queue->pfIntCallBack(prcv_queue->pIntCallbackBuff,spktHeader.payload_len);
						     else{
							      Clean_Com_RCV_Buff;
					              DbgWriteText("cannot get the whole interrupt packet %d\r\n",spktHeader.payload_len);
						         }
					      }
				      
				       }else
				        {
					      Clean_Com_RCV_Buff;
					      DbgWriteText("Interrupt receiving buffer is too small %d, Payload size %d\r\n", prcv_queue->IntCallbackDataSize,spktHeader.payload_len);
					    } 

			        }else //no registered callback
				     {
                          Clean_Com_RCV_Buff;
					      DbgWriteText("There is no interrupt callback registered\r\n");

				  }
			   break;
		
		  case COMMAND_PACKET:
		  case DATA_PACKET:
			
			   buff_length = spktHeader.payload_len;
	       	        
			   prcv_queue = ponectrl->prcv_queue->oc_Find_Rcv_Queue(spktHeader.if_type_unit);
		       if(prcv_queue ==NULL)
				   Clean_Com_RCV_Buff
				   
			   if(spktHeader.packet_num == PACKET_FIRST )
		       {
				 // there is a payload
		          if(spktHeader.transfer_len)
				    buff_length = spktHeader.transfer_len;
				
			      
				  if(prcv_queue)
		           {
		             if(!ponectrl->prcv_queue->oc_Alloc_Rcv_Queue(prcv_queue,buff_length))
					   Clean_Com_RCV_Buff
				   
				   }else
				       Clean_Com_RCV_Buff
				  
				
			     } 
			   else//end first pack check
			    {
				   //never get the first packet
				   if( prcv_queue->State != QUEUE_STATE_FILLED)
					   Clean_Com_RCV_Buff
				}

			   //check the read data size and left buffer size
			   if((prcv_queue->pData + prcv_queue->WriteIndex + spktHeader.payload_len) <= (prcv_queue->pData + prcv_queue->CurBuffSize))
               {
			      ReadFile((HANDLE)Com_Handle,(prcv_queue->pData + prcv_queue->WriteIndex),(DWORD)spktHeader.payload_len,(DWORD*)&read_bytes,NULL); 

               }
			   else
                 {
					 Clean_Com_RCV_Buff
			         DbgWriteText("Payload size too big %d\r\n",spktHeader.payload_len);
			   }
			   
			   if(spktHeader.payload_len == read_bytes)
			   {
				   //need to protect by smphone
				   if(g_Sys_Manager.ocSysM_GetSemaphore(ponectrl->Read_Semaphore, 10000 ))
					       Clean_Com_RCV_Buff

/*** following 4 lines should execute only if ocSys_GetSemaphore succeeded ***/				   
					   prcv_queue->WriteIndex += read_bytes; 
				       prcv_queue->DataSize += read_bytes;
				       prcv_queue->State = QUEUE_STATE_FILLED;
			   	       prcv_queue->Status = spktHeader.status;
					   g_Sys_Manager.ocSysM_ReleaseSemaphore(ponectrl->Read_Semaphore ); 
				   
					
			   }else
			   {
			     
			       if(g_Sys_Manager.ocSysM_GetSemaphore(ponectrl->Read_Semaphore, 10000 ))
					       Clean_Com_RCV_Buff

/*** following 4 lines should execute only if ocSys_GetSemaphore succeeded ***/				   
					   prcv_queue->WriteIndex += read_bytes; 
				       prcv_queue->DataSize += read_bytes;
				       prcv_queue->State = QUEUE_STATE_FILLED;
			   	       prcv_queue->Status = spktHeader.status;
					   {
					       //reade left bytes
					       DWORD left_bytes;    
						   ReadFile((HANDLE)Com_Handle,(prcv_queue->pData + prcv_queue->WriteIndex),(DWORD)(spktHeader.payload_len - read_bytes) ,(DWORD*)&left_bytes,NULL);
						   if(left_bytes == (spktHeader.payload_len - read_bytes))
						   {
						   
						      prcv_queue->WriteIndex += left_bytes; 
				              prcv_queue->DataSize += left_bytes;
				              
						   }
						   else
							  Clean_Com_RCV_Buff
					      
					      }		  
						
					   g_Sys_Manager.ocSysM_ReleaseSemaphore(ponectrl->Read_Semaphore ); 

			    }
			     
			   if(prcv_queue->pfDataReadyCallBack && prcv_queue->pCallbackBuff)
			   {
				   if(g_Sys_Manager.ocSysM_GetSemaphore(ponectrl->Read_Semaphore, 1000)) 
                      Clean_Com_RCV_Buff

/*** following lines should execute only if ocSys_GetSemaphore succeeded ***/				   
				   uint32_t sent_datasize = prcv_queue->CallbackDataSize; 				   
				   
				   if(prcv_queue->DataSize < prcv_queue->CallbackDataSize)
					   sent_datasize = prcv_queue->DataSize;
				  
				   memcpy(prcv_queue->pCallbackBuff,prcv_queue->pData, sent_datasize);  
				   prcv_queue->pfDataReadyCallBack(prcv_queue->pCallbackBuff,sent_datasize);
				   
				   prcv_queue->ReadIndex += prcv_queue->DataSize; 
				   prcv_queue->DataSize -= sent_datasize;
				   
				   if(prcv_queue->DataSize == 0)
				    prcv_queue->State = QUEUE_STATE_IDLE;
				   
				   //callback must be reset by every call
				   prcv_queue->pfDataReadyCallBack = NULL;
			   	   g_Sys_Manager.ocSysM_ReleaseSemaphore(ponectrl->Read_Semaphore ); 
			   }
			  break;
		  case SCRIPT_DATA_PACKET:
          case SCRIPT_STATUS_PACKET:
			    break;
		  
		  default:
			   Clean_Com_RCV_Buff
					 
			  break;
		  
		   
		  
		  }
	}//while
	
	//close the duplicate handle
	CloseHandle((HANDLE)Com_Handle);  
	ponectrl->pm_onectrl->onectrl_Set_RcvThreadState(false);

  
}

#if 0 
DWORD CComThread::MonitorComm(DWORD nTimeOut, DWORD nExMsk)
{
 BOOL fSuccess;
 DWORD dwEvtMask;
 DWORD dRes = 0;
 m_dStopCom = 0;
 DWORD cmMask;
 GetCommMask(hCom,&cmMask);
 DWORD cmMaskTmp = cmMask & ~nExMsk;
 SetCommMask(hCom,cmMaskTmp);
 // Create an event object for use in WaitCommEvent.
 o.hEvent = CreateEvent(
  NULL,
  FALSE,
  FALSE,
  "CommTrigger"
  );
 do{
  if (WaitCommEvent(hCom, &dwEvtMask, &o))
  {
   dRes = dwEvtMask;
  }
  else{
   fSuccess = GetLastError();
   if(fSuccess == ERROR_IO_PENDING){
    DWORD res;
    MSG msg;
    do{
     res = WaitForSingleObject(o.hEvent,nTimeOut);
     if(PeekMessage(&msg,NULL,WM_USER, FM_MSG_LAST,PM_REMOVE))
      ProcessPostMsg(msg);
    }while(res == WAIT_TIMEOUT && !nTimeOut);
    switch(res){
    // Event is signaled
    case WAIT_OBJECT_0:
     dRes = dwEvtMask;
    // Wait operation timesout
    case WAIT_TIMEOUT:
     break;
    // There is an error
    default:
     break;
    }
   }
  }
  if(dRes == EV_RXCHAR)
  {
    // Lots of stuff
   }
   PurgeComm(hCom,PURGE_RXCLEAR);
   dwEvtMask = 0;
   dRes = 0x8000;
  }
  if(m_dStopCom)   // break if StopCom has been called
   return MAXDWORD;
 }while(dRes > 0x1888); // goes back to original do
  return dRes; 
#endif