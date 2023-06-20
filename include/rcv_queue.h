
#ifndef __RCV_QUEUE_H
  #define __RCV_QUEUE_H

#define MAX_STATUS_STR_LEN 256
#ifdef  __cplusplus
extern "C" {
#endif

typedef enum
{
    QUEUE_STATE_IDLE,               //queue is empty
    QUEUE_STATE_FILLED         //queue is full
 } RCV_QUEUE_STATE;



/*global dta stucture definition*/
/*interface receive queue*/
typedef struct  
{
    // uint16_t IF;//the queue belongs to interface  
	 int16_t      Status;    // Status code returned by command function
     int8_t1       ErrorStr[MAX_STATUS_STR_LEN]; //from firmware 
	 uint16_t     Command;       // Command code
	 uint8_t  *pData;      //the data buffer 
     uint32_t  WriteIndex;
	 uint32_t  ReadIndex;
	 uint32_t  DataSize; //the buffer data size
     uint32_t  CurBuffSize; //the buffer data size
     RCV_QUEUE_STATE State;

     void (*pfDataReadyCallBack)(uint8_t *pData, uint16_t DataSize); //callback routine to notify the caller, this is one time callbck
	 uint8_t  *pCallbackBuff;      //the data buffer 
     uint32_t  CallbackDataSize;
     void (*pfIntCallBack)(uint8_t *pData, uint16_t DataSize); //interrupt callback to notify the caller
	 uint8_t  *pIntCallbackBuff;      //the data buffer 
     uint32_t  IntCallbackDataSize;


} RCV_QUEUE;
  


class ocRcvQueue
{
public:
	ocRcvQueue(int32_t oc_handle){ this->oc_handle = oc_handle;}
    
/** find the receive queue

    @param oc_Handle: OneController handle
	@param IF: Interface type and uint
    return: found queue pointer
*/


RCV_QUEUE  *oc_Find_Rcv_Queue(uint16_t IF);

/** alloc he receive buffer and initalize receive queue state

    @param oc_Handle: OneController handle
	@param DatSize: buffer size
    return: 0, if success
*/

int32_t  oc_Alloc_Rcv_Queue(RCV_QUEUE * Rcv_Queue, uint32_t DatSize);

/** release receive buffer and uninitalize receive queue state

    @param oc_Handle: OneController handle
	@param Rcv_Queue: receive queue pointer
    return: 0, if success
*/
void  oc_Release_Rcv_Queue(RCV_QUEUE * Rcv_Queue);
void  oc_Init_Rcv_Queue(RCV_QUEUE * Rcv_Queue, uint16_t IF);
private:
	int32_t oc_handle;
};
#ifdef  __cplusplus
}
#endif

#endif
