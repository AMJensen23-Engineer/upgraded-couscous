//*****************************************************************************
//
// OneController pwm functions.
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

#ifndef __OC_PWM_H
#define __OC_PWM_H


#define ACCTRL_PWM_UNITS           4

/*!
 *  @brief   PWM period unit definitions.  Refer to device specific
 *  implementation if using PWM_PERIOD_COUNTS (raw PWM/Timer counts).
 */
typedef enum PWM_Period_Units_ {
    PWM_PERIOD_US,    /* Period in microseconds */
    PWM_PERIOD_HZ,    /* Period in (reciprocal) Hertz
                         (for example 2MHz = 0.5us period) */
 } PWM_Period_Units;

/*!
 *  @brief   PWM duty cycle unit definitions.  Refer to device specific
 *  implementation if using PWM_DUTY_COUNTS (raw PWM/Timer counts).
 */
typedef enum PWM_Duty_Units_ {
    PWM_DUTY_US,       /* Duty cycle in microseconds */
    PWM_DUTY_FRACTION, /* Duty as a fractional part of PWM_DUTY_FRACTION_MAX */
 
} PWM_Duty_Units;

/*!
 *  @brief   Idle output level when PWM is not running (stopped / not started).
 */
typedef enum PWM_IdleLevel_ {
    PWM_IDLE_LOW  = 0,
    PWM_IDLE_HIGH = 1,
} PWM_IdleLevel;

/*!
 *  @brief PWM Parameters
 *
 *  PWM Parameters are used to with the PWM_open() call. Default values for
 *  these parameters are set using PWM_Params_init().
 *
 *  @sa     PWM_Params_init()
 */
typedef struct PWM_Params_ {
    PWM_Period_Units periodUnits; /*!< Units in which the period is specified */
    uint32_t         periodValue; /*!< PWM initial period */
    PWM_Duty_Units   dutyUnits;   /*!< Units in which the duty is specified */
    uint32_t         dutyValue;   /*!< PWM initial duty */
    PWM_IdleLevel    idleLevel;   /*!< Pin output when PWM is stopped. */
} PWM_Params;


#ifdef  __cplusplus
extern "C" {
#endif

/**
* This function is used to enable or disable an pwm interface unit.
* @param handle An open handle for communications 
* @param unit The number of the unit to be enabled/disabled.
* @param enable Set true to enable, or false to disable, the I2C unit.
* @date 10/4/2015
*/
acAPI PWM_Enable(AC_HANDLE handle, uint8_t unit, bool enable);

/**
* This function is used to configure an spi interface unit.
* @param handle An open handle for communications 
* @param unit The number of the unit to be configured.
* @param params : pwm configuration 
* @date 10/4/2015
*/
acAPI PWM_Config(AC_HANDLE handle, uint8_t unit,PWM_Params *params);


/**
* This function is used to start a pwm .

* @param handle An open handle for communications 
* @param unit The number of the unit to be started.
* @date 10/4/2015
*/
acAPI PWM_Start(AC_HANDLE handle, uint8_t unit);

/**
* This function is used to stop a pwm.

* @param handle An open handle for communications 
* @param unit The number of the unit to be stoped.
* @date 10/4/2015
*/
acAPI PWM_Stop(AC_HANDLE handle, uint8_t unit);


#ifdef  __cplusplus
}
#endif


#endif // __OC_UART_H
