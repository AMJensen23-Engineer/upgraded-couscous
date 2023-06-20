
//*****************************************************************************
//
//  log.h - provide log functions.
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
//+ 2015-11-10 [RT] - Removed unnecessary include files 
//

#ifndef __LOG_H
#define __LOG_H
/**/
#define  MAX_ERRROR_STR_LEN   256 
extern int8_t1 g_LastErrStr[MAX_ERRROR_STR_LEN];
extern int8_t1 g_LastFirmwareErrStr[ MAX_SUPPORT_OC][MAX_ERRROR_STR_LEN];
extern STATUS g_LastFirmwareErrCode[MAX_SUPPORT_OC];

//#pragma once

//#include <stdio.h>
//#include "windows.h"

#define TIMESTAMP_MAX_LEN       30

#if 0
#define PROFILE_TIMES           0
#define MAX_BUFFERED_LINES		100

//#define LOG_DISABLED            0
//#define LOG_SINGLE_SESSION      1
//#define LOG_APPEND              2
//#define LOG_SYSDEBUG            4   // use OutputDebugString() instead of file

// IsLoggingEnabled is defined in the Windows include file UrlMon.h,
// which causes a conflict with our function definition.
#undef IsLoggingEnabled                   

typedef enum
{
    DEBUG_READ,
    DEBUG_WRITE
} DEBUG_RW;

class CLogging
{
public:
    CLogging(LPSTR szSuffix = "", int nSizeLimitKBytes = 250);
    ~CLogging(void);

    void SetID(unsigned id);
    void WriteText(char *msgfmt, ...);
    void WriteLine(char *msgfmt, ...);
    void WriteSysDebug(char *msgfmt, ...);
	void BufferedWriteLine(BOOL bUseElapsedTime, char *msgfmt, ...);
    void WriteBufferedLines();
	void PostWriteLine(char *msgfmt, ...);
    LPSTR GetTimestamp(int nUseElapsedTime = -1);
    LPSTR GetTimestamp(LPSTR szTimestamp);
    LPSTR GetLogPath(LPSTR szBuffer, int len);
    BOOL IsLoggingEnabled(void);
	BOOL IsSessionStarted();
    BOOL Enable(void);
    BOOL Disable(void);
    BOOL Suppress(BOOL state=TRUE);
    BOOL EnablePacketLogging(int state=TRUE);
    double StartProfileTimer(double time=0.0);
    void EnableProfileTimer(BOOL enable=TRUE);
    void WriteProfileTime(LPSTR szMsg=NULL);
    BOOL IsProfilingEnabled();
    double GetElapsedTime();

private:
    void InitDebugSettings();
    void WriteReadDebugSettings(DEBUG_RW mode);
    LPSTR GetFilename(LPSTR szBuffer, int len);
    void Reset(void);
    void LogElapsedTime(BOOL bLogET);

    BOOL OpenLogfile();
    void CloseLogfile();
    void InitializePath();
    void startTime();
    void WriteLog(LPSTR buffer, BOOL bTimestamp);

    char m_szLogPath[MAX_PATH];
    char m_szBackupFullName[MAX_PATH];
    char m_szArchiveFullName[MAX_PATH];
    char m_szFileName[MAX_PATH];
    char m_szFileNameSuffix[40];
    char m_szTimestamp[TIMESTAMP_MAX_LEN];
    int m_nSizeLimitKBytes;
    int m_nID;
    int m_nDebugSettings;
    BOOL m_bPathInitialized;
    BOOL m_bLoggingEnabled;
    BOOL m_bLoggingSuppressed;
    BOOL m_bSessionStarted;
    BOOL m_bAppendLog;
    BOOL m_bSysDebugLog;
    BOOL m_bPacketLogEnabled;
    BOOL m_bLogElapsedTime;
    BOOL m_bFirstWrite;
    BOOL m_bDebugSettingsAreDirty;
    FILE *m_fpLogFile;
    char *m_pSysDebugBuffer;
	LPSTR m_szBufferedWriteBuffer[MAX_BUFFERED_LINES];
    char  m_sLockFileName[128];
    int m_fdLock;
    double m_fProfileStartTime;
    BOOL m_bProfileTimerEnabled;

	int m_nBufferedWriteLines;
	int m_nBufferedWriteAttempts;
	//FILE * fLog;
#if PROFILE_TIMES
    double m_fLastTime;
#endif
};

#endif

#ifndef ASSERT
#ifdef DEBUG
#define ASSERT(expr) do                                                       \
                     {                                                        \
                         if(!(expr))                                          \
                         {                                                    \
                             __error__(__FILE__, __LINE__);                   \
                         }                                                    \
                     }                                                        \
                     while(0)
#else
#define ASSERT(expr)  
#endif
#endif
void LogFile_Init();
int32_t ocLog_SetLogFileName(const char *logfilename);
void ocSet_EnableLog(bool Enable);
void DbgWriteText(char *msgfmt, ...);
#endif