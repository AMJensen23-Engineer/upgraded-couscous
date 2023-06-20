//*****************************************************************************
//
//  OneController get pin map 
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
#include <stdio.h>
#include <string.h>
#include <stdlib.h>


#include "acctrl.h"
#include "acctrl_api.h"



#define LGPIO_NAME "LGPIO_"
#define FIRST_LGPIO_NAME "LGPIO_00"

#define LI2C_NAME "LI2C_"
#define FIRST_LI2C_NAME "LI2C_0"
#define TIVA_I2C_NAME "TIVA_I2C_"

#define LSPI_NAME "LSPI_"
#define FIRST_LSPI_NAME "LSPI_0"
#define TIVA_SPI_NAME "TIVA_SPI_"

#define LUART_NAME "LUART_"
#define FIRST_LUART_NAME "LUART_0"
#define TIVA_UART_NAME "TIVA_UART_"


#define PERIPH_MAP_FILE_NAME  "periph_map.cfg"

static STATUS get_gpio_map(char * pin_name)
{
  char *str;
  
  uint8_t lgpio_num = atoi(pin_name + strlen(LGPIO_NAME));
   
  //the first line must be GPIO_00  
   if(lgpio_num == 0 )
     g_gpio_pin_num = 0;

  if(lgpio_num == g_gpio_pin_num)
	{
	    str = strchr((pin_name + strlen(FIRST_LGPIO_NAME)), 'P');
		
		if(str)
			 {
		    
				 if(str[1] < 'A' || str[1] > 'Q' || str[1] == 'O' || str[1] == 'I' )
	    	           return	OC_ERR_PARM_WRONG ;

	    
	            if(str[2] >= '0' && str[2] <= '7'  )
	                {
	                  //copy GPIO name to map  
					  memcpy(g_gpio_pin_name[g_gpio_pin_num],str,PIN_NAME_LENGTH -1);
					  g_gpio_pin_name[g_gpio_pin_num][PIN_NAME_LENGTH -1] = 0;
					  g_gpio_pin_num ++;

		            }else
	    	         return	OC_ERR_PARM_WRONG ;
			 }
    } else
     {
	    DbgWriteText("GPIO number is not contigous: %s \r\n",pin_name);
	     return	OC_ERR_PARM_WRONG ;
	 
	}
	return STAT_OK;		
}

static STATUS get_i2c_map(char * i2c_name)
{
  char *str;
  
  uint8_t li2c_num = atoi(i2c_name + strlen(LI2C_NAME));
   
  //the first line must be GPIO_00  
   if(li2c_num == 0 )
      g_i2c_num = 0;
   
   if(li2c_num == g_i2c_num)
	{
	    str = strstr((i2c_name + strlen(FIRST_LI2C_NAME)), TIVA_I2C_NAME);
		
		if(str)
		{
	     
		 uint8_t tiva_i2c_num = atoi(str + strlen(TIVA_I2C_NAME));
          if(tiva_i2c_num >=0 && tiva_i2c_num <10)
            {
				g_i2c_map[g_i2c_num] = tiva_i2c_num ;
		        g_i2c_num ++; 
		    }else
		    {
			  DbgWriteText("I2C bus bum out of range:%s \r\n",i2c_name);
	         return	OC_ERR_PARM_WRONG ;
			}

	    }else
		{
			 DbgWriteText("I2C map line problem %s \r\n",i2c_name);
	         return	OC_ERR_PARM_WRONG ;
		}
    }else
     {
	    DbgWriteText("I2C number is not contigous: %s \r\n",i2c_name);
	     return	OC_ERR_PARM_WRONG ;
	 
	}
   
  return STAT_OK;		
}

static STATUS get_spi_map(char * spi_name)
{
  char *str;
  
  uint8_t lspi_num = atoi(spi_name + strlen(LSPI_NAME));
   
  //the first line must be GPIO_00  
   if(lspi_num == 0 )
      g_spi_num = 0;
   
   if(lspi_num == g_spi_num)
	{
	    str = strstr((spi_name + strlen(FIRST_LSPI_NAME)), TIVA_SPI_NAME);
		
		if(str)
		{
	     
		 uint8_t tiva_spi_num = atoi(str + strlen(TIVA_SPI_NAME));
          if(tiva_spi_num >=0 && tiva_spi_num < TIVA_SPI_UNITS)
            {
				g_spi_map[g_spi_num] = tiva_spi_num ;
		        g_spi_num ++; 
		    }else
		    {
			  DbgWriteText("SPI bus bum out of range:%s \r\n",spi_name);
	         return	OC_ERR_PARM_WRONG ;
			}

	    }else
		{
			 DbgWriteText("SPI map line problem %s \r\n",spi_name);
	         return	OC_ERR_PARM_WRONG ;
		}
    }else
     {
	    DbgWriteText("SPI number is not contigous: %s \r\n",spi_name);
	     return	OC_ERR_PARM_WRONG ;
	 
	}
   
  return STAT_OK;		
}
static STATUS get_uart_map(char * uart_name)
{
  char *str;
  
  uint8_t luart_num = atoi(uart_name + strlen(LUART_NAME));
   
  //the first line must be GPIO_00  
   if(luart_num == 0 )
      g_uart_num = 0;
   
   if(luart_num == g_uart_num)
	{
	    str = strstr((uart_name + strlen(FIRST_LUART_NAME)), TIVA_UART_NAME);
		
		if(str)
		{
	     
		 uint8_t tiva_uart_num = atoi(str + strlen(TIVA_UART_NAME));
          if(tiva_uart_num >=0 && tiva_uart_num < TIVA_UART_UNITS)
            {
				g_uart_map[g_uart_num] = tiva_uart_num ;
		        g_uart_num ++; 
		    }else
		    {
			  DbgWriteText("UART bus bum out of range:%s \r\n",uart_name);
	         return	OC_ERR_PARM_WRONG ;
			}

	    }else
		{
			 DbgWriteText("UART map line problem %s \r\n",uart_name);
	         return	OC_ERR_PARM_WRONG ;
		}
    }else
     {
	    DbgWriteText("UART number is not contigous: %s \r\n",uart_name);
	     return	OC_ERR_PARM_WRONG ;
	 
	}
   
  return STAT_OK;				
}

acAPI ac_Sys_Get_PIN_MAP(char *map_file_path)
{
	FILE * fp;
	char map_file_name[256];
	STATUS retState =STAT_OK;
	char buff[256];
	
	uint8_t spi_num = 0;
	uint8_t i2c_num = 0;
	uint8_t uart_num = 0;

	
	strcpy(map_file_name,map_file_path);
	strcat(map_file_name,PERIPH_MAP_FILE_NAME);
	fp = fopen(map_file_name,"rt");
	if(fp)
	{
	  char  *str; 
	  char  *l_str;

	 do{
        if(fgets(buff, 256,fp))
		{
		   strupr(buff);
		   if(str=strstr(buff, "#MAP"))
		   {
		      if(l_str = strstr(str, LGPIO_NAME))
			  {
                if((retState= get_gpio_map(l_str))!=STAT_OK)
					break;
				
			  
			  }else if(l_str = strstr(str, LI2C_NAME))
			  {
			    if((retState = get_i2c_map(l_str))!=STAT_OK)
					break;
				
			  }else if(l_str = strstr(str, LSPI_NAME))
			  {
			   if((retState = get_spi_map(l_str))!=STAT_OK)
					break;
				
			  }else if(l_str = strstr(str, LUART_NAME))
			  {
			   if((retState = get_uart_map(l_str))!=STAT_OK)
					break;
				
			  }else
			  {
			    retState = OC_ERR_PARM_WRONG;
				break;
			  }
		   }

		}


	 }while(!feof(fp));

	 fclose(fp);
	}
 
	 return retState;
}