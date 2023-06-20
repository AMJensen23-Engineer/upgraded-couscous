#include <stdlib.h>
#include <string.h>

#include "acctrl.h"
#include "acAPI_gpio.h"
#include "acAPI_lpp.h"

static bool bInitialized = false;
static bool bEnabled[MAX_SUPPORT_OC][TIVA_LPP_UNITS];
static uint16_t nNumberOfBytesRead[MAX_SUPPORT_OC][TIVA_LPP_UNITS];
static const uint8_t LPP_FILE_REV_MAJOR = 0x02;
static const uint8_t LPP_FILE_REV_MINOR = 0x02;

uint8_t g_lpp_num = TIVA_LPP_UNITS;  //default number of OneController
uint8_t g_lpp_map[TIVA_LPP_UNITS] = { 0 };

acAPI LPP_dllRevision(uint8_t *revVal)
{
	*revVal = LPP_FILE_REV_MAJOR;
	*(revVal + 1) = LPP_FILE_REV_MINOR;
	return 0;
}

void LPP_Initialize(OC_COMMAND *pCmd)
{
	if (!bInitialized)
		bInitialized = true;

	if (pCmd != NULL)
		InitCommand(pCmd);
}

acAPI LPP_Enable(AC_HANDLE handle, uint8_t unit, bool enable)
{
	OC_COMMAND cmd;
	STATUS status = OC_ERR_OPERATION_FAILED;

	HANDLE_CHECK(handle);

	if (unit < g_lpp_num)
		unit = g_lpp_map[unit];
	else
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);

	if (enable && !bEnabled[handle][unit])
		status = (0 == acInitIF(handle, IF_LPP, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
	else
		status = bEnabled[handle][unit] ? STAT_OK : OC_ERR_NOT_ENABLED;

	if (STAT_OK == status)
	{
		cmd.if_type = LPP_Interface;
		cmd.if_unit = unit;
		cmd.command = acCmd_LPP_Enable;
		cmd.param[0] = unit;
		cmd.param[1] = enable;

		status = ocSendCommand(handle, &cmd);
		if (STAT_OK == status)
		{
			nNumberOfBytesRead[handle][unit] = 0;
			status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

			/* RT 01-07-16: change the status flag only if successful */
			if (IsSuccess(status))
				bEnabled[handle][unit] = enable;
		}
	}

	// Disable only if the interface with the correct 
	// handle and unit is enabled.  (RT 467)
	if (!enable && bEnabled[handle][unit])
	{
		status = (0 == acUnInitIF(handle, IF_LPP, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
		//should not care return status.
		bInitialized = false;
		bEnabled[handle][unit] = false;
	}

	return status;
}

acAPI LPP_Config(AC_HANDLE handle, uint8_t unit, LPP_CFG *p_lpp_cfg)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_Config;
	cmd.param[0] = unit;
	cmd.param[1] = p_lpp_cfg->part;  // 2 = Celsius

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_WriteAndRead(AC_HANDLE handle, uint8_t unit, LPP_CFG *p_lpp_cfg)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_ReadWrite;
	cmd.param[0] = unit;
	cmd.param[1] = p_lpp_cfg->part;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
	{
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);
	}

	// debug to see if I can get return data
	uint8_t write_data_buffer[2] = { 0x00, 0x00 };
	uint16_t bytes_to_write = 2;
	if (STAT_OK == status)
	{
		uint32_t count = acPacket_ReadSync(handle, MAKE_IF(LPP_Interface, unit), write_data_buffer, bytes_to_write, (uint16_t)(255 + bytes_to_write));

		nNumberOfBytesRead[handle][unit] = count;
		if (count < bytes_to_write)
		{
			int32_t error_status;
			acPacket_GetIFStatus(handle, MAKE_IF(LPP_Interface, unit), &error_status);
			status = error_status;
		}
	}

	return status;
}

acAPI LPP_ConfigTimer(AC_HANDLE handle, uint8_t unit, uint32_t usecs)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_TIMERS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_ConfigTimer;
	cmd.param[0] = unit;
	cmd.param[1] = usecs;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_StartTimer(AC_HANDLE handle, uint8_t unit)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_TIMERS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_StartTimer;
	cmd.param[0] = unit;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_StopTimer(AC_HANDLE handle, uint8_t unit)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_TIMERS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_StopTimer;
	cmd.param[0] = unit;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_FirePWMs(AC_HANDLE handle, uint8_t unit, uint32_t freq, uint8_t dutyCycle1, uint8_t dutyCycle2, bool syncMode, bool phase180)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.param[0] = unit;
	cmd.param[1] = freq;
	cmd.param[2] = dutyCycle1;
	cmd.param[3] = dutyCycle2;
	cmd.param[4] = syncMode;
	cmd.param[5] = phase180;
	cmd.command = acCmd_LPP_FirePWMs;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_FirePWMsDB(AC_HANDLE handle, uint8_t unit, uint32_t freq, uint8_t dutyCycle1, uint8_t dutyCycle2, uint8_t deadBandRising, uint8_t deadBandFalling, bool divideBy4, bool phase180)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.param[0] = unit;
	cmd.param[1] = freq;
	cmd.param[2] = dutyCycle1;
	cmd.param[3] = dutyCycle2;
	cmd.param[4] = deadBandRising;
	cmd.param[5] = deadBandFalling;
	cmd.param[6] = divideBy4;
	cmd.param[7] = phase180;
	cmd.command = acCmd_LPP_FirePWMs;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_EnableSTM_MPIO1_interrupt(AC_HANDLE handle, uint8_t unit)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_enableSTM_MPIO1_interrupt;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_EnableSTM_MPIO1_Writes(AC_HANDLE handle, uint8_t unit)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_enableSTM_MPIO1_writes;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_EnableCTM_MPIO0_Writes(AC_HANDLE handle, uint8_t unit, uint8_t device)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_enableCTM_MPIO0_writes;
	cmd.param[0] = device;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	return status;
}

acAPI LPP_getWatchdogData(AC_HANDLE handle, uint8_t unit, uint8_t *p_read_data_buffer)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_get_watchdog_data;
	cmd.param[0] = unit;
	cmd.pdata = p_read_data_buffer;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	int bytes_to_read = 16;
	if (STAT_OK == status)
	{
		uint32_t count = acPacket_ReadSync(handle, MAKE_IF(LPP_Interface, unit), p_read_data_buffer, bytes_to_read, (uint16_t)(255 + bytes_to_read));

		nNumberOfBytesRead[handle][unit] = count;
		if (count < (uint32_t)bytes_to_read)
		{
			int32_t error_status;
			acPacket_GetIFStatus(handle, MAKE_IF(LPP_Interface, unit), &error_status);
			status = error_status;
		}
	}

	return status;
}

acAPI LPP_fwRevision(AC_HANDLE handle, uint8_t unit, uint8_t *p_read_data_buffer)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_fw_Rev;
	cmd.param[0] = unit;
	cmd.pdata = p_read_data_buffer;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	int bytes_to_read = 2;
	if (STAT_OK == status)
	{
		uint32_t count = acPacket_ReadSync(handle, MAKE_IF(LPP_Interface, unit), p_read_data_buffer, bytes_to_read, (uint16_t)(255 + bytes_to_read));

		nNumberOfBytesRead[handle][unit] = count;
		if (count < (uint32_t)bytes_to_read)
		{
			int32_t error_status;
			acPacket_GetIFStatus(handle, MAKE_IF(LPP_Interface, unit), &error_status);
			status = error_status;
		}
	}

	return status;
}

acAPI LPP_Processor(AC_HANDLE handle, uint8_t unit, uint8_t *p_read_data_buffer)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	if (unit >= TIVA_LPP_UNITS)
		return OC_ERR_PARAM_OUT_OF_RANGE;

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_Pro;
	cmd.param[0] = unit;
	cmd.pdata = p_read_data_buffer;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(LPP_Interface, unit), 10255);

	int bytes_to_read = 1;
	if (STAT_OK == status)
	{
		uint32_t count = acPacket_ReadSync(handle, MAKE_IF(LPP_Interface, unit), p_read_data_buffer, bytes_to_read, (uint16_t)(255 + bytes_to_read));

		nNumberOfBytesRead[handle][unit] = count;
		if (count < (uint32_t)bytes_to_read)
		{
			int32_t error_status;
			acPacket_GetIFStatus(handle, MAKE_IF(LPP_Interface, unit), &error_status);
			status = error_status;
		}
	}

	return status;
}

acAPI LPP_SpareOne(AC_HANDLE handle, uint8_t unit, uint32_t arg1, uint32_t arg2)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_SpareOne;
	cmd.param[0] = unit;
	cmd.param[1] = arg1;
	cmd.param[2] = arg2;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);

	return status;
}

acAPI LPP_SpareTwo(AC_HANDLE handle, uint8_t unit, uint32_t arg1, uint32_t arg2)
{
	OC_COMMAND cmd;
	STATUS status;
	HANDLE_CHECK(handle);

	LPP_Initialize(&cmd);
	InitCommand(&cmd);
	cmd.if_type = LPP_Interface;
	cmd.if_unit = unit;
	cmd.command = acCmd_LPP_SpareTwo;
	cmd.param[0] = unit;
	cmd.param[1] = arg1;
	cmd.param[2] = arg2;

	status = ocSendCommand(handle, &cmd);
	if (STAT_OK == status)
		status = acWaitForStatus(handle, MAKE_IF(UART_Interface, unit), 50000);

	return status;
}