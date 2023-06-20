//*****************************************************************************
//
// ac_sys.h - ACController system information.
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

#ifndef __OC_SYS_H
#define __OC_SYS_H

//#include "ocAPI_interface.h"

#ifdef  __cplusplus
extern "C" {
#endif

//#ifndef __TYPEDEF_H
//#include "oc_stdint.h"
//#endif
#ifndef __RCV_QUEUE_H
#include "rcv_queue.h"
#endif

#ifndef __RING_BUFF_H
#include "ring_buff.h"
#endif

/* RT 01-07-16: Changed MAX_UNIT to 6 to allow 6 I2C ports */
#define MAX_UNIT   10
//support maxumum ACController number
#define MAX_SUPPORT_OC 6 
//usb attribute string length
#define USB_ATTR_STR_LEN     128   

//maximum I2C bus number
#define MAX_I2C_BUS   6
//maximum SPI buss nuber
#define MAX_SPI_BUS   2
//every bus maximu chip select
#define MAX_SPI_CS    8
typedef enum{
	ACCONTROLLER_UNPLUG = 1,
	ACCONTROLLER_PLUG
}EVENET_TYPE;
	
class OneCtrl_Manager;
extern ACCONTROLLERINFO g_sInfo;
//ACController infomation 
class OC_SYS{
 public:
	         
	           ocRcvQueue  *prcv_queue; //the queue management 
			   OneCtrl_Manager *pm_onectrl; //ACController mamangement

			   int32_t oc_handle;
	           int8_t1  oc_UsbAttr[USB_ATTR_STR_LEN];
			   bool isConnected;
			   uint8_t Com_Port_Num;
			   bool isOpen;
			   int32_t Com_Handle;
			   int32_t Com_DHandle; 
			   uint32_t Read_Semaphore;
			   uint32_t Write_Semaphore;
			   uint32_t Rcv_Thread_ID;
			   uint32_t oc_IO_ReadEvent;
			   uint32_t oc_IO_WriteEvent;
			   bool     Rcv_Thraed_isRunning; 
			   RCV_RINGBUFF *Rcv_Buff[5];
			 //  RCV_QUEUE Sys_Rcv;
              // RCV_QUEUE I2C_Rcv[MAX_I2C_BUS];
              // RCV_QUEUE SPI_Rcv[MAX_SPI_BUS];//[MAX_SPI_CS];
               //RCV_QUEUE Gpio_Rcv;
			   //RCV_QUEUE Power_Rcv;
			   //RCV_QUEUE PWM_Rcv;
			   //RCV_QUEUE CAN_Rcv;
			   //RCV_QUEUE ONEWIRE_Rcv;
			   RCV_QUEUE *Rcv[IF_MAX][MAX_UNIT];
             

			 

};//OC_SYS;

/**/

class SYS_Manager{
public:
	SYS_Manager();
	~SYS_Manager();

/** initalize ACController ACController system 
   
*/
void ocSysM_Init();

/** Uninitalize ACController system
    
*/

void ocSysM_UnInit();

/** Find the free ACController handlle 
    retrun: ACController handle
*/
int32_t ocSysM_FindACControllerFreeHandle();

/** find a free ACController handle 
    
    return: the handle
*/
int32_t ocSysM_FindACControllerHandle(uint8_t Com_Port_Num);

/** initalize ACController device 
    @param oc_Handle: return by ocSys_FindACControllerHandle
    return: 0, if success
*/


 int32_t ocSysM_InitACController(int32_t oc_handle);

/** Uninitalize ACController device 
    @param oc_Handle: return by ocSys_FindACControllerHandle
    return: 0, if success
*/
int32_t ocSysM_UnInitACController(int32_t oc_handle);

uint32_t ocSysM_CreateSemaphore(uint8_t InitialCount, uint8_t MaximumCount, int8_t1 *pName);
void ocSysM_CloseSemaphore(uint32_t SemaphoreID );
uint32_t ocSysM_GetSemaphore(uint32_t SemaphoreID, uint32_t MS  );
void ocSysM_ReleaseSemaphore(uint32_t SemaphoreID );
uint32_t ocSysM_CreateEvent(int8_t1 * pName  );
void ocSysM_GetTimestemp(int8_t1 * pBuff);
int32_t ocSysM_MDir(int8_t1 * Path);
uint32_t ocSysM_isFoundUSBSerial(char *serial);

//private:
	/* global variable  */
   OC_SYS  *psOC_Sys[MAX_SUPPORT_OC];

};

class OneCtrl_Manager{ 

public:
	OneCtrl_Manager(OC_SYS  *ponectrl) { this->ponectrl = ponectrl; }
	
void onectrl_Set_USBSerial(int8_t1 *Usb_Attr);

/** Set ACController related virtual COm port 
    @param Port number: virtual com port
	
*/
void onectrl_Set_Com_Port_Num(uint8_t Port_Num);
uint8_t  onectrl_Get_Com_Port_Num();
void     onectrl_Set_OpenocComHandle(int32_t Com_Handle, int32_t Com_DHandle );

int32_t onectrl_Get_OpenComHandle();
int32_t onectrl_Get_OpenComDupHandle();

void onectrl_Set_OCConnected(bool isConnected);
bool onectrl_Get_OCConnected();

void onectrl_Set_OCOpen(bool isOpen);
bool onectrl_Get_OCOpen();
void onectrl_Set_RcvThreadID(uint32_t Rcv_Thread_ID);
void onectrl_Set_RcvThreadState(bool isRunning);
bool onectrl_Get_RcvThreadState();

private:
	 OC_SYS  *ponectrl;
};

#ifdef  __cplusplus
} 
#endif

#endif
