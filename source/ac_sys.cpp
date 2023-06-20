
//*****************************************************************************
//
//  oc_sys.cpp - manage ACController.
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
//+ 2015-11-10 [RT] - Added include file ocAPI_System.h
//
#include <string.h>
#include <malloc.h>

#include "acctrl.h"
#include "acAPI_System.h"
#include "ac_sys.h"


//#pragma warning (disable:4996)

SYS_Manager::SYS_Manager()
{
    
	for(int i = 0; i < MAX_SUPPORT_OC; i++)
	    psOC_Sys[i] = (OC_SYS*)NULL;
}
SYS_Manager::~SYS_Manager()
{
    
	for(int i = 0; i < MAX_SUPPORT_OC; i++)
	 { 
		 if(psOC_Sys[i])
		 {
			 if(psOC_Sys[i]->isConnected)
			 {
				if(psOC_Sys[i]->isOpen)
					Sys_Close(i);

			     ocSysM_UnInitACController(i);
				 delete psOC_Sys[i];
				 psOC_Sys[i] =(OC_SYS*)NULL;
			 }
		 }
	
	}

}
void SYS_Manager::ocSysM_Init()
{
    
	for(int i = 0; i < MAX_SUPPORT_OC; i++)
	  psOC_Sys[i] = (OC_SYS*)NULL;
}


void SYS_Manager::ocSysM_UnInit()
{
    
	for(int i = 0; i < MAX_SUPPORT_OC; i++)
	 { 
		 if(psOC_Sys[i])
		 {
			 if(psOC_Sys[i]->isConnected)
			 {
				if(psOC_Sys[i]->isOpen)
					Sys_Close(i);
			    ocSysM_UnInitACController(i);
				delete psOC_Sys[i];
				psOC_Sys[i] =(OC_SYS*)NULL;
			 }
		 }
	
	}
}


int32_t SYS_Manager::ocSysM_FindACControllerFreeHandle()
{
    int32_t oc_Handle = 0;  
	for(int i = 1; i < MAX_SUPPORT_OC; i++)
       {
		  if(psOC_Sys[i] ==(OC_SYS*)NULL)
	         return ((i));
	   }
   return oc_Handle;
}

int32_t SYS_Manager::ocSysM_FindACControllerHandle(uint8_t Com_Port_Num)
{
    int32_t oc_handle = 0;  
	for(int i = 0; i < MAX_SUPPORT_OC; i++)
       {
		  if(psOC_Sys[i]!=(OC_SYS*)NULL)
	        {
			   if(psOC_Sys[i]->Com_Port_Num == Com_Port_Num)
				return (i);
		     }
	   }
   return oc_handle;
}


int32_t SYS_Manager::ocSysM_InitACController(int32_t oc_handle )
{
    int32_t result = -1;       
    
	
	 
	// psOC_Sys[oc_handle] =(OC_SYS*) malloc(sizeof(OC_SYS));
     
	 psOC_Sys[oc_handle] =new OC_SYS();
		 
	 if(g_Sys_Manager.psOC_Sys[oc_handle])
		{
			memset(psOC_Sys[oc_handle],0,sizeof(OC_SYS));
			psOC_Sys[oc_handle]->oc_handle = oc_handle;
			psOC_Sys[oc_handle]->isConnected = false;
			psOC_Sys[oc_handle]->isOpen = false;
			psOC_Sys[oc_handle]->Com_Port_Num = 0;
			result = 0;
			for(int i =0; i < IF_MAX; i++)
			 {
			     for(int j =0; j < MAX_UNIT; j++)
			     	psOC_Sys[oc_handle]->Rcv[i][j] =NULL;
			 }
			for(int ir=0; ir < 5; ir++)
			  psOC_Sys[oc_handle]->Rcv_Buff[ir] = NULL;

			psOC_Sys[oc_handle]->Read_Semaphore = ocSysM_CreateSemaphore(1,1, NULL);
			psOC_Sys[oc_handle]->Write_Semaphore = ocSysM_CreateSemaphore(1,1, NULL);
			//g_psOC_Sys[oc_Handle]->oc_IO_ReadEvent = ocSys_CreateEvent(NULL);
			//g_psOC_Sys[oc_Handle]->oc_IO_WriteEvent = ocSys_CreateEvent(NULL);
			psOC_Sys[oc_handle]->prcv_queue = new ocRcvQueue(oc_handle);
			psOC_Sys[oc_handle]->pm_onectrl = new OneCtrl_Manager(g_Sys_Manager.psOC_Sys[oc_handle]);
			if(!psOC_Sys[oc_handle]->prcv_queue || !psOC_Sys[oc_handle]->pm_onectrl)
				return -1;
	 }
	 return result;
}
int32_t SYS_Manager::ocSysM_UnInitACController(int32_t oc_handle)
{
	

	  if(g_Sys_Manager.psOC_Sys[oc_handle])
	  {	 

	   ocSysM_CloseSemaphore(psOC_Sys[oc_handle]->Read_Semaphore);
	   ocSysM_CloseSemaphore(psOC_Sys[oc_handle]->Write_Semaphore);
	  
	   if(psOC_Sys[oc_handle]->prcv_queue && psOC_Sys[oc_handle]->pm_onectrl)
	    {
		  delete psOC_Sys[oc_handle]->prcv_queue;
	      delete psOC_Sys[oc_handle]->pm_onectrl;
	    }
	   delete psOC_Sys[oc_handle];

	   psOC_Sys[oc_handle] =(OC_SYS*)NULL;
	  }
	 return 0;
}

// returns the index of the entry for the specified serial number 
uint32_t SYS_Manager::ocSysM_isFoundUSBSerial(char *serial)
{
    uint32_t index = 0; //zero means 'not found'

    if (NULL != serial)
    {
        for(int i = 1; i < MAX_SUPPORT_OC; i++) 
        {
            if(psOC_Sys[i] && !strcmp(psOC_Sys[i]->oc_UsbAttr, serial))   
            {
                index = i;
                break;
            }
        }
    }

    return index; 
}

//semaphore function group

uint32_t SYS_Manager::ocSysM_CreateSemaphore(uint8_t InitialCount, uint8_t MaximumCount, int8_t1 *pName)
{
  return ac_OS_CreateSemaphore(InitialCount, MaximumCount, pName);
}
void SYS_Manager::ocSysM_CloseSemaphore(uint32_t SemaphoreID )
{
  	ac_OS_CloseSemaphore(SemaphoreID );
}

uint32_t SYS_Manager::ocSysM_GetSemaphore(uint32_t SemaphoreID, uint32_t MS  )
{
  
	return ac_OS_GetSemaphore(SemaphoreID,  MS );

}

void SYS_Manager::ocSysM_ReleaseSemaphore(uint32_t SemaphoreID )
{
  
	ac_OS_ReleaseSemaphore(SemaphoreID );

}

uint32_t SYS_Manager::ocSysM_CreateEvent(int8_t1 * pName  )
{
  
	return ac_OS_CreateEvent( pName);

}

int32_t SYS_Manager::ocSysM_MDir(int8_t1 * Path)
{
  	   return ac_OS_MDir(Path);
}

void SYS_Manager::ocSysM_GetTimestemp(int8_t1 * pBuff)
{
   ac_OS_GetTimestemp(pBuff);
}

void OneCtrl_Manager::onectrl_Set_USBSerial(int8_t1 *Usb_Attr)
{
  
	 ASSERT(Usb_Attr);
	
#ifdef SAFE_STR
	 strcpy_s(ponectrl->oc_UsbAttr, sizeof(ponectrl->oc_UsbAttr), Usb_Attr);
#else
	 strcpy(ponectrl->oc_UsbAttr, Usb_Attr);
#endif
   
}

void OneCtrl_Manager::onectrl_Set_OCConnected(bool isConnected)
{
	
   	   ponectrl->isConnected = isConnected;
   
}

bool OneCtrl_Manager::onectrl_Get_OCConnected()
{
	
   	   return ponectrl->isConnected;
   
}
void OneCtrl_Manager::onectrl_Set_OCOpen(bool isOpen)
{
         // ASSERT( g_psOC_Sys[oc_Handle]);
   	  	  
		  ponectrl->isOpen = isOpen;
   
}
bool OneCtrl_Manager::onectrl_Get_OCOpen()
{
          
		 return  ponectrl->isOpen ;
   
}
void OneCtrl_Manager::onectrl_Set_Com_Port_Num( uint8_t Port_Num)
{
      ponectrl->Com_Port_Num = Port_Num;

}

uint8_t  OneCtrl_Manager::onectrl_Get_Com_Port_Num()
{
      return ponectrl->Com_Port_Num ;

}

void OneCtrl_Manager::onectrl_Set_OpenocComHandle(int32_t Com_Handle, int32_t Com_DHandle )
{
        
		ponectrl->Com_Handle = Com_Handle;
		ponectrl->Com_DHandle = Com_DHandle;
	   
}

void OneCtrl_Manager::onectrl_Set_RcvThreadID(uint32_t Rcv_Thread_ID)
{
         
		 ponectrl->Rcv_Thread_ID = Rcv_Thread_ID ;
		

}

void OneCtrl_Manager::onectrl_Set_RcvThreadState(bool isRunning)
{
      
		  ponectrl->Rcv_Thraed_isRunning = isRunning;
		

}

bool OneCtrl_Manager::onectrl_Get_RcvThreadState()
{
      
		return  ponectrl->Rcv_Thraed_isRunning ;
		

}
int32_t OneCtrl_Manager::onectrl_Get_OpenComHandle()
{
      
	
	return ponectrl->Com_Handle;
			
	  
	   
	  
}

#if 0
int32_t OneCtrl_Manager::onectrl_Get_ReadEventHandle()
{
       ASSERT(g_psOC_Sys[oc_handle]);
		
	   return oc_sys->oc_IO_ReadEvent;
	   
}
#endif
int32_t OneCtrl_Manager::onectrl_Get_OpenComDupHandle()
{
       ASSERT(g_psOC_Sys[oc_handle]);
		
	   return ponectrl->Com_DHandle;
	   
}




