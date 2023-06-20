//*****************************************************************************
//
// OneController LED functions.
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
// Change History
//
//+ 2015-11-10 [RT] - Enabled ocLED_Write and ocLED_Read functions
//

#include "acctrl.h"

#include "acAPI_led.h"

#define VIA_LED_INTERFACE_UNITS      1   /* this is a single-unit interface */

/**************************************************
* ocLED_Initialize
*
* This function initializes the command structure that
* will be sent to the LED interface. It can also do
* one-time initialization of variables, structures, and so on.
*
* Parameters:
*   pCmd        A pointer to the OC_COMMAND struct that is
*               to be initialized.
*
* Returns:
*   Nothing.
*/
static void acLED_Initialize(OC_COMMAND *pCmd, uint8_t unit, uint16_t command)
{
    static bool s_bDoneOnce = false;

    if (!s_bDoneOnce)
    {
        /**************************************************
         * TODO 
         * Initialize variables, structures, and so on.
         * This code is executed only once, the first time
         * any of the API functions is called.
         */
        s_bDoneOnce = true;
    }

    if (pCmd != NULL)
    {
        InitCommand(pCmd);
        pCmd->if_type  = LED_Interface;
        pCmd->if_unit  = unit;
        pCmd->command  = command;
    }
}

/**************************************************
* ocLED_Enable
*
* This function is used to enable or disable the LED interface.
*
* Parameters:
*   handle      Contains the handle value returned by the
*               ocSys_Open() function.
*   enable      The LED interface is enabled if this is true,
*               or disabled if it is false.
*
* Returns:
*   On success, returns zero; otherwise, an error code (always 
*   negative) is returned.
*/
acAPI ocLED_Enable_unsupport(AC_HANDLE handle, bool enable)
{
    OC_COMMAND cmd;
    STATUS status = OC_ERR_OPERATION_FAILED;
    uint8_t unit = 0;   // unit is always zero for the LED interface

	HANDLE_CHECK(handle);

    /* initialize the command packet */
    acLED_Initialize(&cmd, unit, acCmd_LED_Enable);

    /* the interface must be initialized in the Enable function */
    status = InitInterface(handle, IF_LED, unit, enable);
    
    if (STAT_OK == status)
    {
        cmd.param[0] = unit;            /* pass unit as a parameter  */  
        cmd.param[1] = enable;          /* pass the enable parameter */ 

        /* send the command to the interface */
        status = ocSendCommand(handle, &cmd);

        if (STAT_OK == status)
        {
            /* Wait for acknowledgement of the command */
            status = acWaitForStatus(handle, MAKE_IF(LED_Interface, unit), 255);
        }
    }

    return status;
}

/**************************************************
* ocLED_Config
*
* This function sends a configuration command and any
* necessary parameters to the LED interface.
*
* Parameters:
*   handle      Contains the handle value returned by the
*               ocSys_Open() function.
*   LEDs        A bit mask representing the LEDs to be configured.
*                   1 = Red, 2 = Green, 4 = Blue
*   operation   The operation to be performed on the selected LEDs.
*                   0 = Off, 1 = On, 2 = Exclusive On, 3 = Toggle
*
* Returns:
*   On success, returns zero; otherwise, an error code (always 
*   negative) is returned.
*/
acAPI ocLED_Config_unsupport(AC_HANDLE handle, uint8_t LEDs, uint32_t operation)
{
    OC_COMMAND cmd;
    STATUS status;
    uint8_t unit = 0;   // unit is always zero for the LED interface

	HANDLE_CHECK(handle);

    /* initialize the command packet */
    acLED_Initialize(&cmd, 0, acCmd_LED_Config);

    cmd.param[0] = LEDs;        
    cmd.param[1] = operation; 

    /* send the command to the interface */
    status = ocSendCommand(handle, &cmd);

    if (STAT_OK == status)
    {
        /* Wait for acknowledgement of the command */
        status = acWaitForStatus(handle, MAKE_IF(LED_Interface, unit), 255);
    }

    return status;
}

/**************************************************
* ocLED_Write
*
* This is an example of a function that writes one 
* or more bytes of data to the LED interface.
*
* Parameters:
*   handle          Contains the handle value returned by the
*                   ocSys_Open() function.
*   unit            The unit number of the LED device. For
*                   single-unit devices, this is always zero.
*   bytes_to_write  The number of bytes to be written.
*   pData           A pointer to a buffer that contains the data
*                   to be written.
*
* Returns:
*   On success, returns zero; otherwise, an error code (always 
*   negative) is returned.
*/
acAPI ocLED_Write_unsupport(AC_HANDLE handle, uint8_t unit, uint16_t bytes_to_write, uint8_t *pData)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

    /* initialize the command packet */
    acLED_Initialize(&cmd, unit, acCmd_LED_Write);

    cmd.param[0] = unit;            /* pass unit as a parameter */
    cmd.data_len = bytes_to_write;  /* number of bytes to write */
    cmd.pdata    = pData;           /* pointer to the data to be written */

    /* send the command to the interface */
    status = ocSendCommand(handle, &cmd);

    if (STAT_OK == status)
    {
        /* Wait for acknowledgement of the command */
        status = acWaitForStatus(handle, MAKE_IF(LED_Interface, unit), 255);
    }

    return status;
}

/**************************************************
* ocLED_Read
*
* This is an example of a function that reads one 
* or more bytes of data from the LED interface.
*
* Parameters:
*   handle          Contains the handle value returned by the
*                   ocSys_Open() function.
*   unit            The unit number of the LED device. For
*                   single-unit devices, this is always zero.
*   bytes_to_read   The number of bytes to be read.
*   pData           A pointer to a buffer that will receive the
*                   data that is read.
*
* Returns:
*   On success, returns the number of bytes read; otherwise, an
*   error code (always negative) is returned.
*/
acAPI ocLED_Read(AC_HANDLE handle, uint8_t unit, uint16_t bytes_to_read, uint8_t *pData)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

    /* initialize the command packet */
    acLED_Initialize(&cmd, unit, acCmd_LED_Read);

    cmd.param[0] = unit;            /* pass unit as a parameter   */
    cmd.param[1] = bytes_to_read;   /* number of bytes to be read */

    /* send the command to the interface */
        status = ocSendCommand(handle, &cmd);

        if (STAT_OK == status)
        {
        /* get the received data into the buffer pointed to by pData */
            status = acGetReceivedData(handle, LED_Interface, unit, pData, bytes_to_read, 255);
    }

    return status;
}

