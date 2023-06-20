

#ifndef _ONECTRL_INTERFACE_H_
#define _ONECTRL_INTERFACE_H_

#ifdef WIN32
#define ONECTRL_API __declspec(dllexport) 
#define WIN_API     __stdcall  
#else
#define ONECTRL_API 
#define WIN_API
#endif

/* definition of handle type */
#define STATUS              int32_t
#define AC_HANDLE		    int32_t
#define INVALID_EVMC_HANDLE   ((AC_HANDLE) 0)
#define IsValidOC_HANDLE(h) ((h) > INVALID_AC_HANDLE)

#define acAPI			    STATUS ONECTRL_API WIN_API
#define acAPI_HANDLE	    AC_HANDLE ONECTRL_API WIN_API

#define MAKE_IF(intf, unit) ((((uint8_t) intf) << 8) + unit)


#endif // _ONECTRL_INTERFACE_H_