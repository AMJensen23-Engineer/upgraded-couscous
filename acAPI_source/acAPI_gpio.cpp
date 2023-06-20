//*****************************************************************************
//
//  OneController GPIO functions.
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

#include <string.h>

#include "acctrl.h"
#include "acctrl_api.h"


#define DEFAULT_TIMEOUT         255

static bool bInitialized = false;
static bool bEnabled[MAX_SUPPORT_OC];
static uint16_t nNumberOfBytesRead[MAX_SUPPORT_OC];

uint8_t g_gpio_pin_num  = GPIO_PIN_COUNT;

char g_gpio_pin_name[GPIO_PIN_COUNT][PIN_NAME_LENGTH] = {
	"PN2",  // SPI address 0 LPP SPI chip select Chip Select SSN
	"PN3",  // SPI address 1
	"PP2",  // SPI address 2
	"PK3",
	"PK4",
	"PK5",
	"PF2",
	"PF3",
	"PB2",
	"PB3",
	"PB4",	// was _PIN_PD4
	"PB5",	// OneController Dongle Rev A (and later) and BoosterPack
	"PP0",
	"PP1",
	"PP3",
	"PP4"
};

void GPIO_Initialize(OC_COMMAND *pCmd)
{
    if (!bInitialized)
    {
        memset(bEnabled, false, sizeof(bEnabled));
        memset(nNumberOfBytesRead, 0, sizeof(nNumberOfBytesRead));
        bInitialized = true;
    }

    if (pCmd != NULL)
    {
        InitCommand(pCmd);
    }
}

acAPI GPIO_Enable(AC_HANDLE handle, uint32_t pin_mask, bool enable)
{
    OC_COMMAND cmd;
    STATUS status = OC_ERR_OPERATION_FAILED;
	
	
	HANDLE_CHECK(handle);
	
	//check the gpio number out of range
	if(pin_mask & ~((1 << g_gpio_pin_num) -1))
		return OC_ERR_PARM_WRONG;


    GPIO_Initialize(&cmd);
    if (enable && !bEnabled[handle])
    {
        status = (0 == acInitIF(handle, IF_GPIO, 0)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
    }
    else if (!enable && bEnabled[handle])
    {
        status = (0 == acUnInitIF(handle, IF_GPIO, 0)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
    }
    else
    {
        status = bEnabled[handle] ? STAT_OK : OC_ERR_NOT_ENABLED;
    }

    if (STAT_OK == status)
	{
		cmd.if_type  = GPIO_Interface;
		cmd.if_unit  = 0;
		cmd.command  = acCmd_GPIO_Enable;
		cmd.param[0] = pin_mask; 
		cmd.param[1] = enable; 

#if 0
	   char pin_name_buff[64];
	
	   uint8_t gpio_pin;
	   uint16_t bit =1;
	   uint8_t pin_num  = 0;
		 // create the resource list
	  char *p_pin_name = (char*)pin_name_buff;
      for (gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin, bit <<= 1)
       {
         if (pin_mask & bit)     // enable/disable this pin?
         {
           strcpy(p_pin_name,(char*)g_gpio_pin_name[gpio_pin]);
           pin_num++;
		   p_pin_name += PIN_NAME_LENGTH;
         }
       }

	     //set pin  number
		 cmd.param[1] = pin_num;  
		 //set copy data length
		 cmd.data_len = pin_num * PIN_NAME_LENGTH;
         cmd.pdata    = (uint8_t*)pin_name_buff;
    

#endif
		 
		 status = ocSendCommand(handle, &cmd);
		if (STAT_OK == status)
		{
            bEnabled[handle] = enable;
            nNumberOfBytesRead[handle] = 0;

            status = acWaitForStatus(handle, MAKE_IF(GPIO_Interface, 0), DEFAULT_TIMEOUT);
		}
	}

    return status;
}

acAPI GPIO_Config(AC_HANDLE handle, uint32_t pin_mask, uint8_t mode)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

	//check the gpio number out of range
	if(pin_mask & ~((1 << g_gpio_pin_num) -1))
		return OC_ERR_PARM_WRONG;

    GPIO_Initialize(&cmd);
    cmd.if_type  = GPIO_Interface;
    cmd.if_unit  = 0;
    cmd.command  = acCmd_GPIO_Config;
    cmd.param[0] = pin_mask;
    cmd.param[1] = mode;

#if 0
	uint8_t pin_num = 0;
	char pin_name_buff[64];
	uint8_t gpio_pin;
	uint16_t bit =1;
	
	// create the resource list
	  char *p_pin_name = (char*)pin_name_buff;
      for (gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin, bit <<= 1)
       {
         if (pin_mask & bit)     // enable/disable this pin?
         {
           strcpy(p_pin_name,(char*)g_gpio_pin_name[gpio_pin]);
           pin_num++;
		   p_pin_name += PIN_NAME_LENGTH;
         }
       }

	     //set pin  number
		 cmd.param[1] = pin_num;  
		 //set copy data length
		 cmd.data_len = pin_num * PIN_NAME_LENGTH;
         cmd.pdata    = (uint8_t*)pin_name_buff;
#endif
		 status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(GPIO_Interface, 0), DEFAULT_TIMEOUT);
    }

    return status;
}

acAPI GPIO_Write(AC_HANDLE handle, uint32_t pin_mask, uint32_t values)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

	//check the gpio number out of range
    if(pin_mask & ~((1 << g_gpio_pin_num) -1))
		return OC_ERR_PARM_WRONG;

    GPIO_Initialize(&cmd);
    cmd.if_type  = GPIO_Interface;
    cmd.if_unit  = 0;
    cmd.command  = acCmd_GPIO_Write;
    cmd.param[0] = pin_mask;
    cmd.param[1] = values;
	
#if 0
	uint8_t pin_num = 0;
	char pin_name_buff[64];
	uint8_t gpio_pin;
	uint16_t bit =1;
		 // create the resource list
	  char *p_pin_name = (char*)pin_name_buff;
      for (gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin, bit <<= 1)
       {
         if (pin_mask & bit)     // enable/disable this pin?
         {
           strcpy(p_pin_name,(char*)g_gpio_pin_name[gpio_pin]);
           pin_num++;
		   p_pin_name += PIN_NAME_LENGTH;

         }
       }

	     //set pin  number
		 cmd.param[2] = pin_num;  
		 //set copy data length
		 cmd.data_len = pin_num * PIN_NAME_LENGTH;
         cmd.pdata    = (uint8_t*)pin_name_buff;
#endif  
    status = ocSendCommand(handle, &cmd);
    if (STAT_OK == status)
    {
        status = acWaitForStatus(handle, MAKE_IF(GPIO_Interface, 0), DEFAULT_TIMEOUT);
    }

	return status;
}

acAPI GPIO_Read(AC_HANDLE handle, uint32_t pin_mask, uint32_t *pvalues)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

	//check the gpio number out of range
	if(pin_mask & ~((1 << g_gpio_pin_num) -1))
		return OC_ERR_PARM_WRONG;

    GPIO_Initialize(&cmd);
    cmd.if_type  = GPIO_Interface;
    cmd.if_unit  = 0;
    cmd.command  = acCmd_GPIO_Read;
    cmd.param[0] = pin_mask;
#if 0
    char pin_name_buff[64];
	uint8_t gpio_pin;
	uint16_t bit =1;
	uint8_t pin_num  = 0;
		 // create the resource list
	  char *p_pin_name = (char*)pin_name_buff;
      for (gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin, bit <<= 1)
       {
         if (pin_mask & bit)     // enable/disable this pin?
         {
           strcpy(p_pin_name,(char*)g_gpio_pin_name[gpio_pin]);
           pin_num++;
		   p_pin_name += PIN_NAME_LENGTH;
         }
       }

	     //set pin  number
		 cmd.param[1] = pin_num;  
		 //set copy data length
		 cmd.data_len = pin_num * PIN_NAME_LENGTH;
         cmd.pdata    = (uint8_t*)pin_name_buff;
#endif

	status = ocSendCommand(handle, &cmd);

    if (STAT_OK == status)
    {
        uint32_t num_bytes = sizeof(uint32_t);
        uint32_t count = acPacket_ReadSync(handle, MAKE_IF(GPIO_Interface, 0), (uint8_t *) pvalues, num_bytes, DEFAULT_TIMEOUT);

        if (count < num_bytes)
        {
			status = Sys_GetLastError(handle, (char *) NULL, 0);
			status = acWaitForStatus(handle, MAKE_IF(GPIO_Interface, 0), DEFAULT_TIMEOUT);
        }
    }

    return status;
}
acAPI GPIO_RegisterInt(AC_HANDLE handle, uint32_t pin_mask,uint8_t IntType[3],uint8_t EnableInt[3],  void (*int_callback)(uint8_t *pcallbackdata, uint16_t callback_data_size),uint8_t* pdata, uint32_t data_size)
{
    OC_COMMAND cmd;
    STATUS status;

	HANDLE_CHECK(handle);

	//check the gpio number out of range
	if(pin_mask & ~((1 << g_gpio_pin_num) -1))
		return OC_ERR_PARM_WRONG;
	
	if(int_callback == NULL || pdata == NULL || data_size ==0)
		return OC_ERR_PARM_WRONG;

    GPIO_Initialize(&cmd);
    cmd.if_type  = GPIO_Interface;
    cmd.if_unit  = 0;
    cmd.command  = acCmd_GPIO_RegisterInterrupt;
    cmd.param[0] = pin_mask;
	
	cmd.param[1] = IntType[0];
	cmd.param[2] = IntType[1];
    cmd.param[3] = IntType[2];

    cmd.param[4] = EnableInt[0];
	cmd.param[5] = EnableInt[1];
    cmd.param[6] = EnableInt[2];

#if 0
    char pin_name_buff[64];
	uint8_t gpio_pin;
	uint16_t bit =1;
	uint8_t pin_num  = 0;
		 // create the resource list
	  char *p_pin_name = (char*)pin_name_buff;
      for (gpio_pin = 0; gpio_pin < g_gpio_pin_num; ++gpio_pin, bit <<= 1)
       {
         if (pin_mask & bit)     // enable/disable this pin?
         {
           //maximum support three gpios
			if(pin_num  >= 3 )
               return OC_ERR_PARM_WRONG;

		   strcpy(p_pin_name,(char*)g_gpio_pin_name[gpio_pin]);
           p_pin_name += PIN_NAME_LENGTH;
		   if(  ( IntType[pin_num] >= RISING_EDGE  &&  IntType[pin_num] <= LEVEL_LOW ) && (EnableInt[pin_num] >= REGISTER_GPIO_INT && EnableInt[pin_num] <= NO_CHANGE))
			   ;
		   else
              return OC_ERR_PARM_WRONG;

		    pin_num++;
			
		  
         }
       }
	  
	     //set pin  number
		 cmd.param[7] = pin_num;  
		 //set copy data length
		 cmd.data_len = pin_num * PIN_NAME_LENGTH;
         cmd.pdata    = (uint8_t*)pin_name_buff;
#endif
	     status = ocSendCommand(handle, &cmd);

    if (STAT_OK == status)
    {
	
        status = acWaitForStatus(handle, MAKE_IF(GPIO_Interface, 0), DEFAULT_TIMEOUT);
    	if(STAT_OK == status && ((EnableInt[0] == REGISTER_GPIO_INT || EnableInt[0] == REGISTER_GPIO_INT_EN) || (EnableInt[1] == REGISTER_GPIO_INT || EnableInt[1] == REGISTER_GPIO_INT_EN)|| (EnableInt[2] == REGISTER_GPIO_INT || EnableInt[2] == REGISTER_GPIO_INT_EN) ))
		{
		
			RCV_QUEUE *rcv = g_Sys_Manager.psOC_Sys[handle]->prcv_queue->oc_Find_Rcv_Queue(MAKE_IF(GPIO_Interface, 0));
		     if(rcv)
		      {
		        rcv->pfIntCallBack = int_callback;
			    rcv->pIntCallbackBuff = pdata;
			    rcv->IntCallbackDataSize =  data_size;
		      }
		}
    }

    return status;
}
