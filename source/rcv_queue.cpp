#include <string.h>
#include <malloc.h>

#include "acctrl.h"
#include "rcv_queue.h"

RCV_QUEUE  *ocRcvQueue::oc_Find_Rcv_Queue(uint16_t IF)
{
	RCV_QUEUE *rcv_buff =(RCV_QUEUE*) NULL;

     
	 //don'tinitialze OneController system yet
	 if(!g_Sys_Manager.psOC_Sys[oc_handle])
         {
		     DbgWriteText("oc_Find_Rcv_Queue : systme not initalized \r\n");
			 return rcv_buff;
	     }
	 uint8_t if_type = (IF >> 8) & 0xFF;
	 uint8_t uint = (IF & 0xFF);
	
	if((if_type >= IF_SYS && IF_SYS < IF_MAX ) && uint >=0 && uint < MAX_UNIT)
	{
	
		if(g_Sys_Manager.psOC_Sys[oc_handle]->Rcv[if_type][uint])
			return g_Sys_Manager.psOC_Sys[oc_handle]->Rcv[if_type][uint];
		else
		{
		   g_Sys_Manager.psOC_Sys[oc_handle]->Rcv[if_type][uint] = (RCV_QUEUE*)malloc(sizeof(RCV_QUEUE)); //new RCV_QUEUE(); 
		   if(!g_Sys_Manager.psOC_Sys[oc_handle]->Rcv[if_type][uint])
		   {
		     DbgWriteText("oc_Find_Rcv_Queue : if_type =%d, unit =%d  cannot allocate rcv_queue \r\n");
	          return NULL;
		   }
		  
		   memset(g_Sys_Manager.psOC_Sys[oc_handle]->Rcv[if_type][uint], 0, sizeof(RCV_QUEUE));


		   return g_Sys_Manager.psOC_Sys[oc_handle]->Rcv[if_type][uint];
		}
		    

	}else
	{
	    DbgWriteText("oc_Find_Rcv_Queue : if_type =%d, unit =%d out of range \r\n");
	    
	}
	 

	return NULL;
	 
#if 0	 
	 switch(if_type)
	 {
	 case IF_SYS:
		   //sys interface, only has a unit 
		   if(!uint) 
			 rcv_buff = &g_Sys_Manager.psOC_Sys[oc_handle]->Sys_Rcv;
		   
		  break;

	 case IF_I2C:
		   if(uint >= 0 && uint < MAX_I2C_BUS ) 
			   rcv_buff = &g_Sys_Manager.psOC_Sys[oc_handle]->I2C_Rcv[uint];
		   
		   break;
     case IF_SPI:
		  
		  if(uint >= 0 && uint < MAX_SPI_BUS ) 
			   rcv_buff = &g_Sys_Manager.psOC_Sys[oc_handle]->SPI_Rcv[uint];
		  
		 break;
     case IF_GPIO:
		   if(!uint ) 
			 rcv_buff = &g_Sys_Manager.psOC_Sys[oc_handle]->Gpio_Rcv;
		   break;
	 case IF_POWER:
		   if(!uint ) 
			 rcv_buff = &g_Sys_Manager.psOC_Sys[oc_handle]->Power_Rcv;
		   break;
	 
	 default:
		 break;

	 }
	 
	 return rcv_buff;
#endif
}

	

int32_t  ocRcvQueue::oc_Alloc_Rcv_Queue(RCV_QUEUE * Rcv_Queue, uint32_t DatSize)
{
     int32_t result = 0;
	Rcv_Queue->ReadIndex  = 0;
	Rcv_Queue->WriteIndex = 0;
	if(DatSize)
	 {
	 
      //in two cases: need to alloc receiving buffer: size is smaller and no buffer		  
	  if( !Rcv_Queue->pData || (Rcv_Queue->CurBuffSize < DatSize)) 
	   {
	      if(Rcv_Queue->pData) free(Rcv_Queue->pData);
		
		  Rcv_Queue->pData = (uint8_t*)malloc(DatSize + 64);
	       
		  if(!Rcv_Queue->pData)
           return result;

		  Rcv_Queue->ReadIndex = 0;
		  Rcv_Queue->WriteIndex = 0;
		  Rcv_Queue->State = QUEUE_STATE_IDLE; 
		  Rcv_Queue->CurBuffSize = DatSize+64;
	   }
	
	 }
	 //reinitalize data in buffer
	 Rcv_Queue->DataSize =  0;

	return (result=1);
} 

void  ocRcvQueue::oc_Release_Rcv_Queue(RCV_QUEUE * Rcv_Queue)
{
    if(Rcv_Queue->pData) free(Rcv_Queue->pData);
	
	Rcv_Queue->pData =(uint8_t*)NULL;
	Rcv_Queue->CurBuffSize = 0;
}

void ocRcvQueue::oc_Init_Rcv_Queue(RCV_QUEUE * Rcv_Queue, uint16_t IF)
{
	
	memset(Rcv_Queue, 0, sizeof(RCV_QUEUE));
	//Rcv_Queue->IF = IF;
	
}