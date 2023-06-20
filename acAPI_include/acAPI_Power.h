//*****************************************************************************
//
// OneController Power functions.
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

#ifndef __OC_POWER_H
#define __OC_POWER_H


#define VIA_POWER_UNITS           1
#define POWER_INTERFACE_ID        0x0C

#define POWER_STATUS_3V3_ON        1   // clear=3.3V OFF, set=3.3V ON
#define POWER_STATUS_3V3_FAULT     2   // clear=3.3V OK, set=3.3V FAULT
#define POWER_STATUS_5V0_ON        4   // clear=5V OFF, set=5V ON
#define POWER_STATUS_5V0_FAULT     8   // clear=5V OK, set=5V FAULT

/*****************************************************************************
 * Power Functions
 *
 * All API functions MUST use a "C" calling convention.
 ****************************************************************************/
#ifdef  __cplusplus
extern "C" {
#endif

/**
* This function is used to enable or disable one Power interface unit.
* Parameters:
*    handle     An open handle for communications 
*    unit       Which unit to enable/disable. There are two units:
*                   POWER_UNIT_3V3
*                   POWER_UNIT_5V0
*    enable     Set true to enable the power output, false to disable it.
*
* Returns:
*    OneController status code.
*  This API is dependent on the hardware platform, it only works on dongle
*/
acAPI Power_Enable_unsupport(AC_HANDLE handle, uint8_t unit, bool enable);

/**
* This function is used to get the status of the Power interface.
* Parameters:
*    handle     An open handle for communications 
*
* Returns:
*    OneController power status, as follows:
*
*       Bit   Output    Low   High
*       ---  ---------  ---   -----
*        0   +3.3V_EXT  OFF    ON
*        1   +3.3V_EXT  OK    FAULT
*        2   +5V_EXT    OFF    ON
*        3   +5V_EXT    OK    FAULT
*
* Comments:
*   Normally, the current power status is cached locally and is returned immediately, 
*   without the overhead of sending a command to the firmware and waiting for a reply. 
*   The local cached status is updated automatically whenever the OneController's
*   power status changes.
*
*   However, the first time this function is called after a OneController board has   
*   been connected, disconnected, or reconnected, the locally cached copy of the status 
*   may be "stale" (not reflecting the actual power status). In that case, a status
*   request command is sent to the OneController, which will incur a small overhead of
*   a few milliseconds.
*/
acAPI Power_GetStatus_unsupport(AC_HANDLE handle);

#ifdef  __cplusplus
}
#endif

#endif // __OC_POWER_H
