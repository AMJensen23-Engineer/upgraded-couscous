
//*****************************************************************************
//
//  OneController Power functions.
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
#include <stdlib.h>
#include <memory.h>

#include "acctrl.h"
#include "acAPI_Power.h"


#define DEFAULT_TIMEOUT         1000
#define STATUS_REFRESH_TIMEOUT  1000
#define STATUS_CHECK_TIMEOUT    50
#define UNKNOWN_POWER_STATUS    0x55555555

#define POWER_BASE_UNIT         0       // there is really only one power unit

// Various flag arrays -- must be initialized to all zeros.
static bool s_bEnabled[MAX_SUPPORT_OC][POWER_UNITS];
static bool s_bInitIF_done[MAX_SUPPORT_OC];
static uint32_t s_PowerStatus[MAX_SUPPORT_OC];

STATUS ResetPowerStatus(AC_HANDLE handle)
{
	HANDLE_CHECK(handle);

    s_PowerStatus[handle] = UNKNOWN_POWER_STATUS;
    return OC_SUCCESS;
}

static void ecPower_Initialize(OC_COMMAND *pCmd, uint8_t unit)
{
    static bool s_bInitialized = false;

    // This must be done only once per DLL instance!
    if (!s_bInitialized)    
    {
        s_bInitialized = true;

        // initialize all of these to zero
        memset(s_bEnabled, 0, sizeof(s_bEnabled));
        memset(s_bInitIF_done, 0, sizeof(s_bInitIF_done));

        // reset power status for all possible devices
        for (uint32_t handle = 0; handle < MAX_SUPPORT_OC; ++handle)
        {
            s_PowerStatus[handle] = UNKNOWN_POWER_STATUS;
        }
    }

    if (pCmd != NULL)
    {
        InitCommand(pCmd);
        pCmd->if_type = POWER_INTERFACE_ID;
        pCmd->if_unit = unit;
    }
}

acAPI Power_Enable_unsupport(AC_HANDLE handle, uint8_t unit, bool enable)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;
    uint32_t param = 0;

	HANDLE_CHECK(handle);

    // validate parameters
    switch (unit)
    {
    case POWER_UNIT_3V3:
        param = enable? OC_POWER_3v3_ON : OC_POWER_3v3_OFF;
        break;

    case POWER_UNIT_5V0:
        param = enable? OC_POWER_5v0_ON : OC_POWER_5v0_OFF;
        break;

    default:
        status = OC_ERR_PARAM_OUT_OF_RANGE;
        break;
    }

	if (!s_bInitIF_done[handle]) // do this only once for each handle!
	{
		status = (0 == acInitIF(handle, POWER_INTERFACE_ID, POWER_BASE_UNIT)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
		s_bInitIF_done[handle] = (STAT_OK == status);
	}

    if (STAT_OK == status && s_bInitIF_done[handle])
    {
        ecPower_Initialize(&cmd, POWER_BASE_UNIT);
        cmd.command  = acCmd_Power_Enable;
        cmd.param[0] = param;  
        cmd.param[1] = 0; 

        status = ocSendCommand(handle, &cmd);
        if (IsSuccess(status))
        {
            s_bEnabled[handle][unit] = enable;
            status = acWaitForStatus(handle, MAKE_IF(POWER_INTERFACE_ID, POWER_BASE_UNIT), DEFAULT_TIMEOUT);
            if (IsSuccess(status))
            {
                s_PowerStatus[handle] = status;
            }
        }
    }

    return status;
}

acAPI Power_GetStatus(AC_HANDLE handle)
{
    STATUS status = STAT_OK;

	HANDLE_CHECK(handle);

    // send a status request command only if the current power status is unknown
    if (UNKNOWN_POWER_STATUS == s_PowerStatus[handle])   
    {
        OC_COMMAND cmd;

        ecPower_Initialize(&cmd, POWER_BASE_UNIT);
        cmd.command  = acCmd_Power_FaultStatus;

        status = ocSendCommand(handle, &cmd);
        if (IsSuccess(status))
        {
            status = acWaitForStatus(handle, MAKE_IF(POWER_INTERFACE_ID, POWER_BASE_UNIT), STATUS_REFRESH_TIMEOUT);
            if (IsSuccess(status))
            {
                s_PowerStatus[handle] = status;
            }
        }
    }
    else
    {
        STATUS new_status = acWaitForStatus(handle, MAKE_IF(POWER_INTERFACE_ID, POWER_BASE_UNIT), STATUS_CHECK_TIMEOUT);
        if (IsSuccess(new_status))
        {
            s_PowerStatus[handle] = new_status;
        }
    }

    // if successful, return the current status; otherwise, the error code
    return IsSuccess(status) ? s_PowerStatus[handle] : status;
}

