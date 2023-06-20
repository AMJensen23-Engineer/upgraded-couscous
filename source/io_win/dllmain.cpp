// dllmain.cpp : Defines the entry point for the DLL application.
#include "windows.h"
#include <stdio.h>
#include "acctrl.h"
#include "ac_monitor.h"

extern char szClassName[ ];
acAPI ac_Sys_Get_PIN_MAP(char *map_file_path);
/* RT 01-07-16: Added ENABLE_VIA_MONITOR to allow device notifications
to be disabled while debugging */
#define ENABLE_VIA_MONITOR      1

HINSTANCE g_hDll;
DWORD id;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



LPSTR GetVersionInfo(char *szValue, char *szBuffer, ULONG nLength, char *szSrcFileName)
{
	LPSTR szRet = NULL;
	char szFileName[MAX_PATH] = "";

    if (szSrcFileName == NULL || *szSrcFileName == 0)
    {
        if (!GetModuleFileName(NULL, szFileName, sizeof(szFileName)))
        {
            return NULL;
        }
    }
    else
    {
        lstrcpy(szFileName, szSrcFileName);
    }

	UINT len = GetFileVersionInfoSize(szFileName, 0);
	szRet = NULL;
	LPVOID ver = LocalAlloc(LPTR, len);

	if (ver != NULL)
	{
		DWORD *codepage;
		char fmt[256];
		PVOID ptr = 0;

		if (GetFileVersionInfo(szFileName, 0, len, ver))
		{
			if (VerQueryValue(ver, "\\VarFileInfo\\Translation", (LPVOID *) &codepage, (PUINT) &len))
			{
				wsprintf(fmt, "\\StringFileInfo\\%08x\\%s", (*codepage) << 16 | (*codepage) >> 16, szValue);
				if (VerQueryValue(ver, fmt, &ptr, &len))
				{
					lstrcpyn(szBuffer, (char *)ptr, min(nLength, len));
					szRet = szBuffer;
				}
			}
		}
		LocalFree(ver);
	}

	return szRet;
}

void DebugLog(char *format, ...)
{
    char buffer[1024];
    va_list args;

    va_start(args, format);
    vsprintf_s(buffer,  sizeof(buffer), format, args);
    if (strchr(buffer, '\n') == NULL)
        strcat_s(buffer, sizeof(buffer), "\r\n");

    OutputDebugString(buffer);
    va_end(args);
}

void GetInfo()
{
    char szModule[MAX_PATH] = "";
    char szTemp[MAX_PATH];
    char sParentName[MAX_PATH];

    GetModuleFileName(g_hDll, szModule, MAX_PATH);

    // get the full path of the program that loaded this DLL
    GetModuleFileName(NULL, sParentName, MAX_PATH);
    GetVersionInfo("ProductVersion", szTemp, sizeof(szTemp), szModule);

    DebugLog("");
    DebugLog("################################  SESSION START  ################################");
    DebugLog("Loading %s (v%s)", szModule, szTemp);
    DebugLog("Loaded by %s)", sParentName);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


BOOL oc_UnRegisterForDeviceNotification(HDEVNOTIFY* diNotifyHandle);
extern HDEVNOTIFY diNotifyHandle;
BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		  g_hDll = (HINSTANCE)hModule; 
#if 0 
		  if(FindWindow(szClassName, "OneController_DLL"))
		  {
		     MessageBox(NULL, "OneController DLL is already loaded","Error",MB_OK); 
			  return FALSE;
		  }
#endif	

          GetInfo();
		  LogFile_Init();

		  {
		    char szModule[MAX_PATH] = "";
            GetModuleFileName(hModule, szModule, MAX_PATH);
		    char *p_path= strrchr(szModule,'\\');
			if(p_path)
              { *(p_path + 1) = 0;
			  if(ac_Sys_Get_PIN_MAP(szModule)!=STAT_OK)
				  return false;
			  }
		  }
		  g_Sys_Manager.ocSysM_Init();
#if ENABLE_VIA_MONITOR
		  CreateThread(NULL,0,VIA_Monitor,NULL,0,&id);
#endif
          break;
	case DLL_THREAD_ATTACH:
		  break;
	case DLL_THREAD_DETACH:
		  break;
	case DLL_PROCESS_DETACH:
#if ENABLE_VIA_MONITOR
	     oc_UnRegisterForDeviceNotification(&diNotifyHandle);
#endif
		g_Sys_Manager.ocSysM_UnInit();
		  break;
	}
	return TRUE;
}

