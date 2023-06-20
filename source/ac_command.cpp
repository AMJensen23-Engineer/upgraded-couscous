#include <stdlib.h>
#include <memory.h>


#include "acctrl.h"
#include "acctrl_api.h"


void InitCommand(OC_COMMAND *cmd)
{
    memset(cmd, 0, sizeof(OC_COMMAND));
    cmd->size = sizeof(OC_COMMAND);
}

STATUS InitInterface(AC_HANDLE handle, uint8_t _interface, uint8_t unit, bool enable)
{
    STATUS status;

    if (enable)
    {
        status = (0 == acInitIF(handle, _interface, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
    }
    else 
    {
        status = (0 == acUnInitIF(handle, _interface, unit)) ? STAT_OK : OC_ERR_OPERATION_FAILED;
    }

    return status;
}

STATUS ocSendCommand(AC_HANDLE handle, OC_COMMAND *pCmd)
{
    STATUS status = STAT_OK;

	HANDLE_CHECK(handle);

    if (pCmd->data_len > MAX_PAYLOAD)
    {
        status = OC_ERR_TOO_MUCH_DATA;
    }
    else
    {
        OCF_PACKET packet;
        uint32_t nBytesWritten;
        uint32_t len;

        packet.ph.signature = PACKET_SIGNATURE;
        packet.ph.type = COMMAND_PACKET;
        packet.ph.transfer_len = 0;
        packet.ph.packet_num = 1;
        packet.ph.status = 0;
        packet.ph.payload_len = pCmd->data_len;
        packet.ph.if_type_unit = (pCmd->if_type << 8) + pCmd->if_unit;
        packet.ph.command = pCmd->command;
        memcpy(packet.ph.param, pCmd->param, sizeof(packet.ph.param));

        len = sizeof(OCF_PACKET_HEADER) + packet.ph.payload_len;
		if (pCmd->data_len)
        {
            memcpy(packet.payload, pCmd->pdata, pCmd->data_len);
        }

        // reset and clear out any old status
        acPacket_ResetIFStatus(handle, packet.ph.if_type_unit);

        // end the command
        nBytesWritten = acPacket_Write(handle, (uint8_t *) &packet, len);
        if (nBytesWritten < len)
            status = OC_ERR_SYS_COM_WRITE_ERROR;
    }

    return status;
}

STATUS ocSendCommand_BigData(AC_HANDLE handle, OC_COMMAND *pCmd)
{
    STATUS status = STAT_OK;

	HANDLE_CHECK(handle);

        OCF_PACKET *p_packet;
        uint32_t nBytesWritten;
        uint32_t len;

        unsigned char *pBuff= (unsigned char*)malloc( pCmd->data_len + sizeof(OCF_PACKET));
		if(pBuff)
		{
		p_packet = (OCF_PACKET *)pBuff; 
	    p_packet->ph.signature = PACKET_SIGNATURE;
        p_packet->ph.type = COMMAND_PACKET;
        p_packet->ph.transfer_len = pCmd->data_len;
        p_packet->ph.packet_num = 1;
        p_packet->ph.status = 0;
        p_packet->ph.payload_len = pCmd->data_len;
        p_packet->ph.if_type_unit = (pCmd->if_type << 8) + pCmd->if_unit;
        p_packet->ph.command = pCmd->command;
        memcpy(p_packet->ph.param, pCmd->param, sizeof(p_packet->ph.param));
	
		}
		
		
        len = sizeof(OCF_PACKET_HEADER) + p_packet->ph.transfer_len;
        
		
		if (pCmd->data_len)
        {
            memcpy(pBuff +sizeof(OCF_PACKET_HEADER), pCmd->pdata, pCmd->data_len);
        }

        // reset and clear out any old status
        acPacket_ResetIFStatus(handle, p_packet->ph.if_type_unit);

        // end the command
        nBytesWritten = acPacket_Write(handle, (uint8_t *) pBuff, len);

        if (nBytesWritten < len)
            status = OC_ERR_SYS_COM_WRITE_ERROR;
    
    return status;
}


STATUS WaitForStatus(AC_HANDLE handle, uint16_t IF, int32_t timeout)
{
    STATUS status = OC_ERR_SYS_UNKNOWN_STATUS;

    while (timeout > 0)
    {
        if (STAT_OK == acPacket_GetIFStatus(handle, IF, &status))
        {
            break;
        }

        ac_OS_Sleep(1);
        timeout -= 1;
    }

	if(timeout <= 0)
	{
	    //time out, flush the write buffer 
	    acPacket_WriteFlush(handle);
    }

    return (timeout > 0) ? status : OC_ERR_TIMEOUT;
}