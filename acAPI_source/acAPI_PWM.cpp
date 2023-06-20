
//*****************************************************************************
//
//  OneController pwm functions.
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
#include <memory.h>

#include "acctrl.h"
#include "acAPI_PWM.h"

static bool bInitialized = false;
static bool bEnabled[MAX_SUPPORT_OC][ACCTRL_PWM_UNITS];

//uint8_t g_pwm_num  = 4; //default number of OneController



void PWM_Initialize(OC_COMMAND *pCmd)
{
    if (!bInitialized)
    {
        memset(bEnabled, false, sizeof(bEnabled));
        bInitialized = true;
    }

    if (pCmd != NULL)
    {
        InitCommand(pCmd);
    }
}

acAPI PWM_Enable(AC_HANDLE handle, uint8_t unit, bool enable)
{
    OC_COMMAND cmd;
    STATUS status = OC_ERR_OPERATION_FAILED;;

	HANDLE_CHECK(handle);
	
	if(unit >= ACCTRL_PWM_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;


	
    PWM_Initialize(&cmd);
    
    if (enable && !bEnabled[handle][unit])
    {
        status = (0 == acInitIF(handle, IF_PWM, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
	}
    else
    {
        status = bEnabled[handle][unit] ? STAT_OK : OC_ERR_NOT_ENABLED;
    }
    
	
    if (STAT_OK == status)
    {
        cmd.if_type  = PWM_Interface;
        cmd.if_unit  = unit;
        cmd.command  = acCmd_PWM_Enable;
        cmd.param[0] = unit;  
        cmd.param[1] = enable; 


        status = ocSendCommand(handle, &cmd);
        if (STAT_OK == status)
        {
         	bEnabled[handle][unit] = enable;
           status = acWaitForStatus(handle, MAKE_IF(PWM_Interface, unit), 10255);
        }
    }

    // Disable only if the interface with the correct 
    // handle and unit is enabled.  (RT 467)
	if (!enable && bEnabled[handle][unit])
    {
        status = (0 == acUnInitIF(handle, IF_PWM, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
        //should not care return status.
		bInitialized = false;
		bEnabled[handle][unit] = false;
	}
   
    return status;
}

acAPI PWM_Config(AC_HANDLE handle, uint8_t unit,PWM_Params *params)
{
    OC_COMMAND cmd;
    STATUS status;

	if(unit >= ACCTRL_PWM_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	
	HANDLE_CHECK(handle);
    PWM_Initialize(&cmd);
    InitCommand(&cmd);
    cmd.if_type  = PWM_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_PWM_Config;
    cmd.param[0] = unit;
    		
	cmd.param[1]= params->periodUnits;
	cmd.param[2] = params->periodValue;
	cmd.param[3] = params->dutyUnits ;
	cmd.param[4] = params->dutyValue;
	cmd.param[5] = params->idleLevel;

	
    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(PWM_Interface, unit), 10255);
    }

    return status;
}




acAPI PWM_Start(AC_HANDLE handle, uint8_t unit)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;

	if(unit >= ACCTRL_PWM_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	
	HANDLE_CHECK(handle);
    PWM_Initialize(&cmd);
    cmd.if_type  = PWM_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_PWM_Start;
    cmd.param[0] = unit;
    
	
    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
     {
        status = acWaitForStatus(handle, MAKE_IF(PWM_Interface, unit), 50000);

     }

    return status;
}

acAPI PWM_Stop(AC_HANDLE handle, uint8_t unit)
{
    OC_COMMAND cmd;
    STATUS status = STAT_OK;

	
	
	HANDLE_CHECK(handle);
    PWM_Initialize(&cmd);
    cmd.if_type  = PWM_Interface;
    cmd.if_unit  = unit;
    cmd.command  = acCmd_PWM_Stop;
    cmd.param[0] = unit;
    

    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
      status = acWaitForStatus(handle, MAKE_IF(PWM_Interface, unit), 50000);

    }

    return status;
}



