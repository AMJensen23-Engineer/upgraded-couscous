//*****************************************************************************
//
// onectrl_monitor.cpp - monitor OneController plugin status.
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

#include <windows.h>
#include <Dbt.h>
#include <setupapi.h>

#include "acctrl.h"
#include "acAPI_System.h"



void (*g_pfSysNotify)(uint32_t EvenID, void *pData, uint16_t DataSize) = NULL; 
DWORD WINAPI  VIA_Monitor(LPVOID lpParam);
HDEVNOTIFY diNotifyHandle;
extern HINSTANCE g_hDll;

STATUS ResetPowerStatus(HANDLE handle);    // This is in ocAPI_Power.cpp

void OnDeviceChange(UINT nEventType, DWORD dwData)
{
}


void acSet_Notify(void(* SysEventCallback)( uint32_t EventID,  void *pData, uint16_t DataSize))
{
  g_pfSysNotify = SysEventCallback;
}

//******************************************************************************

/* A5DCBF10-6530-11D2-901F-00C04FB951ED */
DEFINE_GUID(GUID_DEVINTERFACE_USB_DEVICE, 0xA5DCBF10L, 0x6530, 0x11D2, 0x90, 0x1F, 0x00, \
             0xC0, 0x4F, 0xB9, 0x51, 0xED);

BOOL RegisterForDeviceNotification(HWND hWnd, HDEVNOTIFY* diNotifyHandle)
{
    DEV_BROADCAST_DEVICEINTERFACE broadcastInterface;
    GUID test = { 0xA5DCBF10, 0x6530, 0x11D2, { 0x90, 0x1F, 0x00, 0xC0, 0x4F, 0xB9, 0x51, 0xED }};
    broadcastInterface.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
    broadcastInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
    broadcastInterface.dbcc_classguid = test;

    *diNotifyHandle = RegisterDeviceNotification(hWnd, &broadcastInterface, DEVICE_NOTIFY_WINDOW_HANDLE);

    return (NULL != diNotifyHandle);
}
static LRESULT CALLBACK HookProcGetMessage(int nCode, WPARAM wParam, LPARAM lParam)
{
    if (nCode >= 0)
    {
        LPMSG msg = (LPMSG) lParam;

        if (msg->message == WM_DEVICECHANGE)
            OnDeviceChange((UINT)msg->wParam, (DWORD)msg->lParam);
    }

    LRESULT nextRet = CallNextHookEx(NULL, nCode, wParam, lParam);
    return nextRet;
}


/*  Declare Windows procedure  */
LRESULT CALLBACK WindowProcedure (HWND, UINT, WPARAM, LPARAM);

/*  Make the class name into a global variable  */
char szClassName[ ] = "ACControll_Sig";



DWORD WINAPI VIA_Monitor(LPVOID lpParam)
{
    HWND hwnd;               /* This is the handle for our window */
    MSG messages;            /* Here messages to the application are saved */
    WNDCLASSEX wincl;        /* Data structure for the windowclass */

    /* The Window structure */
    wincl.hInstance = g_hDll;
    wincl.lpszClassName =(LPCSTR)szClassName;
    wincl.lpfnWndProc = WindowProcedure;      /* This function is called by windows */
    wincl.style = CS_DBLCLKS;                 /* Catch double-clicks */
    wincl.cbSize = sizeof (WNDCLASSEX);

    /* Use default icon and mouse-pointer */
    wincl.hIcon = LoadIcon (NULL, IDI_APPLICATION);
    wincl.hIconSm = LoadIcon (NULL, IDI_APPLICATION);
    wincl.hCursor = LoadCursor (NULL, IDC_ARROW);
    wincl.lpszMenuName = NULL;                 /* No menu */
    wincl.cbClsExtra = 0;                      /* No extra bytes after the window class */
    wincl.cbWndExtra = 0;                      /* structure or the window instance */
    /* Use Windows's default colour as the background of the window */
    wincl.hbrBackground = (HBRUSH) COLOR_BACKGROUND;

    /* Register the window class, and if it fails quit the program */
    if (!RegisterClassEx (&wincl))
        return 0;

    /* The class is registered, let's create the program*/
    hwnd = CreateWindowEx (
           0,                   /* Extended possibilites for variation */
           (LPCSTR)szClassName,         /* Classname */
           (LPCSTR)"ACController_DLL",       /* Title Text */
           WS_OVERLAPPEDWINDOW, /* default window */
           CW_USEDEFAULT,       /* Windows decides the position */
           CW_USEDEFAULT,       /* where the window ends up on the screen */
           0,                 /* The programs width */
           0,                 /* and height in pixels */
           HWND_DESKTOP,        /* The window is a child-window to desktop */
           NULL,                /* No menu */
           g_hDll,       /* Program Instance handler */
           NULL                 /* No Window Creation data */
           );

    /* Make the window visible on the screen */
    ShowWindow (hwnd, 0);
	 DEV_BROADCAST_DEVICEINTERFACE broadcastInterface;
    GUID test = { 0xA5DCBF10, 0x6530, 0x11D2, { 0x90, 0x1F, 0x00, 0xC0, 0x4F, 0xB9, 0x51, 0xED }};
    broadcastInterface.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
    broadcastInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
    broadcastInterface.dbcc_classguid = test;

    diNotifyHandle =RegisterDeviceNotification(hwnd, &broadcastInterface, DEVICE_NOTIFY_WINDOW_HANDLE);

    /* Run the message loop. It will run until GetMessage() returns 0 */
    while (GetMessage (&messages, NULL, 0, 0))
    {
        /* Translate virtual-key messages into character messages */
        TranslateMessage(&messages);
        /* Send message to WindowProcedure */
        DispatchMessage(&messages);
    }
	
    /* The program return-value is 0 - The value that PostQuitMessage() gave */
    return (DWORD)messages.wParam;
}


static char *FindSN(char *dbcc_name)
{
    static const char *key = "USB#VID_0400&PID_2014#";
    static char serialBuf[64];
    char *p = strstr(dbcc_name, key);

    *serialBuf = 0;         // start with no serial number
    if (NULL != p)          // if key string was found...
    {
        p += strlen(key);   // point to the serial number
        char *serial_end;
		
		serial_end = strchr(p,'#');

	    if (serial_end)    
        {
            *serial_end = 0;       // terminate the string
            strcpy_s(serialBuf, sizeof(serialBuf), p);
        }
    }

    return serialBuf;
}


/*  This function is called by the Windows function DispatchMessage()  */

LRESULT CALLBACK WindowProcedure (HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)                  /* handle the messages */
    {
    case  WM_DEVICECHANGE :


        if ((wParam == DBT_DEVICEARRIVAL))
        {
            PDEV_BROADCAST_HDR pDevBrdcstHdr = (PDEV_BROADCAST_HDR) lParam;
            //  UnregisterDeviceNotification(&diNotifyHandle);
            int dbccSize = pDevBrdcstHdr->dbch_size;
            int devType = pDevBrdcstHdr->dbch_devicetype;

            if (devType == DBT_DEVTYP_DEVICEINTERFACE)
            {
                PDEV_BROADCAST_DEVICEINTERFACE pDevBrdcstDevIntf = (PDEV_BROADCAST_DEVICEINTERFACE) pDevBrdcstHdr;

                if(strstr(pDevBrdcstDevIntf->dbcc_name,ACCONTROLLER_USB_ID_RESET) || strstr(pDevBrdcstDevIntf->dbcc_name,ACCONTROLLER_USB_ID_RESET_XP))
                {
                    Sleep(500);

                    //notify user application
                    if(g_pfSysNotify)
                    {
                        char *serialNumber = FindSN(pDevBrdcstDevIntf->dbcc_name);

                        g_pfSysNotify(ACCONTROLLER_PLUG, serialNumber, (uint16_t)strlen(serialNumber));
                    //    g_pfSysNotify(ONECONTROLLER_PLUG,pDevBrdcstDevIntf->dbcc_name,(uint16_t)strlen(pDevBrdcstDevIntf->dbcc_name));
                    }

                }

            }
        }



        if(wParam == DBT_DEVICEREMOVECOMPLETE)
        {
            PDEV_BROADCAST_HDR pDevBrdcstHdr = (PDEV_BROADCAST_HDR) lParam;
            int devType = pDevBrdcstHdr->dbch_devicetype;


            if (devType == DBT_DEVTYP_DEVICEINTERFACE)
            {
                PDEV_BROADCAST_DEVICEINTERFACE pDevBrdcstDevIntf = (PDEV_BROADCAST_DEVICEINTERFACE) pDevBrdcstHdr;

                if(strstr(pDevBrdcstDevIntf->dbcc_name, ACCONTROLLER_USB_ID_RESET) || strstr(pDevBrdcstDevIntf->dbcc_name,ACCONTROLLER_USB_ID_RESET_XP))
                {
                    //noytify the user and fix bug 511
                    if(g_pfSysNotify)
                    {
                        char *serialNumber = FindSN(pDevBrdcstDevIntf->dbcc_name);

                        g_pfSysNotify(ACCONTROLLER_UNPLUG, serialNumber, (uint16_t)strlen(serialNumber));
                    //    g_pfSysNotify(ONECONTROLLER_UNPLUG,pDevBrdcstDevIntf->dbcc_name,(uint16_t)strlen(pDevBrdcstDevIntf->dbcc_name));
                    }

                    //close and clean 
                    for(int i =1; i < MAX_SUPPORT_OC; i++)
                        if(g_Sys_Manager.psOC_Sys[i])
                        {
                            if( strstr(pDevBrdcstDevIntf->dbcc_name,ACCONTROLLER_USB_ID_RESET) || strstr(pDevBrdcstDevIntf->dbcc_name,ACCONTROLLER_USB_ID_RESET_XP))
                            {
                                if(g_Sys_Manager.psOC_Sys[i]->isOpen)
                                    Sys_Close(i);

                                g_Sys_Manager.ocSysM_UnInitACController(i);

                                // OneController has been disconnected. Reset power status to "unknown".
                               // ResetPowerStatus((HANDLE) i);

                                break;
                            }

                        }
                }
            }
        }
        break;

    case WM_DESTROY:
        PostQuitMessage (0);       /* send a WM_QUIT to the message queue */
        break;
    default:                      /* for messages that we don't deal with */
        return DefWindowProc (hwnd, message, wParam, lParam);
    }

    return 0;
}

BOOL oc_UnRegisterForDeviceNotification(HDEVNOTIFY* diNotifyHandle)
{
    return UnregisterDeviceNotification(*diNotifyHandle);
}
