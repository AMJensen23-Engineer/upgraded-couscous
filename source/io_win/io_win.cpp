//*****************************************************************************
//
// io_win.c - implement IO Windows function.
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
#include <process.h>
#include <Dbt.h>
#include <setupapi.h>
#include <stdio.h>
#include <Setupapi.h>
#include <Cfgmgr32.h>

#include "acctrl.h"
#include "acctrl_api.h"

#include "rcv_thread.h"



#define RETRY_SCAN_COUNT        0

//#pragma warning (disable:4996)

/*keep the found ACController port number */
static uint8_t ACControllerCom[10];
static int8_t1 FoundACControllerNum = -1;
static HANDLE dupHandle; //used by receive thread 
static bool g_scan = false;

ACCONTROLLERINFO g_sInfo;
STATUS ScanACController()
{
    uint8_t ACControllerNum = 0;
    int i;
    SP_DEVINFO_DATA sDevInfoData;
    char szSearchKey[80];
    char szSearchKey1[80];

    wsprintf(szSearchKey, "USB\\VID_%04X&PID_%04X", USB_VID_VIA, USB_PID_VIA_BOOSTERPACK);
    wsprintf(szSearchKey1, "USB\\VID_%04X&PID_%04X&MI", USB_VID_VIA, USB_PID_VIA_BOOSTERPACK);

#if RETRY_SCAN_COUNT
    int retry = RETRY_SCAN_COUNT;
    do
    {
#endif
        HDEVINFO hDevInfo = SetupDiGetClassDevs(NULL,"USB",NULL,DIGCF_PRESENT|DIGCF_ALLCLASSES);

        if( -1 == (int)hDevInfo ) 
        {
            return OC_ERR_OPERATION_FAILED;
        }

        sDevInfoData.cbSize = sizeof(SP_DEVINFO_DATA);

        for(i=0; SetupDiEnumDeviceInfo(hDevInfo, i, &sDevInfoData); i++) 
        {
            DWORD nSize=0 ;
            char buf[MAX_PATH];
            // DWORD DataT ;
            char *szSerialNum;
            char szDeviceInstanceID[1024];

            CONFIGRET cr = CM_Get_Device_ID(sDevInfoData.DevInst, szDeviceInstanceID , MAX_PATH, 0);
            if (cr != CR_SUCCESS)
            {
                continue;
            }

            // Make sure the device VID and PID are the ones we're looking for
            if (strstr(szDeviceInstanceID, szSearchKey) == NULL  )
            {
                continue;
            }

            // Make sure this is not an "MI" record
            if(strstr(szDeviceInstanceID, szSearchKey1) != NULL)
            {
                continue;
            }

            szSerialNum = &szDeviceInstanceID[(strlen(szSearchKey)+1)];

            // The serial number MUST be the correct length!
            // Changed to six hex characters (e.g., A569B3) starting with V1.0.4.45
           // if (strlen(szSerialNum) != VALID_SN_LEN)
            //{
              //  continue;
            //}

            char szKey[1200];

#ifdef SAFE_STR
            sprintf_s(szKey, sizeof(szKey), "System\\Controlset001\\Enum\\%s\\%s",szSearchKey,szSerialNum);
#else
            sprintf(szKey,"System\\Controlset001\\Enum\\%s\\%s",szSearchKey,szSerialNum);
#endif

            DWORD BufferSize = 100;
            RegGetValue(HKEY_LOCAL_MACHINE, szKey, ("ParentIdPrefix"), RRF_RT_ANY, NULL,(PVOID)buf, &BufferSize);
#ifdef SAFE_STR
            sprintf_s(szKey, sizeof(szKey), "System\\Controlset001\\Enum\\%s&MI_00\\%s&0000", szSearchKey, buf);
#else
            sprintf(szKey,"System\\Controlset001\\Enum\\%s&MI_00\\%s&0000",szSearchKey,buf);
#endif
            BufferSize =100;
            if(RegGetValue(HKEY_LOCAL_MACHINE,szKey,("FriendlyName"),RRF_RT_ANY,NULL,(PVOID)buf,&BufferSize)== ERROR_SUCCESS)
            {   
               AC_HANDLE oc_handle;
#if 1
                int32_t port = ac_OSCDC_GetComPortFromSerial(szSerialNum);

                // if we already know the port for this serial number
                if (port > 0)
                {
                    ACControllerCom[ACControllerNum] = port;
                    ACControllerNum++;
                    continue;
                }
#else
                uint32_t index;

                //if it is second times calling.
                if (0 == (index = g_Sys_Manager.ocSys_isFoundUSBSerial(szSerialNum)))
                {
                    ACControllerCom[ACControllerNum] = g_Sys_Manager.psOC_Sys[index]->pm_onectrl->onectrl_Get_Com_Port_Num();
                    ACControllerNum++;
                    continue;
                }
#endif
                //get the free ACController resource
                oc_handle = g_Sys_Manager.ocSysM_FindACControllerFreeHandle();
                if (INVALID_EVMC_HANDLE == oc_handle)
                {
                    //write debug info
                    return OC_ERR_OPERATION_FAILED;
                }

                //initalize ACController resource 
                if(g_Sys_Manager.ocSysM_InitACController(oc_handle))
                {
                    return OC_ERR_OPERATION_FAILED;
                }

                g_Sys_Manager.psOC_Sys[oc_handle]->pm_onectrl->onectrl_Set_USBSerial(szSerialNum);
                g_Sys_Manager.psOC_Sys[oc_handle]->pm_onectrl->onectrl_Set_OCConnected(true);

                //find ACController virtual com port number
                char *p_comnum=strstr(buf,"COM");

                if (p_comnum)
                {
#if 1
                    ACControllerCom[ACControllerNum] = atoi(p_comnum + 3);
#else
                    p_comnum=p_comnum + 3;
                    if(*p_comnum >='0' && *p_comnum <='9') 
                    {
                        ACControllerCom[ACControllerNum] = *p_comnum -'0'; 
                        p_comnum++;
                        if(*p_comnum >='0' && *p_comnum <='9') 
                        {
                            ACControllerCom[ACControllerNum] *= 10; 
                            ACControllerCom[ACControllerNum] += (*p_comnum -'0'); 
                            p_comnum++;   
                            if(*p_comnum >='0' && *p_comnum <='9')
                            {
                                ACControllerCom[ACControllerNum] *= 10; 
                                ACControllerCom[ACControllerNum] += (*p_comnum -'0'); 
                            }
                        }
                    }
#endif
                    g_Sys_Manager.psOC_Sys[oc_handle]->pm_onectrl->onectrl_Set_Com_Port_Num(ACControllerCom[ACControllerNum]);   
                    ACControllerNum++;
                } 
            }
        }

        FoundACControllerNum = ACControllerNum; 

#if RETRY_SCAN_COUNT // should be done by application program, if necessary
        // if device is not found, sleep some time and try again
        if( retry < RETRY_SCAN_COUNT && FoundACControllerNum == 0)
        {
            Sleep(500);
        }

    } while(ACControllerNum == 0 && ++retry < RETRY_SCAN_COUNT );
#endif

    return ACControllerNum;
}


STATUS ac_OSCDC_FindACControllers()
{
   return ScanACController();
  
}

int32_t ac_OSCDC_GetComPortNumber(uint8_t ACControllerIndex)
{
    if (ACControllerIndex >= FoundACControllerNum)
    {
        return OC_ERR_PARAM_OUT_OF_RANGE;
    }

    return ACControllerCom[ACControllerIndex];
}

STATUS ac_OSCDC_GetComPortFromSerial(const char *szSerialNum)
{
    uint32_t index = g_Sys_Manager.ocSysM_isFoundUSBSerial((char *) szSerialNum);

    if (0 != index)
    {
        return (STATUS) g_Sys_Manager.psOC_Sys[index]->pm_onectrl->onectrl_Get_Com_Port_Num();
    }

    return OC_ERR_SYS_COM_INVALID_PORT;
}

STATUS ac_OSCDC_OpenComPort(int32_t port)
{
    char portname[40];

    sprintf_s(portname,"\\\\.\\COM%d", port);
    HANDLE hComm = CreateFile( portname, GENERIC_READ | GENERIC_WRITE, 
            0, 0, OPEN_EXISTING, 0, 0);

    return (hComm != INVALID_HANDLE_VALUE) ? (STATUS) hComm : OC_ERR_SYS_COM_OPEN_FAILED;
}

STATUS ac_OSCDC_CloseComPort(int32_t hComm)
{
    BOOL success = CloseHandle((HANDLE) hComm);

    return success ? OC_SUCCESS : OC_ERR_OPERATION_FAILED;
}

//#define THROUGHPUT_TEST
#ifdef THROUGHPUT_TEST 
void send_Data1(int32_t oc_handle)
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
	spktHeader->Param[0]=5096;
	//char buff[215];


	int i =ocPacket_Write(oc_handle,buff,512);
}
	void Init_Com(HANDLE handle);
	void startTime();

double GetElapsedTime();
double t ;
#endif

AC_HANDLE ac_OSCDC_Open(uint8_t ComPortNum)
{
    HANDLE m_handle;
    //need to be passed to receiving thread
    static AC_HANDLE oc_handle;	
    OC_SYS  *ponectrl = NULL; 
    char buff[256];

    oc_handle = g_Sys_Manager.ocSysM_FindACControllerHandle(ComPortNum);

    if(oc_handle)
    {
        ponectrl = g_Sys_Manager.psOC_Sys[oc_handle] ;

        if((ponectrl && ponectrl->pm_onectrl->onectrl_Get_OCOpen()))
        {
            DbgWriteText(" The com%d already opened \r\n",ComPortNum);
            return OC_ERR_SYS_COM_OPEN_FAILED;
        }
    } else
    {
        //scan ACController
        ScanACController();
        oc_handle= g_Sys_Manager.ocSysM_FindACControllerHandle(ComPortNum);
        if(oc_handle)
            ponectrl = g_Sys_Manager.psOC_Sys[oc_handle] ;
        {
            DbgWriteText(" The com %d is not ACController's port or FindACController be not called  \r\n",ComPortNum);
            return OC_ERR_SYS_COM_OPEN_FAILED;
        }
    }
#ifdef SAFE_STR
    sprintf_s(buff,"\\\\.\\COM%d",ComPortNum);
#else
    sprintf(buff,"\\\\.\\COM%d",ComPortNum);
#endif

    //open the ACController virtual com port
    m_handle= 	 CreateFile(buff,   
        GENERIC_READ | GENERIC_WRITE,
        0,                    
        NULL,                 
        OPEN_EXISTING,        
        0, 
        NULL);         

    DbgWriteText(" Open %s\r\n",buff ); 
    if(m_handle != INVALID_HANDLE_VALUE && ponectrl )
    {

        DuplicateHandle(GetCurrentProcess(),m_handle, GetCurrentProcess(),&dupHandle, 0, FALSE, DUPLICATE_SAME_ACCESS);

        ponectrl->pm_onectrl->onectrl_Set_OpenocComHandle((int32_t)m_handle,(int32_t)dupHandle );
        ponectrl->pm_onectrl->onectrl_Set_OCOpen(true);

#ifdef THROUGHPUT_TEST

        {



            //ask_Data(ponectrl->oc_handle); 
            int read_header = sizeof(OCF_PACKET_HEADER); 
            DWORD read_bytes;
            uint32_t buff_length;
            OCF_PACKET_HEADER spktHeader;
            DWORD dwEvtMask;

            RCV_QUEUE *prcv_queue;

            //initalize virtual Com port
            Init_Com(m_handle);
            unsigned char buff[2048];
            int i =0;
            int tread_bytes =512;
            DWORD rread_bytes;

            send_Data1(oc_handle);
            startTime();
            while(1)
            {
                ReadFile(m_handle,&spktHeader,read_header,&rread_bytes,0);

                if( read_header == rread_bytes)// && spktHeader.signature == PACKET_SIGNATURE )
                { 

                    ReadFile(m_handle,buff,spktHeader.payload_len,&rread_bytes,0);
                    i++;
                    buff[1] =0;
                } else WaitCommEvent(m_handle,&dwEvtMask,NULL);
                if(i>4096) break;
            }

            t = GetElapsedTime();

        }

#endif

        DWORD id;
        if( id =(DWORD)_beginthread(ac_Rcv_Thread,0,ponectrl))
        {
            ponectrl->pm_onectrl->onectrl_Set_RcvThreadID(id);
            ponectrl->pm_onectrl->onectrl_Set_RcvThreadState(true);
        }

    }else {
        oc_handle = OC_ERR_SYS_COM_OPEN_FAILED;
        DbgWriteText(" Cannot open %s\r\n",buff ); 
        return oc_handle ;
    }

    DbgWriteText("try to get system info \r\n"); 

    //work around to solve the first time read from firmware slow
    
    int status = Sys_GetACControllerInfo(oc_handle, &g_sInfo);
    //work around for first time failure
    if(status < 0 )
        status = Sys_GetACControllerInfo(oc_handle, &g_sInfo);
	
    // double ii= GetElapsedTime();
    if(status < 0) 
        DbgWriteText("get system info failure  \r\n"); 
	//reset resorce  
	Sys_ResetResource(oc_handle);
    return oc_handle;
}

	 
STATUS ac_OSCDC_Close(AC_HANDLE Handle)
{
    int timeout =20;

    DbgWriteText("ocSys_Close handle =%d\r\n", Handle);

    //in case the com port is closed 
    if(g_Sys_Manager.psOC_Sys[Handle]== NULL)
        return OC_ERR_SYS_COM_PORT_NOT_OPEN;

    if(g_Sys_Manager.psOC_Sys[Handle]->isOpen == true)
    {
        g_Sys_Manager.psOC_Sys[Handle]->isOpen = false;

        if(g_Sys_Manager.psOC_Sys[Handle]->pm_onectrl->onectrl_Get_RcvThreadState() )
        {
            while(g_Sys_Manager.psOC_Sys[Handle]->pm_onectrl->onectrl_Get_RcvThreadState()&& timeout--)
                Sleep(10);
        }

        if(timeout)
        {
            if(CloseHandle((HANDLE)g_Sys_Manager.psOC_Sys[Handle]->Com_Handle))
                return OC_SUCCESS;
            else return  OC_ERR_SYS_COM_PORT_NOT_OPEN;
        } else OC_ERR_SYS_COM_PORT_NOT_OPEN;
    } 

    return OC_ERR_SYS_COM_PORT_NOT_OPEN;
}

int32_t ac_OSCDC_RegsterIfReadQueue(uint16_t IF)
{

	int32_t result =-1;

    return result;

}

//no need for these two routines
//RCV_QUEUE  *oc_Find_Rcv_Queue(int32_t oc_Handle, uint16_t IF);
int32_t ac_OSCDC_ReadAsync(int32_t oc_handle, uint16_t IF,uint8_t *pdata, uint8_t data_size , void (*data_ready)( uint8_t *Pdata, uint16_t data_size))
{

	 	int32_t result =-1;
	 RCV_QUEUE *prcv_queue;
	 
	 if(pdata == NULL)
		 return result;

	 prcv_queue = g_Sys_Manager.psOC_Sys[oc_handle]->prcv_queue->oc_Find_Rcv_Queue(IF);
	 if(prcv_queue)
	 {
		
		 if(prcv_queue->State != QUEUE_STATE_FILLED ||  !prcv_queue->DataSize)
		 {
			 prcv_queue->pfDataReadyCallBack =  data_ready;
			 prcv_queue->pCallbackBuff = pdata;
			 prcv_queue->CallbackDataSize = data_size;
			 return 0;
		 }
		 
		 
		 if(prcv_queue->State == QUEUE_STATE_FILLED && prcv_queue->DataSize)
		 {
		     if(!ac_OS_GetSemaphore(g_Sys_Manager.psOC_Sys[oc_handle]->Read_Semaphore, 10000 ))
			 {
			     uint32_t read_data = data_size;
				 if(prcv_queue->DataSize < data_size)
					 read_data = prcv_queue->DataSize;
				 memcpy(pdata, (prcv_queue->pData + prcv_queue->ReadIndex) ,read_data);

				 prcv_queue->DataSize -= read_data;
				 prcv_queue->ReadIndex += read_data;
				if(!prcv_queue->DataSize)
					prcv_queue->State = QUEUE_STATE_IDLE; 
				 
				 result = read_data;
				 
				 ac_OS_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[oc_handle]->Read_Semaphore);
			 }

             
		 }else
		   return 0;
	 }

    return result;


}
int32_t ac_OSCDC_ReadSync (  int32_t oc_handle,  uint16_t IF,  uint8_t *pdata, uint8_t data_size , uint8_t timeout)
{
	 int32_t result =-1;
	 RCV_QUEUE *prcv_queue;
	 
	 if(pdata == NULL)
		 return result;

	 prcv_queue = g_Sys_Manager.psOC_Sys[oc_handle]->prcv_queue->oc_Find_Rcv_Queue(IF);
	 if(prcv_queue)
	 {
		
		 if(prcv_queue->State != QUEUE_STATE_FILLED ||  !prcv_queue->DataSize)
		 {
		     int wait_count = timeout/10+1;
			 while(wait_count)
			  {
				 ac_OS_Sleep(10);
			    if(prcv_queue->State == QUEUE_STATE_FILLED && prcv_queue->DataSize)
					break;
				wait_count--;
			 }
		 }
		 
		 
		 if(prcv_queue->State == QUEUE_STATE_FILLED && prcv_queue->DataSize)
		 {
		     if(!ac_OS_GetSemaphore(g_Sys_Manager.psOC_Sys[oc_handle]->Read_Semaphore, 10000 ))
			 {
			     uint32_t read_data = data_size;
				 if(prcv_queue->DataSize < data_size)
					 read_data = prcv_queue->DataSize;
				 memcpy(pdata, (prcv_queue->pData + prcv_queue->ReadIndex) ,read_data);

				 prcv_queue->DataSize -= read_data;
				 prcv_queue->ReadIndex += read_data;
				if(!prcv_queue->DataSize)
					prcv_queue->State = QUEUE_STATE_IDLE; 
				 
				 result = read_data;
				 
				 ac_OS_ReleaseSemaphore(g_Sys_Manager.psOC_Sys[oc_handle]->Read_Semaphore);
			 }

             
		 }else
		   return 0;
	 }

    return result;

}

int32_t ac_OSCDC_FlushWrite(int32_t handle)
{
  	 uint32_t write_bytes = 0;
	 PurgeComm((HANDLE)handle, PURGE_TXCLEAR|PURGE_TXABORT);
	 return write_bytes;

}


int32_t ac_OSCDC_Write(int32_t handle, uint8_t *pdata,  uint16_t data_size)
{
  	 uint32_t write_bytes;
	 
	 WriteFile((HANDLE)handle, pdata, data_size, (LPDWORD)&write_bytes,0);
	
	 
	 if(!write_bytes)
		 {
	         
			 LPVOID lpMsgBuf;
           //  LPVOID lpDisplayBuf;
               DWORD dw = GetLastError(); 

        FormatMessage(
        FORMAT_MESSAGE_ALLOCATE_BUFFER | 
        FORMAT_MESSAGE_FROM_SYSTEM |
        FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        dw,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (LPTSTR) &lpMsgBuf,
        0, NULL );
			 DbgWriteText("WriteFile Failure: size = %d, written 0 , %s ", data_size, lpMsgBuf);


			 WriteFile((HANDLE)handle, pdata, data_size, (LPDWORD)&write_bytes,0);
	
			 //FlushFile((HANDLE)handle);
			 if(!write_bytes)
			 {
			   DbgWriteText("Second WriteFile Failure: size = %d, written 0 , %s ", data_size, lpMsgBuf);

			 }
	 }
	 return write_bytes;

}

int32_t ac_OS_KillRcvTask(uint32_t TaskID)
{
   //ExitThread(TaskID);
	return 0;
}
void ac_OS_Sleep(uint32_t MS)
{
   SwitchToThread();
	Sleep(MS);
}


uint32_t ac_OS_CreateSemaphore( uint8_t InitialCount, uint8_t MaximumCount, int8_t1 *pName)
{
   HANDLE handle=  CreateSemaphore(NULL,InitialCount,MaximumCount,pName);

   return (uint32_t)handle;
}

void ac_OS_CloseSemaphore(uint32_t SemaphoreID )
{
  
    CloseHandle((HANDLE)SemaphoreID);
}

uint32_t ac_OS_GetSemaphore(uint32_t SemaphoreID, uint32_t MS  )
{
  
    return WaitForSingleObject((HANDLE)SemaphoreID, MS);

}

void ac_OS_ReleaseSemaphore(uint32_t SemaphoreID )
{
  
	ReleaseSemaphore((HANDLE)SemaphoreID, 1, 0);

}

uint32_t ac_OS_CreateEvent( int8_t1 *pName)
{
	return(uint32_t)CreateEvent( 
         NULL,    // default security attribute 
         FALSE,    // manual-reset event 
         FALSE,    // initial state = signaled 
         NULL);   // unnamed event object 

}

int32_t  ac_OS_MDir(int8_t1 *Path)
{
   return CreateDirectory(Path,NULL);
}

void ac_OS_GetTimestemp(int8_t1 *pTimestamp)
{
        SYSTEMTIME st;
        GetLocalTime(&st);
#ifdef SAFE_STR
        sprintf_s(pTimestamp, TIMESTAMP_MAX_LEN, "%02d-%02d %02d:%02d:%02d.%03d",
               st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);
#else
        sprintf(pTimestamp, "%02d-%02d %02d:%02d:%02d.%03d",
               st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);
#endif
}