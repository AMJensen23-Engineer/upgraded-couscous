//*****************************************************************************
//
//  log.cpp - provide log functions.
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
#include "oc_stdint.h"



#include <stdarg.h>
#include <Shlobj.h>
#include <io.h>
#include <fcntl.h>
#include <sys/stat.h>

#include "acctrl.h"

#include "Log.h"


//#pragma warning (disable:4996)

/* globa variable*/
int8_t1 g_LastErrStr[256];
int8_t1 g_LastFirmwareErrStr[ MAX_SUPPORT_OC][MAX_ERRROR_STR_LEN];
STATUS g_LastFirmwareErrCode[MAX_SUPPORT_OC];
static bool   g_EnableLog = true;
static int8_t1 log_file_name[MAX_ERRROR_STR_LEN];
static FILE   *g_LogFilefp = NULL;

static LARGE_INTEGER timerFreq_;
static LARGE_INTEGER counterAtStart_;
static BOOL timerStarted = FALSE;
#define SUCCEEDED(hr) (((HRESULT)(hr)) >= 0)
#if PROFILE_TIMES
static double fLastTime = 0.0;
#endif

void GetTimestemp(int8_t1 *timestamp); 

//log out the message
void DbgWriteText(char *msgfmt, ...)
{
        char buffer[512];
		char output_buffer[1024];
		
		char timestamp[TIMESTAMP_MAX_LEN];
        va_list args;

#ifdef SAFE_STR
        va_start(args, msgfmt);
        vsprintf_s(buffer, sizeof(buffer), msgfmt, args);
        va_end(args);
		strcpy_s(g_LastErrStr, sizeof(g_LastErrStr), buffer);
#else
        va_start(args, msgfmt);
        vsprintf(buffer, msgfmt, args);
        va_end(args);
		strcpy(g_LastErrStr,buffer);
#endif
        GetTimestemp(timestamp); 
#ifdef SAFE_STR
		sprintf_s(output_buffer, sizeof(output_buffer), "%s:%s",timestamp, buffer);
#else
		sprintf(output_buffer, "%s:%s",timestamp, buffer);
#endif
    
		if(g_EnableLog && g_LogFilefp)
        {
    		fprintf(g_LogFilefp, output_buffer);
            fflush(g_LogFilefp);
        }
		
 
}
void ocSet_EnableLog(bool Enable)
{
	g_EnableLog = Enable;
}

//intialize to open the log file
void LogFile_Init()
{
    char m_szLogPath[1024];
#ifdef _WIN32 
	
	
	  g_LastErrStr[0] = 0;
    for(int i =0; i< MAX_SUPPORT_OC; i++)
	   g_LastFirmwareErrStr[i][0] = 0;
	
	if(SUCCEEDED(SHGetFolderPath(NULL, 
                            CSIDL_PERSONAL|CSIDL_FLAG_CREATE, 
                            NULL, 0, m_szLogPath))) 
    {
#ifdef SAFE_STR
        strcat_s(m_szLogPath, sizeof(m_szLogPath), "\\OneCtrl");
#else
        strcat(m_szLogPath, "\\OneCtrl");
#endif
        
		g_Sys_Manager.ocSysM_MDir(m_szLogPath);
#ifdef SAFE_STR
		strcat_s(m_szLogPath, sizeof(m_szLogPath), "\\Logs");
#else
		strcat(m_szLogPath, "\\Logs");
#endif
        g_Sys_Manager.ocSysM_MDir(m_szLogPath);
		 
		
	   
	}
	
#ifdef SAFE_STR
	strcat_s(m_szLogPath, sizeof(m_szLogPath), "\\onecontroller.log");
#else
	strcat(m_szLogPath, "\\onecontroller.log");
#endif

#ifdef SAFE_STR
    fopen_s(&g_LogFilefp, m_szLogPath,"wt");
#else
    g_LogFilefp = fopen(m_szLogPath,"wt");
#endif
#endif
}

//open the log file
FILE *openLogFile(int8_t1 *logFileName)
{
   	FILE *fp;
	char output_buffer[1024];
		
	char timestamp[TIMESTAMP_MAX_LEN];
	
	if(logFileName)
#ifdef SAFE_STR
		fopen_s(&fp, logFileName, "wt");
#else
		fp = fopen(logFileName, "wt");
#endif
	if(fp)
       g_LogFilefp = fp;
	else
       {  
		   GetTimestemp(timestamp); 
#ifdef SAFE_STR
		   sprintf_s(output_buffer, sizeof(output_buffer), "%s: Cannot open log file: %s",timestamp, logFileName);
           strcpy_s(g_LastErrStr, sizeof(g_LastErrStr), output_buffer);
#else
		   sprintf(output_buffer, "%s: Cannot open log file: %s",timestamp, logFileName);
           strcpy(g_LastErrStr,output_buffer);
#endif
	   }
		return fp;
}

void CloseLogFile(FILE *logFilefp)
{
    if(logFilefp)
	 fclose(logFilefp);
}

//the user sets the log file name
int32_t ocLog_SetLogFileName(const char *logfilename )
{
     int32_t res;
	if(logfilename)
#ifdef SAFE_STR
	   strcpy_s(log_file_name, sizeof(log_file_name), logfilename);
#else
	   strcpy(log_file_name,logfilename);
#endif
	else res = -1;

	CloseLogFile(g_LogFilefp);
	g_LogFilefp = openLogFile(log_file_name);
	if(g_LogFilefp)
	return 0;
	else return -1;
}


static void GetTimestemp(int8_t1 *pTimestamp)
{
    g_Sys_Manager.ocSysM_GetTimestemp(pTimestamp);
}


void startTime()
{
    timerStarted = TRUE;
    QueryPerformanceFrequency(&timerFreq_);
    QueryPerformanceCounter(&counterAtStart_);
}

double GetElapsedTime()
{
    if (timerFreq_.QuadPart != 0)
    {
        LARGE_INTEGER curTime;
        QueryPerformanceCounter(&curTime);
        return (double)(curTime.QuadPart - counterAtStart_.QuadPart) / timerFreq_.QuadPart;
    }

    return -1.0;
}

int ReadSetting(LPSTR szSection, LPSTR szKey, int nDefault)
{
//    char temp[40];
 //   LPSTR szTemp = ReadSetting(szSection, szKey, _itoa(nDefault, temp, 10));
    return 1;
}

void WriteSetting(LPSTR szSection, LPSTR szKey, LPSTR szValue)
{
   // InitSettingsPath();
    //WritePrivateProfileStringA(szSection, szKey, szValue, szIniFilePath);
}


static void CreateDir(char* Path)
{
	// thanks to Birender Singh 
	// http://www.codeproject.com/Articles/10606/Folder-Utility-Create-Path-Remove-Folder-Remove-Al

	char DirName[MAX_PATH];
	char* p = Path;
	char* q = DirName; 

	while(*p)
	{
		if (('\\' == *p) || ('/' == *p))
		{
			if (':' != *(p-1))
			{
				CreateDirectory(DirName, NULL);
			}
		}
		*q++ = *p++;
		*q = '\0';
	}

	CreateDirectory(DirName, NULL);
}

#if 0
CLogging::CLogging(LPSTR szSuffix, int nSizeLimitKBytes)
{
    m_nDebugSettings = 0;
    m_bPathInitialized = FALSE;
    m_bSessionStarted = FALSE;
    m_bLoggingEnabled = FALSE;
    m_bLoggingSuppressed = FALSE;
    m_nSizeLimitKBytes = nSizeLimitKBytes * 1024;
    m_bLogElapsedTime = FALSE;
    m_bAppendLog = FALSE;
    m_bSysDebugLog = FALSE;
    m_bFirstWrite = TRUE;
    m_nID = 0;
	m_nBufferedWriteLines = 0;
    m_nBufferedWriteAttempts = 0;
    m_fpLogFile = NULL;
    m_pSysDebugBuffer = NULL;
    m_fdLock = -1;
    m_fProfileStartTime = 0.0;
    m_bProfileTimerEnabled = FALSE;
    m_bDebugSettingsAreDirty = FALSE;

    *m_szTimestamp = 0;
	memset(m_szBufferedWriteBuffer, 0, sizeof(m_szBufferedWriteBuffer));

    strcpy(m_szFileNameSuffix, szSuffix);
	
    if (!timerStarted)
        startTime();

#if PROFILE_TIMES
    m_fLastTime = GetElapsedTime();
#endif
}

CLogging::~CLogging(void)
{
    static DWORD dwErr;

    if (m_bSessionStarted)
    {
        GetTimestamp(FALSE);
        WriteText("##################### SESSION END  %s #####################\n", m_szTimestamp);
    }

    CloseLogfile();

    if (m_fdLock != -1)
    {
        _close(m_fdLock);
        m_fdLock = -1;
        DeleteFile(m_sLockFileName);
        dwErr = GetLastError();
    }

    if (m_pSysDebugBuffer != NULL)
        free(m_pSysDebugBuffer);

    WriteReadDebugSettings(DEBUG_WRITE);
}

void CLogging::SetID(unsigned id)
{
    m_nID = id;
}

void CLogging::LogElapsedTime(BOOL bLogET)
{
    m_bLogElapsedTime = bLogET;
}

void CLogging::startTime()
{
    timerStarted = TRUE;
    QueryPerformanceFrequency(&timerFreq_);
    QueryPerformanceCounter(&counterAtStart_);
}

double CLogging::GetElapsedTime()
{
    if (timerFreq_.QuadPart != 0)
    {
        LARGE_INTEGER curTime;
        QueryPerformanceCounter(&curTime);
        return (double)(curTime.QuadPart - counterAtStart_.QuadPart) / timerFreq_.QuadPart;
    }

    return -1.0;
}


LPSTR CLogging::GetFilename(LPSTR szBuffer, int len)
{
    lstrcpyn(szBuffer, m_szFileName, len);
    return szBuffer;
}

LPSTR CLogging::GetLogPath(LPSTR szBuffer, int len)
{
    lstrcpyn(szBuffer, m_szLogPath, len);
    return szBuffer;
}

BOOL CLogging::IsSessionStarted()
{
	return m_bSessionStarted;
}

void CLogging::InitializePath()
{
    if (m_bPathInitialized)
        return;

    if(SUCCEEDED(SHGetFolderPath(NULL, 
                            CSIDL_PERSONAL|CSIDL_FLAG_CREATE, 
                            NULL, 0, m_szLogPath))) 
    {
        strcat(m_szLogPath, "OneCtrl\\Logs");
        CreateDirectory(m_szLogPath, NULL);
    }


	strcat(m_szLogPath, "\\onecontroller.log");
    m_fpLogFile = fopen(m_szLogPath,"wt");
	if(m_fpLogFile)
      m_bPathInitialized = TRUE;
}

BOOL CLogging::OpenLogfile()
{
    // do this only once, at startup
    if (!m_bSessionStarted && !m_bSysDebugLog)
    {
        if (!m_bPathInitialized)
            InitializePath();

#if 0

        CloseLogfile();

        CFileStatus filestatus;
        CFile::GetStatus(m_szBackupFullName, filestatus);

        if (filestatus.m_size > m_nSizeLimitKBytes)
        {
            // if archived log already exists, delete it
            if (CFile::GetStatus(m_szArchiveFullName, filestatus) != 0)
                _unlink(m_szArchiveFullName);

            // move the backup log to the archive folder
            rename(m_szBackupFullName, m_szArchiveFullName);
        }

        // if a log file already exists, append it to the backup log
        if (CFile::GetStatus(m_szFileName, filestatus) != 0)
        {
            FILE *fpSrc;
            FILE *fpDst;

            if ((fpSrc = fopen(m_szFileName, "rt")) != NULL)
            {
                if ((fpDst = fopen(m_szBackupFullName, "at")) != NULL)
                {
                    char buf[10240];
                    size_t size;

                    while ((size = fread(buf, 1, sizeof(buf), fpSrc)) > 0)
                        fwrite(buf, 1, size, fpDst);

                    fclose(fpDst);
                }

                fclose(fpSrc);
                _unlink(m_szFileName);
            }
        }

        if (m_bProfileTimerEnabled || m_bPacketLogEnabled)
        {
            char msg[1000];
            
            wsprintf(msg, "NOTE: The following debugging option%s enabled and may slightly affect performance:\r\n", 
                (m_bProfileTimerEnabled && m_bPacketLogEnabled) ? "s are" : " is");

            if (m_bProfileTimerEnabled)
                lstrcat(msg, "\r\n    - API function profiling");

            if (m_bPacketLogEnabled)
                lstrcat(msg, "\r\n    - Communications packet logging");

            MessageBox(GetFocus(), msg, "USB2ANY Performance Warning", MB_OK | MB_ICONWARNING);
        }
    }

    if (!m_bSysDebugLog)
	    m_fpLogFile = fopen(m_szFileName, "at");

    if (!m_bSessionStarted && (m_fpLogFile != NULL || m_bSysDebugLog))
    {
        m_bSessionStarted = TRUE;
        GetTimestamp(FALSE);

        WriteBufferedLines();
        WriteText(" \n");

        WriteReadDebugSettings(DEBUG_WRITE);

        //if (m_fpLogFile == NULL)
	       // m_fpLogFile = fopen(m_szFileName, "at");
    }

#endif
}
	return (m_fpLogFile != NULL || m_bSysDebugLog);
}

void CLogging::CloseLogfile()
{
    if (m_fpLogFile != NULL)
    {
        fclose(m_fpLogFile);
        m_fpLogFile = NULL;
    }
}

BOOL CLogging::IsLoggingEnabled(void)
{
    InitDebugSettings();
    return m_bLoggingEnabled;
}

#define SETTINGS_COUNT      6

void CLogging::WriteReadDebugSettings(DEBUG_RW mode)
{
#if 0
    if (mode == DEBUG_READ)
    {
        int count;

        m_nDebugSettings    = 1;
        m_bLoggingEnabled   = ReadSetting(m_szFileNameSuffix, "DebugLogging", 1) != 0;
        m_bAppendLog        = ReadSetting(m_szFileNameSuffix, "AppendDebugLog", 0) != 0;
        m_bSysDebugLog      = ReadSetting(m_szFileNameSuffix, "UseSystemDebugLog", 0) != 0;
        m_bProfileTimerEnabled = ReadSetting(m_szFileNameSuffix, "Profiling", 0) != 0;
        m_bPacketLogEnabled = ReadSetting(m_szFileNameSuffix, "PacketLogging", 0);
        count               = ReadSetting(m_szFileNameSuffix, "Settings", 0); // total number of settings, including this one

        if (count != SETTINGS_COUNT)     // only once, or when number of settings changes
            m_bDebugSettingsAreDirty = TRUE;
    }
    else if (mode == DEBUG_WRITE && m_bDebugSettingsAreDirty)
    {
        WriteSetting(m_szFileNameSuffix, "Settings", SETTINGS_COUNT);   // total number of settings, including this one
        WriteSetting(m_szFileNameSuffix, "DebugLogging", m_bLoggingEnabled);
        WriteSetting(m_szFileNameSuffix, "AppendDebugLog", m_bAppendLog);
        WriteSetting(m_szFileNameSuffix, "UseSystemDebugLog", m_bSysDebugLog);
        WriteSetting(m_szFileNameSuffix, "Profiling", m_bProfileTimerEnabled);
        WriteSetting(m_szFileNameSuffix, "PacketLogging", m_bPacketLogEnabled);
        m_bDebugSettingsAreDirty = FALSE;
    }
#endif
}

void CLogging::InitDebugSettings()
{
    if (m_nDebugSettings == 0)  // only once per session
    {            
        WriteReadDebugSettings(DEBUG_READ);
        if (!m_bSessionStarted)
        {
            BufferedWriteLine(FALSE, "DebugLogging is %s", m_bLoggingEnabled ? "Enabled" : "Disabled");
            BufferedWriteLine(FALSE, "PacketLogging is %s", m_bPacketLogEnabled ? "Enabled" : "Disabled");
            BufferedWriteLine(FALSE, "Profiling is %s", m_bProfileTimerEnabled ? "Enabled" : "Disabled");
        }

        if (m_bSysDebugLog && m_pSysDebugBuffer == NULL)
            m_pSysDebugBuffer = (char *) malloc(1024);
    }
}

BOOL CLogging::EnablePacketLogging(int state/*=TRUE*/)
{
    InitDebugSettings();    // sets initial state of m_bPacketLogEnabled
    BOOL bPrevState = m_bPacketLogEnabled;

    if (state != -1)      // -1 means "read, but don't change"
    {
        m_bPacketLogEnabled = (state != 0);  // only zero means FALSE

	    if (bPrevState != m_bPacketLogEnabled)
	    {
		    if (m_bSessionStarted)
			    WriteLine("Packet logging %s.", state ? "enabled" : "disabled");
		    else
			    BufferedWriteLine(FALSE, "Packet logging %s.", state ? "enabled" : "disabled");
	    }
    }

	return bPrevState;
}


BOOL CLogging::Suppress(BOOL state)
{
    BOOL prevState = m_bLoggingSuppressed;

    m_bLoggingSuppressed = state;
    return prevState;
}

BOOL CLogging::Enable(void)
{
    InitDebugSettings();
    BOOL bPrevState = m_bLoggingEnabled;

    m_bLoggingEnabled = TRUE;

	if (bPrevState != m_bLoggingEnabled)
	{
		if (m_bSessionStarted)
			WriteLine("Logging enabled.");
		else
			BufferedWriteLine(FALSE, "Logging enabled.");
	}

	return bPrevState;
}

BOOL CLogging::Disable(void)
{
    InitDebugSettings();
    BOOL bPrevState = m_bLoggingEnabled;

    m_bLoggingEnabled = FALSE;
	if (bPrevState != m_bLoggingEnabled)
	{
		if (m_bSessionStarted)
			WriteLine("Logging disabled.");
		else
			BufferedWriteLine(FALSE, "Logging disabled.");
	}

    return bPrevState;
}

void CLogging::Reset(void)
{
    CloseLogfile();
    OpenLogfile();
}

//
// returns a timestamp in the format: 2012-10-06 10:06:41.517
//
LPSTR CLogging::GetTimestamp(int nUseElapsedTime)  
{
    if (nUseElapsedTime == -1)
        nUseElapsedTime = m_bLogElapsedTime;

    if (nUseElapsedTime)
    {
#if PROFILE_TIMES
        double fCurTime = GetElapsedTime();

        sprintf(m_szTimestamp, "%10.04f", fCurTime - m_fLastTime);
        m_fLastTime = fCurTime;
#else
        sprintf(m_szTimestamp, "%10.04f", GetElapsedTime());
#endif
    }
    else
    {
        SYSTEMTIME st;

        GetLocalTime(&st);
            sprintf(m_szTimestamp, "%4d-%02d-%02d %02d:%02d:%02d.%03d",
            st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);
    }

    return m_szTimestamp;
}

LPSTR CLogging::GetTimestamp(LPSTR szTimestamp)  
{
    GetTimestamp(-1);
    strcpy(szTimestamp, m_szTimestamp);
    return szTimestamp;
}

void CLogging::WriteLog(LPSTR buffer, BOOL bTimestamp)
{
    InitDebugSettings();
    if (m_bLoggingEnabled && !m_bLoggingSuppressed && OpenLogfile())
    {
        char prefix[40] = "";

        if (m_nID > 0)
            sprintf(prefix, "%d ", m_nID);

        if (bTimestamp)
        {
            strcat(prefix, GetTimestamp());
            strcat(prefix, " ");
        }

        if (m_bSysDebugLog)
        {
            sprintf(m_pSysDebugBuffer, "%s%s", prefix, buffer);
            WriteSysDebug(m_pSysDebugBuffer);
        }
        else if (m_fpLogFile != NULL)
        {
            fprintf(m_fpLogFile, "%s%s", prefix, buffer);
            CloseLogfile();
        }

        //if (m_bFirstWrite)
        //{
        //    m_bFirstWrite = FALSE;
        //    WriteLog(" ", TRUE);
        //}
    }
}

void CLogging::WriteText(char *msgfmt, ...)
{
    InitDebugSettings();
    if (m_bLoggingEnabled && !m_bLoggingSuppressed)
    {
        char buffer[1024];
        va_list args;

        va_start(args, msgfmt);
        vsprintf(buffer, msgfmt, args);
        va_end(args);

        WriteLog(buffer, TRUE);
    }
}

void CLogging::WriteLine(char *msgfmt, ...)
{
    InitDebugSettings();
    if (m_bLoggingEnabled && !m_bLoggingSuppressed)
    {
        char buffer[1024];
        va_list args;

        va_start(args, msgfmt);
        vsprintf(buffer, msgfmt, args);
        va_end(args);

        strcat(buffer, "\n");
        WriteLog(buffer, TRUE);
    }
}

void CLogging::BufferedWriteLine(BOOL bUseElapsedTime, char *msgfmt, ...)
{
    if (m_bLoggingEnabled && !m_bLoggingSuppressed)
    {
        ++m_nBufferedWriteAttempts;
        if (m_nBufferedWriteLines < MAX_BUFFERED_LINES)
        {
            char buffer[1024];
            char *pBuffer = buffer;
            va_list args;

            if (bUseElapsedTime)
                sprintf(pBuffer, "%10.04f ", GetElapsedTime());
            else
                sprintf(pBuffer, "%s ", GetTimestamp());

            pBuffer = strchr(buffer, 0);    // advance pBuffer past timestamp

            va_start(args, msgfmt);
            vsprintf(pBuffer, msgfmt, args);
            va_end(args);

            strcat(pBuffer, "\n");

            // make a copy of the full buffer (including timestamp)
		    m_szBufferedWriteBuffer[m_nBufferedWriteLines++] = _strdup(buffer); 
        }
    }
}

void CLogging::WriteBufferedLines()
{
    for (int i = 0; m_nBufferedWriteLines; ++i)
    {
        WriteLog(m_szBufferedWriteBuffer[i], FALSE);
	    free(m_szBufferedWriteBuffer[i]);
	    --m_nBufferedWriteLines;
    }

    m_nBufferedWriteAttempts = 0;
}

double CLogging::StartProfileTimer(double time/*=0.0*/)
{
    if (m_bProfileTimerEnabled)
    {
        m_fProfileStartTime = (time != 0.0) ? time : GetElapsedTime();
    }

    return m_fProfileStartTime;
}

BOOL CLogging::IsProfilingEnabled()
{
    return m_bProfileTimerEnabled;
}

void CLogging::EnableProfileTimer(BOOL enable/*=TRUE*/)
{
    m_bProfileTimerEnabled = enable;
    m_fProfileStartTime = 0.0;
}

void CLogging::WriteProfileTime(LPSTR szMsg/*=NULL*/)
{
    if (m_fProfileStartTime > 0.0)
    {
        double fProfileTime = GetElapsedTime() - m_fProfileStartTime;

        if (szMsg == NULL || *szMsg == 0)
            WriteText("  Elapsed time: %10.05f ms.\n", fProfileTime * 1000.0);
        else
            WriteText("MSG: %s (ET=%10.05f ms.)\n", szMsg, fProfileTime * 1000.0);
    }
}

void CLogging::WriteSysDebug(char *msgfmt, ...)
{
    char m_pSysDebugBuffer[1024];
//    if (m_pSysDebugBuffer != NULL)
    {
        va_list args;

        va_start(args, msgfmt);
        vsprintf(m_pSysDebugBuffer, msgfmt, args);
        va_end(args);

        strcat(m_pSysDebugBuffer, "\r\n");
        OutputDebugString(m_pSysDebugBuffer);
    }
}
#endif