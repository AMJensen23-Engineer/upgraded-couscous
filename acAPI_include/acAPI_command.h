
#ifndef _OC_COMMAND_
#define _OC_COMMAND_


typedef struct
{
    uint32_t    size;           // size of this struct
    uint8_t     if_type;        // Interface type
    uint8_t     if_unit;        // Interface unit
    uint16_t    command;        // Command code
    uint32_t    param[8];       // eight 32-bit parameters
    uint16_t    data_len;       // Total number of bytes in the transfer
    uint8_t     *pdata;         // pointer to the data (NULL if none)
} OC_COMMAND;

void InitCommand(OC_COMMAND *cmd);
STATUS InitInterface(AC_HANDLE handle, uint8_t _interface, uint8_t unit, bool enable);
STATUS ocSendCommand(AC_HANDLE handle, OC_COMMAND *pCmd);
STATUS ocSendCommand_BigData(AC_HANDLE handle, OC_COMMAND *pCmd);
#endif // _OC_COMMAND_
