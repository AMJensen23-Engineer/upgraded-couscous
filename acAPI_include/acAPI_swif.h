
//*****************************************************************************
// Change History
//
//+ 2015-11-10 [RT] - Added prototype for ocSPI_GetNumberOfBytesRead
//

#ifndef __OC_SWIF_H
#define __OC_SWIF_H



#define VIA_SWIF_UNITS           3
#ifdef _WIN32 
#pragma pack(push, 4)
#endif
//work for Windows and GCC 
typedef struct _SWIF_CFG
{
  uint32_t gpio;// which gpio 
  uint8_t  gpio_input_mode;// p  
  uint8_t  gpio_output_mode;//data width from 4 to 16 bits
  uint16_t  swif_mode;       //because each customer may have the different swif protcol 
  uint16_t  state_short_period; //unit uS
  uint16_t  state_long_period; //unit uS
  uint16_t  read_wait_start_time;//unit uS
  }SWIF_CFG;

#ifdef _WIN32
#pragma pack(pop)
#endif

#ifdef  __cplusplus
extern "C" {
#endif

/**
* This function is used to enable or disable an swif interface unit.
* @param handle An open handle for communications 
* @param unit The number of the unit to be enabled/disabled.
* @param enable Set true to enable swif device.
* @date 1/4/2017
*/
acAPI SWIF_Enable(AC_HANDLE handle, uint8_t unit, bool enable);

/**
* This function is used to configure an swif interface unit.
* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param swif_cfg  swif configurations 
* @date 1/4/2017
*/
acAPI SWIF_Config(AC_HANDLE handle, uint8_t unit, SWIF_CFG *p_swif_cfg );


/**
* This function is used to write data to swif interface device.

* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param reg_addr register index.
* @param reg_addr_bits  register address width
* @param vytes_to_write  write data bytes
* @param write_data_buffer A pointer to the buffer that contains the data to be written. the buffer will be as read buffer too.
* @date 1/4/2017
*/
acAPI SWIF_WriteReg(AC_HANDLE handle, uint8_t unit, uint16_t reg_addr, uint8_t reg_addr_bits, 
    uint8_t bytes_to_write,uint8_t *p_write_data_buffer);
/**
* This function is used to reade data from swif interface decvice.

* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param reg_addr register index.
* @param reg_addr_bits  register address width
* @param bytes_to_read  write data bytes
* @param write_data_buffer A pointer to the buffer that contains the data to be written. the buffer will be as read buffer too.
* @date 1/4/2017
*/
acAPI SWIF_ReadReg(AC_HANDLE handle, uint8_t unit, uint16_t reg_addr, uint8_t reg_addr_bits, 
    uint8_t bytes_to_read,uint8_t *p_read_data_buffer);



#ifdef  __cplusplus
}
#endif

#endif // __OC_SWIF_H
