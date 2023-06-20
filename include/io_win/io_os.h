//*****************************************************************************
//
// io_os.h - Defines OS depend API.
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
#ifndef __IO_OS_H
#define __IO_OS_H
//*****************************************************************************
//
// Prototypes for the APIs.
//
//*****************************************************************************


#ifdef  __cplusplus
extern "C" {
#endif

int32_t ac_OSCDC_FindACControllers();
int32_t ac_OSCDC_GetComPortNumber(uint8_t ACControllerIndex);
STATUS ac_OSCDC_GetComPortFromSerial(const char *szSerialNum);
STATUS ac_OSCDC_OpenComPort(int32_t port);
STATUS ac_OSCDC_CloseComPort(int32_t hComm);
AC_HANDLE ac_OSCDC_Open(uint8_t ComPortNum);
STATUS ac_OSCDC_Close(AC_HANDLE Handle);
int32_t ac_OSCDC_RegsterIfReadQueue(uint16_t IF);
int32_t ac_OSCDC_ReadAsync(int32_t Handle, uint16_t IF,uint8_t *pdata, uint8_t data_size , void (*DataReadyCallback)( uint8_t *pData, uint16_t DataSize));
int32_t ac_OSCDC_ReadSync(int32_t Handle, uint16_t IF, uint8_t *pData, uint8_t DataSize , uint8_t Timeout);
int32_t ac_OSCDC_Write(int32_t Handle, uint8_t *pData,  uint16_t DataSize);
int32_t ac_OSCDC_FlushWrite(int32_t handle);
int32_t ac_OS_KillRcvTask(uint32_t TaskID);
void ac_OS_Sleep(uint32_t MS);
uint32_t ac_OS_CreateSemaphore( uint8_t InitialCount, uint8_t MaximumCount, int8_t1 *pName);
void ac_OS_CloseSemaphore(uint32_t SemaphoreID );
uint32_t ac_OS_GetSemaphore(uint32_t SemaphoreID, uint32_t MS  );
void ac_OS_ReleaseSemaphore(uint32_t SemaphoreID );
uint32_t ac_OS_CreateEvent( int8_t1 *Name);
int32_t  ac_OS_MDir(int8_t1 *path);
void ac_OS_GetTimestemp(int8_t1 *pTimestamp);

#ifdef  __cplusplus
}
#endif

#endif 