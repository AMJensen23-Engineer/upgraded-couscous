using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TPS9266xEvaluationModule
{
    class MCUcommands
    {
        // LMM2 Frame Initialization Byte
        public enum FrameInit : byte  // outside of class so limp home object can utilize
        {
            WRITE1 = 0x87,
            WRITE2 = 0x99,
            WRITE3 = 0x1E,
            WRITE4 = 0xAA,
            WRITE5 = 0xAD,  // 664
            WRITE8 = 0x13,  // 664
            WRITE12 = 0x2D,
            WRITE16 = 0x33,
            WRITE20 = 0xB5,  // 664
            WRITE32 = 0xB4,

            READ1 = 0x4B,
            READ2 = 0xCC,
            READ3 = 0xD2,
            READ4 = 0x55,
            READ5 = 0xE5,  // 664
            READ8 = 0x26,  // 664
            READ12 = 0xE1,
            READ16 = 0x66,
            READ20 = 0x7A,  // 664
            READ32 = 0x78,
        };

        // Physical to communications address look-up (including parity)
        public byte[] PHY_TO_COMM = new byte[] 
        {
            //0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            0x20, 0x61, 0xE2, 0xA3, 0x64, 0x25, 0xA6, 0xE7, 0xA8, 0xE9, 0x6A, 0x2B, 0xEC, 0xAD, 0x2E, 0x6F,
            //0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F
            0xF0, 0xB1, 0x32, 0x73, 0xB4, 0xF5, 0x76, 0x37, 0x78, 0x39, 0xBA, 0xFB, 0x3C, 0x7D, 0xFE, 0xBF
        };

        public byte[] PHY_TO_COMM_MTP = new byte[]
        {
            //0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
            0x80, 0xC1, 0x42, 0x03, 0xC4, 0x85, 0x06, 0x47, 0x08, 0x49, 0xCA, 0x8B, 0x4C, 0x0D, 0x8E, 0xCF
        };

        private Queue<byte> txqueue = new Queue<byte>();
        private Queue<byte> rxqueue = new Queue<byte>();
        private MainWindow mainWindow = null;  // get the MainWindow object;

        // To detect redundant calls
        private bool _disposed = false;
        ~MCUcommands() => Dispose(false);

        public MCUcommands(MainWindow master)
        {
            mainWindow = master;

            if (File.Exists("logLMM.txt"))
                File.Delete("logLMM.txt");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                txqueue = null;
                rxqueue = null;
                PHY_TO_COMM = null;
                PHY_TO_COMM_MTP = null;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }

        private void changeEnable(bool? checkState)
        {
            if (checkState == true)
                txqueue.Enqueue(0xFF);
            else
                txqueue.Enqueue(0x00);
        }

        public SPIcommadReturnData sendSPI(bool write, byte regAddr, byte DeviceData, byte addressDevice)
        {
            string readWrite;
            SPIcommadReturnData scrd = new SPIcommadReturnData();
#if DEBUG
            scrd.statusMessage = "Debug enabled!\n";
#else
            byte[] data = assembleSPICmd_682(write, regAddr, DeviceData);
            mainWindow.updateStatus("MOSI: 0x" + data[1].ToString("X2") + data[0].ToString("X2") + "\r\n");
            data = mainWindow.ac.writeReadSPI(2, data);            

            if (write)
                readWrite = "Write of";
            else
                readWrite = "Read of";
            scrd.info = readWrite + " register " + regAddr + ", 0x" + regAddr.ToString("X2") + " data " + DeviceData + ", 0x" + DeviceData.ToString("X2") + " device " + addressDevice + ", 0x" + addressDevice.ToString("X2") + ".\r\n";

            UInt16 assembledReturn = ((UInt16)((UInt16)data[1] << Convert.ToUInt16(8)));  // get SPI status
            assembledReturn &= (UInt16)0xFF00;  // status byte
            assembledReturn |= data[0];  // data byte

            scrd.returnData = "0x" + assembledReturn.ToString("X4");
            scrd.assembledReturn = assembledReturn;
            scrd.statusMessage = "Received SPI: 0x" + assembledReturn.ToString("X4") + ".\r\n";
#endif
            return scrd;
        }

        // this will send 4 bytes in the command so there is no need to push read 2x
        public SPIcommadReturnData sendSPI4(bool write, byte regAddr, byte DeviceData, byte addressDevice)
        {
            string readWrite;
            SPIcommadReturnData scrd = new SPIcommadReturnData();

            //           byte[] data = assembleSPICmd_682(write, regAddr, DeviceData);
            //         data = mainWindow.ac.writeReadSPI(2, data);

            //            byte[] data = assembleSPICmd_682(write, regAddr, DeviceData);
            //          data = mainWindow.ac.writeReadSPI(2, data);


            var data2 = assembleSPICmd_682(write, regAddr, DeviceData);

            byte[] data = new byte[4];
            data[0] = data2[0];
            data[1] = data2[1];
            data[2] = data2[0];
            data[3] = data2[1];
            data = mainWindow.ac.writeReadSPI(4, data);

            if (write)
                readWrite = "Write of";
            else
                readWrite = "Read of";
            scrd.info = readWrite + " register " + regAddr + ", 0x" + regAddr.ToString("X2") + " data " + DeviceData + ", 0x" + DeviceData.ToString("X2") + " device " + addressDevice + ", 0x" + addressDevice.ToString("X2") + ".\r\n";

            UInt16 assembledReturn = ((UInt16)((UInt16)data[3] << Convert.ToUInt16(8)));  // get SPI status
            assembledReturn &= (UInt16)0xFF00;  // status byte
            assembledReturn |= data[2];  // data byte

            scrd.returnData = "0x" + assembledReturn.ToString("X4");
            scrd.assembledReturn = assembledReturn;
            scrd.statusMessage = "Received SPI: 0x" + assembledReturn.ToString("X4") + ".\r\n";
            return scrd;
        }

        public SPIcommadReturnData sendSPI(bool write, byte regAddr, UInt16 DeviceData, byte addressDevice)
        {

            string readWrite;
            SPIcommadReturnData scrd = new SPIcommadReturnData();

#if DEBUG
            scrd.statusMessage = "Debug enabled!\n";
#else
            byte[] data = assembleSPICmd_518(write, regAddr, DeviceData);
            data = mainWindow.ac.writeReadSPI(2, data);
            mainWindow.updateStatus("MOSI 0x" + data[0] + data[1] + "\r\n");

            if (write)
                readWrite = "Write of";
            else
                readWrite = "Read of";
            scrd.info = readWrite + " register " + regAddr + ", 0x" + regAddr.ToString("X2") + " data " + DeviceData + ", 0x" + DeviceData.ToString("X2") + " device " + addressDevice + ", 0x" + addressDevice.ToString("X2") + ".\r\n";

            UInt16 assembledReturn = ((UInt16)((UInt16)data[1] << Convert.ToUInt16(8)));  // get SPI status
            assembledReturn &= (UInt16)0xFF00;  // status byte
            assembledReturn |= data[0];  // data byte

            scrd.returnData = "0x" + assembledReturn.ToString("X4");
            scrd.assembledReturn = assembledReturn;
            scrd.statusMessage = "Received SPI: 0x" + assembledReturn.ToString("X4") + ".\r\n";
#endif
            return scrd;
        }

        private byte[] assembleSPICmd_518(bool write, UInt16 address, UInt16 data)
        {
            UInt16 assembledCmd = 0; // Build this to shift through parity calculation
            UInt16 parity = 0;       // Parity bit calculated here
            UInt16 packet = 0;       // This will be what we send
            byte[] dllFormat = new byte[] { 0, 0 };

            if (write)
                assembledCmd |= 0x8000;  // Set CMD = 1

            assembledCmd |= (UInt16)(((address << 10) & 0x7C00) | (data & 0x01FF));
            packet = assembledCmd;

            while (assembledCmd > 0)  // Calculate parity
            {
                if ((assembledCmd & 0x0001) == 1)  // Count the number of 1s in the LSb
                    parity++;

                assembledCmd >>= 1;  // Shift right
            }

            if ((parity & 0x0001) != 1)  // If the LSb is a 0 (even # of 1s), we need to add the odd parity bit (add / set parity bit for an odd number of bits total, if we already have an odd number do not set parity bit)
                packet |= (1 << 9);

            // must reverse bytes for LaunchPad DLL
            dllFormat[0] = (byte)(packet & 0x00FF);
            dllFormat[1] = (byte)((packet & 0xFF00) >> 8);

            return (dllFormat);
        }

        private byte[] assembleSPICmd_682(bool write, UInt16 address, byte data)
        {
            UInt16 assembledCmd = 0; // Build this to shift through parity calculation
            UInt16 parity = 0;       // Parity bit calculated here
            UInt16 packet = 0;       // This will be what we send
            byte[] dllFormat = new byte[] { 0, 0 };

            if (write)
                assembledCmd |= 0x8000;  // Set CMD = 1

            assembledCmd |= (UInt16)(((address << 9) & 0x7E00) | (data & 0x00FF));
            packet = assembledCmd;

            while (assembledCmd > 0)  // Calculate parity
            {
                if ((assembledCmd & 0x0001) == 1)  // Count the number of 1s in the LSb
                    parity++;

                assembledCmd >>= 1;  // Shift right
            }

            if ((parity & 0x0001) != 1)  // If the LSb is a 0 (even # of 1s), we need to add the odd parity bit (add / set parity bit for an odd number of bits total, if we already have an odd number do not set parity bit)
                packet |= (1 << 8);

            // must reverse bytes for LaunchPad DLL
            dllFormat[0] = (byte)(packet & 0x00FF);
            dllFormat[1] = (byte)((packet & 0xFF00) >> 8);

            return (dllFormat);
        }

        public void LmmWrReg(FrameInit command, byte lmm, byte regAddr, byte[] data_ptr, ref string LmmRdWriteStatus, bool mtpDevId, bool ackEnabled)
        {
#if DEBUG
            return;
#else
            UInt16 crc = 0;
            byte numDataBytes;

            if (Globals.watchdogActive)
                stopUARTtimer(lmm);

            Globals.uartBuf[0] = (byte)command;  // Assemble the packet in the pre-buffer and CRC it
            if (mtpDevId)
                Globals.uartBuf[1] = PHY_TO_COMM_MTP[lmm];
            else
                Globals.uartBuf[1] = PHY_TO_COMM[lmm];
            Globals.uartBuf[2] = regAddr;
            string lmmAddressChan = "Device " + lmm.ToString() + " (0x" + Globals.uartBuf[1].ToString("X") + ") ";

            switch (command)
            {
                case FrameInit.WRITE1:
                    numDataBytes = 1;
                    break;

                case FrameInit.WRITE2:
                    numDataBytes = 2;
                    break;

                case FrameInit.WRITE3:
                    numDataBytes = 3;
                    break;

                case FrameInit.WRITE5:
                    numDataBytes = 5;
                    break;

                case FrameInit.WRITE4:
                    numDataBytes = 4;
                    break;

                case FrameInit.WRITE8:
                    numDataBytes = 8;
                    break;

                case FrameInit.WRITE12:
                    numDataBytes = 12;
                    break;

                case FrameInit.WRITE16:
                    numDataBytes = 16;
                    break;

                case FrameInit.WRITE20:
                    numDataBytes = 20;
                    break;

                case FrameInit.WRITE32:
                    numDataBytes = 32;
                    break;

                default:
                    numDataBytes = 0;
                    break;
            }

            for (crc = 0; crc < numDataBytes; crc++)  // Just use "crc" variable because it's available
                Globals.uartBuf[(3 + crc)] = data_ptr[crc];

            crc = Crc_16_Ibm(Globals.uartBuf, (byte)(3 + numDataBytes));  // Process the CRC over these bytes

            // And load it into the final 2 bytes of the packet
            Globals.uartBuf[(3 + numDataBytes)] = (byte)(crc & 0x00FF);         // LSByte of CRC
            Globals.uartBuf[(4 + numDataBytes)] = (byte)((crc >> 8) & 0x00FF);  // MSByte of CRC

            int result = mainWindow.ac.writeUART((UInt16)(5 + numDataBytes), Globals.uartBuf);  // Now we can send it to the TPS92662 bus

            for (int i = 0; i < (5 + numDataBytes); i++)
                File.AppendAllText(Directory.GetCurrentDirectory() + "\\logLmm.txt", Globals.uartBuf[i].ToString("X2"));
            File.AppendAllText(Directory.GetCurrentDirectory() + "\\logLmm.txt", "\n");

            if (result < 0)
                LmmRdWriteStatus = lmmAddressChan + "Write " + numDataBytes.ToString() + " Byte Command 0x" + command.ToString("X") + " failed to write number of bytes expected. Error Code: " + result.ToString();
            else
            {
                LmmRdWriteStatus = lmmAddressChan + "Write " + numDataBytes.ToString() + " Byte Command 0x" + command.ToString("X") + " completed successfully.\nWrite Output Data: ";
                for (int i = 3; i < numDataBytes + 3; i++)
                    LmmRdWriteStatus += " 0x" + Globals.uartBuf[i].ToString("X2") + ",";
                LmmRdWriteStatus += " CRC: 0x" + Globals.uartBuf[3 + numDataBytes].ToString("X2") + ", 0x" + Globals.uartBuf[4 + numDataBytes].ToString("X2");
                if (ackEnabled)
                    LmmRdWriteStatus += ", ACK: 0x" + Globals.uartBuf[5 + numDataBytes].ToString("X2");
            }

            LmmRdWriteStatus += "\nLMM, ";
            for (int i = 0; i < numDataBytes + 5; i++)
                LmmRdWriteStatus += "0x" + Globals.uartBuf[i].ToString("X2") + " ";

            int totBytes = mainWindow.ac.controlUART(2, 0);
            if (totBytes > 0)
                mainWindow.ac.readUART((byte)(totBytes), Globals.uartBuf);

            if ((Globals.bothGenDevicesPresent) && ((command == FrameInit.WRITE5) || (command == FrameInit.WRITE8) || (command == FrameInit.WRITE20)))
                lmmCommsReset();

            if (Globals.watchdogActive)
                startUARTtimer(lmm);
#endif
        }

        public void LmmWrReg(FrameInit command, byte lmm, byte regAddr, byte[] data_ptr, ref string LmmRdWriteStatus, bool mtpDevId, bool ackEnabled, bool badCRC)
        {
#if DEBUG
            return;
#else
            UInt16 crc = 0;
            byte numDataBytes;

            if (Globals.watchdogActive)
                stopUARTtimer(lmm);

            Globals.uartBuf[0] = (byte)command;  // Assemble the packet in the pre-buffer and CRC it
            if (mtpDevId)
                Globals.uartBuf[1] = PHY_TO_COMM_MTP[lmm];
            else
                Globals.uartBuf[1] = PHY_TO_COMM[lmm];
            Globals.uartBuf[2] = regAddr;
            string lmmAddressChan = "Device " + lmm.ToString() + " (0x" + Globals.uartBuf[1].ToString("X") + ") ";

            switch (command)
            {
                case FrameInit.WRITE1:
                    numDataBytes = 1;
                    break;

                case FrameInit.WRITE2:
                    numDataBytes = 2;
                    break;

                case FrameInit.WRITE3:
                    numDataBytes = 3;
                    break;

                case FrameInit.WRITE5:
                    numDataBytes = 5;
                    break;

                case FrameInit.WRITE4:
                    numDataBytes = 4;
                    break;

                case FrameInit.WRITE8:
                    numDataBytes = 8;
                    break;

                case FrameInit.WRITE12:
                    numDataBytes = 12;
                    break;

                case FrameInit.WRITE16:
                    numDataBytes = 16;
                    break;

                case FrameInit.WRITE20:
                    numDataBytes = 20;
                    break;

                case FrameInit.WRITE32:
                    numDataBytes = 32;
                    break;

                default:
                    numDataBytes = 0;
                    break;
            }

            for (crc = 0; crc < numDataBytes; crc++)  // Just use "crc" variable because it's available
                Globals.uartBuf[(3 + crc)] = data_ptr[crc];

            crc = Crc_16_Ibm(Globals.uartBuf, (byte)(3 + numDataBytes));  // Process the CRC over these bytes

            // And load it into the final 2 bytes of the packet
            if (!badCRC)
            {
                Globals.uartBuf[(3 + numDataBytes)] = (byte)(crc & 0x00FF);         // LSByte of CRC
                Globals.uartBuf[(4 + numDataBytes)] = (byte)((crc >> 8) & 0x00FF);  // MSByte of CRC
            }
            else
            {
                Globals.uartBuf[(3 + numDataBytes)] = (byte)(~crc & 0x00FF);         // LSByte of CRC
                Globals.uartBuf[(4 + numDataBytes)] = (byte)((~crc >> 8) & 0x00FF);  // MSByte of CRC
            }
         
            int result = mainWindow.ac.writeUART((UInt16)(5 + numDataBytes), Globals.uartBuf);  // Now we can send it to the TPS92662 bus

            for (int i = 0; i < (5 + numDataBytes); i++)
                File.AppendAllText(Directory.GetCurrentDirectory() + "\\logLmm.txt", Globals.uartBuf[i].ToString("X2"));
            File.AppendAllText(Directory.GetCurrentDirectory() + "\\logLmm.txt", "\n");

            if (result < 0)
                LmmRdWriteStatus = lmmAddressChan + "Write " + numDataBytes.ToString() + " Byte Command 0x" + command.ToString("X") + " failed to write number of bytes expected. Error Code: " + result.ToString();
            else
            {
                LmmRdWriteStatus = lmmAddressChan + "Write " + numDataBytes.ToString() + " Byte Command 0x" + command.ToString("X") + " completed successfully.\nWrite Output Data: ";
                for (int i = 3; i < numDataBytes + 3; i++)
                    LmmRdWriteStatus += " 0x" + Globals.uartBuf[i].ToString("X2") + ",";
                LmmRdWriteStatus += " CRC: 0x" + Globals.uartBuf[3 + numDataBytes].ToString("X2") + ", 0x" + Globals.uartBuf[4 + numDataBytes].ToString("X2");
                if(ackEnabled)
                    LmmRdWriteStatus += ", ACK: 0x" + Globals.uartBuf[5 + numDataBytes].ToString("X2");
            }

            LmmRdWriteStatus += "\nLMM, ";
            for (int i = 0; i < numDataBytes + 5; i++)
                LmmRdWriteStatus += "0x" + Globals.uartBuf[i].ToString("X2") + " ";

            int totBytes = mainWindow.ac.controlUART(2, 0);
            if (totBytes > 0)
                mainWindow.ac.readUART((byte)(totBytes), Globals.uartBuf);

            if ((Globals.bothGenDevicesPresent) && ((command == FrameInit.WRITE5) || (command == FrameInit.WRITE8) || (command == FrameInit.WRITE20)))
                lmmCommsReset();

            if (Globals.watchdogActive)
                startUARTtimer(lmm);
#endif
        }     

        public int LmmRdReg(FrameInit command, byte lmm, byte regAddr, ref string LmmRdReadStatus, bool mtpDevId, TextBox textBoxReadData)
        {
#if DEBUG
            return 0;
#else
            UInt16 crc = 0;
            byte lmmBytesExpected;

            if(Globals.watchdogActive)
                stopUARTtimer(lmm);

            // Assemble the packet in the pre-buffer and CRC it
            Globals.uartBuf[0] = (byte)command;
            if (mtpDevId)
                Globals.uartBuf[1] = PHY_TO_COMM_MTP[lmm];
            else
                Globals.uartBuf[1] = PHY_TO_COMM[lmm];
            Globals.uartBuf[2] = regAddr;
            string lmmAddressChan = "Device " + lmm.ToString() + " (0x" + Globals.uartBuf[1].ToString("X") + ") ";
            if (textBoxReadData != null)
                textBoxReadData.Text = "";

            switch (command)
            {
                case FrameInit.READ1:
                    lmmBytesExpected = (1 + 2);
                    break;

                case FrameInit.READ2:
                    lmmBytesExpected = (2 + 2);
                    break;

                case FrameInit.READ3:
                    lmmBytesExpected = (3 + 2);
                    break;

                case FrameInit.READ4:
                    lmmBytesExpected = (4 + 2);
                    break;

                case FrameInit.READ5:
                    lmmBytesExpected = (5 + 2);
                    break;

                case FrameInit.READ8:
                    lmmBytesExpected = (8 + 2);
                    break;

                case FrameInit.READ12:
                    lmmBytesExpected = (12 + 2);
                    break;

                case FrameInit.READ16:
                    lmmBytesExpected = (16 + 2);
                    break;

                case FrameInit.READ20:
                    lmmBytesExpected = (20 + 2);
                    break;

                case FrameInit.READ32:
                    lmmBytesExpected = (32 + 2);
                    break;

                default:
                    lmmBytesExpected = 0;
                    break;
            }
           
            crc = Crc_16_Ibm(Globals.uartBuf, 3);  // Process the CRC over the INIT, DEVID, and REGADDR bytes

            // And load it into the final 2 bytes of the packet
            Globals.uartBuf[3] = (byte)(crc & 0x00FF);         // LSByte of CRC
            Globals.uartBuf[4] = (byte)((crc >> 8) & 0x00FF);  // MSByte of CRC

            int numBytesInReadBuffer;
            string commandString = null;
            int result = mainWindow.ac.writeUART(5, Globals.uartBuf);  // Send the write command
            for (int i = 0; i < 5; i++)
                commandString += "0x" + Globals.uartBuf[i].ToString("X2") + " ";

            bool commandEchoed = false;
            if ((numBytesInReadBuffer = mainWindow.ac.controlUART(2, 0)) > (lmmBytesExpected + 4))
                commandEchoed = true;
            if ((result > -1) && commandEchoed)  // read back the command only when echoed
            {
                result = mainWindow.ac.readUART((byte)5, Globals.uartBuf);
                commandEchoed = true;
            }
            else if (result < 0)
            {
                numBytesInReadBuffer = mainWindow.ac.controlUART(2, 0);
                if (numBytesInReadBuffer > 0)
                    mainWindow.ac.readUART((byte)numBytesInReadBuffer, Globals.uartBuf);

                LmmRdReadStatus = lmmAddressChan + "Read " + (lmmBytesExpected - 2).ToString() + " Byte Command 0x" + command.ToString("X") + " failed. Error Code: " + result.ToString();
            }

            numBytesInReadBuffer = mainWindow.ac.controlUART(2, 0);
            if ((result > -1) && (numBytesInReadBuffer == lmmBytesExpected))  // get the read data returned and CRC
            {
                for (int i = 0; i < 5; i++)
                    File.AppendAllText(Directory.GetCurrentDirectory() + "\\logLmm.txt", Globals.uartBuf[i].ToString("X2"));

                result = mainWindow.ac.readUART(lmmBytesExpected, Globals.uartBuf);  // Read the data back; re-use uartBuf to store read data                

                if ((result > -1) && !validateCrc(0, Globals.uartBuf, lmmBytesExpected))  // check returned CRC
                    LmmRdReadStatus = lmmAddressChan + "Read " + (lmmBytesExpected - 2).ToString() + " Byte Command 0x" + command.ToString("X") + " failed CRC check.";
                else
                {
                    LmmRdReadStatus = lmmAddressChan + "Read " + (lmmBytesExpected - 2).ToString() + " Byte Command 0x" + command.ToString("X") + " completed successfully.\nRead Return Data:";
                    for (int i = 0; i < lmmBytesExpected - 2; i++)
                    {
                        LmmRdReadStatus += " 0x" + Globals.uartBuf[i].ToString("X2") + ",";
                        if (textBoxReadData != null)
                            textBoxReadData.Text += Globals.uartBuf[i].ToString("X2") + " ";
                    }
                    LmmRdReadStatus += " CRC: 0x" + Globals.uartBuf[lmmBytesExpected - 2].ToString("X2") + " 0x" + Globals.uartBuf[lmmBytesExpected - 1].ToString("X2");

                    for (int i = 0; i < lmmBytesExpected; i++)
                        File.AppendAllText(Directory.GetCurrentDirectory() + "\\logLmm.txt", Globals.uartBuf[i].ToString("X2"));
                    File.AppendAllText(Directory.GetCurrentDirectory() + "\\logLmm.txt", "\n");
                }
            }
            else
            {
                if (numBytesInReadBuffer > 0)
                    result = mainWindow.ac.readUART((byte)numBytesInReadBuffer, Globals.uartBuf);
                LmmRdReadStatus = lmmAddressChan + "Read " + (lmmBytesExpected - 2).ToString() + " Byte Command 0x" + command.ToString("X") + " failed to read number of bytes expected. Error: F" + numBytesInReadBuffer + "-E" + lmmBytesExpected + "-R" + result;  // found; expected; read
            }
            LmmRdReadStatus += "\nLMM, " + commandString;

            if ((Globals.bothGenDevicesPresent) && ((command == FrameInit.READ5) || (command == FrameInit.READ8) || (command == FrameInit.READ20)))
                lmmCommsReset();

            if (Globals.watchdogActive)
                startUARTtimer(lmm);

            return result;
#endif
        }

        public void lmmCommsReset()
        {
      //      return;
            mainWindow.ac.disableUART();
            int status = mainWindow.ac.initGPIO("PC5", false, 1);  // set UART Tx line low this will set the receiving tranciever's Rx UART line low which will reset the LMM bus
  //          mainWindow.ac.setStateGPIO("PC5", false);
            Thread.Sleep(10);
      //      mainWindow.ac.setStateGPIO("PC5", true);
            mainWindow.ac.enableUART();
            status = mainWindow.ac.configUART(Properties.Settings.Default.baudRate, false);
        }

        private bool validateCrc(byte start, byte[] buffer, byte length)
        {
            UInt16 crc = 0;
            UInt16 j;

            for (int i = start; i < length; i++)
            {
                crc ^= buffer[i];
                for (j = 0; j <= 7; j++)
                    crc = (UInt16)((UInt16)(crc >> 1) ^ (UInt16)((Convert.ToBoolean(crc & 1)) ? 0xA001 : 0x0000));
            }

            if (crc == 0x0000)
                return (true);  // Good packet
            else
                return (false);
        }

        private UInt16 Crc_16_Ibm(byte[] buf, byte len)
        {
            UInt16 crc = 0;
            UInt16 j;

            for(int i = 0; i < len; i++)
            {
                crc ^= buf[i];
                for (j = 0; j < 8; j++)
                    crc = (UInt16)((UInt16)(crc >> 1) ^ (UInt16)((Convert.ToBoolean(crc & 1)) ? 0xA001 : 0x0000));
            }

            return crc;
        }

        public void startUARTtimer(byte deviceLMM)
        {
            mainWindow.ac.startTimerUART(PHY_TO_COMM[deviceLMM]);
        }

        public void stopUARTtimer(byte deviceLMM)
        {
            mainWindow.ac.stopTimerUART(PHY_TO_COMM[deviceLMM]);
        }

        public int closeUARTtimer(byte unit)
        {
            return mainWindow.ac.closeTimerUART(unit);
        }

        public int setBaud(UInt32 baud)
        {
            return mainWindow.ac.configUART(Properties.Settings.Default.baudRate, false);
        }
    }
}
