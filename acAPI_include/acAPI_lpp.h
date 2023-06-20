#ifndef __OC_LPP_H
#define __OC_LPP_H

#define TIVA_LPP_UNITS 2
#define TIVA_LPP_TIMERS 2

//work for Windows and GCC 
#pragma pack(push,4) 
typedef struct _LPP_CFG
{
	uint16_t part;
}LPP_CFG;
#pragma pack(pop)

#ifdef  __cplusplus
extern "C" {
#endif

	acAPI LPP_dllRevision(uint8_t *revVal);
	acAPI LPP_Enable(AC_HANDLE handle, uint8_t unit, bool enable);
	acAPI LPP_Config(AC_HANDLE handle, uint8_t unit, LPP_CFG *p_lpp_cfg);
	acAPI LPP_WriteAndRead(AC_HANDLE handle, uint8_t unit, LPP_CFG *p_lpp_cfg);
	acAPI LPP_ConfigTimer(AC_HANDLE handle, uint8_t unit, uint32_t usecs);
	acAPI LPP_StartTimer(AC_HANDLE handle, uint8_t unit);
	acAPI LPP_StopTimer(AC_HANDLE handle, uint8_t unit);
	acAPI LPP_FirePWMs(AC_HANDLE handle, uint8_t unit, uint32_t freq, uint8_t dutyCycle1, uint8_t dutyCycle2, bool syncMode, bool phase180);
	acAPI LPP_FirePWMsDB(AC_HANDLE handle, uint8_t unit, uint32_t freq, uint8_t dutyCycle1, uint8_t dutyCycle2, uint8_t deadBandRising, uint8_t deadBandFalling, bool divideBy4, bool phase180);
	acAPI LPP_EnableSTM_MPIO1_interrupt(AC_HANDLE handle, uint8_t unit);
	acAPI LPP_EnableSTM_MPIO1_Writes(AC_HANDLE handle, uint8_t unit);
	acAPI LPP_EnableCTM_MPIO0_Writes(AC_HANDLE handle, uint8_t unit, uint8_t device);
	acAPI LPP_fwRevision(AC_HANDLE handle, uint8_t unit, uint8_t *p_read_data_buffer);
	acAPI LPP_Processor(AC_HANDLE handle, uint8_t unit, uint8_t *p_read_data_buffer);
	acAPI LPP_getWatchdogData(AC_HANDLE handle, uint8_t unit, uint8_t *p_read_data_buffer);
	acAPI LPP_SpareOne(AC_HANDLE handle, uint8_t unit, uint32_t arg1, uint32_t arg2, uint8_t *p_read_data_buffer);
	acAPI LPP_SpareTwo(AC_HANDLE handle, uint8_t unit, uint32_t arg1, uint32_t arg2, uint8_t *p_read_data_buffer);

#ifdef  __cplusplus
}
#endif


#endif // __OC_LPP_H