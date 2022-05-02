using System;
using System.Text;

using MTSCRANET;
using MTLIB;

namespace MTNETOEMDemo
{
    class MTOEMUartNfc
    {
        private bool mSLIP = true;
        //private bool mSLIP = false;

        private const byte SLIP_PACKET_START_END = (byte)0xC0;
        private const byte SLIP_ESCAPE = (byte)0xDB;
        private const byte SLIP_ESCAPE_C0 = (byte)0xDC;
        private const byte SLIP_ESCAPE_DB = (byte)0xDD;

        private const int MAX_PACKET_SIZE = 4096;

        private bool mPacketStarted = false;
        private bool mLastByteIsPacketStart = false;
        private bool mLastByteIsEscape = false;
        private int mPacketSize = 0;
        private byte[] mPacket = new byte[MAX_PACKET_SIZE];

        private MTSCRA m_SCRA;

        private byte[] m_uartDataReceived;

        private int m_emvDataLen;
        private byte[] m_emvData;

        public delegate void DebugInfoHandler(object sender, string data);

        public event MTSCRA.DeviceResponseHandler OnDeviceResponse;
        public event MTSCRA.TransactionStatusHandler OnTransactionStatus;
        public event MTSCRA.DisplayMessageRequestHandler OnDisplayMessageRequest;
        public event MTSCRA.UserSelectionRequestHandler OnUserSelectionRequest;
        public event MTSCRA.ARQCReceivedHandler OnARQCReceived;
        public event MTSCRA.TransactionResultHandler OnTransactionResult;

        public event DebugInfoHandler OnDebugInfo;

        public MTOEMUartNfc(MTSCRA scra)
        {
            m_SCRA = scra;

            m_uartDataReceived = null;

            m_emvDataLen = 0;
            m_emvData = null;

            if (mSLIP)
            {
                resetSLIPDecoder();
            }
        }

        private byte[] buildExtendedCommand(byte[] command, byte[] data)
        {
            byte[] commandBytes = null;

            if ((command != null) && (command.Length >= 2) && (data != null))
            {
                int dataLen = data.Length;

                commandBytes = new byte[dataLen + 4];

                commandBytes[0] = command[0];
                commandBytes[1] = command[1];
                commandBytes[2] = (byte)((dataLen >> 8) & 0xFF);
                commandBytes[3] = (byte)(dataLen & 0xFF);

                Array.Copy(data, 0, commandBytes, 4, data.Length);
            }

            return commandBytes;
        }

        protected void sendDebugInfo(string data)
        {
            if (OnDebugInfo != null)
            {
                OnDebugInfo(this, data);
            }
        }

        public int sendExtendedCommandBytes(byte[] command, byte[] data)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;

            const int MAX_DATA_LEN = 60;
//            const int MAX_DATA_LEN = 30;

            int commandLen = 0;
            int dataLen = 0;

            if (command != null)
            {
                commandLen = command.Length;
            }

            if (commandLen != 2)
            {
                return result;
            }

            if (data != null)
            {
                dataLen = data.Length;
            }

            int offset = 0;

            while ((offset < dataLen) || (dataLen == 0))
            {
                int len = dataLen - offset;

                if (len >= (MAX_DATA_LEN - 8))
                {
                    len = MAX_DATA_LEN - 9;
                }

                byte[] extendedCommand = new byte[8 + len];

                extendedCommand[0] = MTEMVDeviceConstants.PROTOCOL_EXTENDER_REQUEST;
                extendedCommand[1] = (byte)(6 + len);
                extendedCommand[2] = (byte)((offset >> 8) & 0xFF);
                extendedCommand[3] = (byte)(offset & 0xFF);
                extendedCommand[4] = command[0];
                extendedCommand[5] = command[1];
                extendedCommand[6] = (byte)((dataLen >> 8) & 0xFF);
                extendedCommand[7] = (byte)(dataLen & 0xFF);

                for (int i = 0; i < len; i++)
                {
                    extendedCommand[8 + i] = data[offset + i];
                }

                offset += len;

                result = sendData(MTParser.getHexString(extendedCommand));

                if (result ==  MTSCRA.SEND_COMMAND_ERROR)
                {
                    return result;
                }

                if (dataLen == 0)
                    break;
            }

            return result;
        }

        public int sendData(string data)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;

            if (m_SCRA != null)
            {
                byte[] uartData = null;

                byte[] dataBytes = MTParser.getByteArrayFromHexString(data);

                if ((dataBytes != null) && (dataBytes.Length > 0))
                {
                    int dataLen = dataBytes.Length;

                    byte[] commandData = new byte[dataLen + 3];
                    commandData[0] = 0x05;
                    commandData[1] = (byte)((dataLen >> 8) & 0xFF);
                    commandData[2] = (byte)(dataLen & 0xFF);
                    commandData[3] = 0;
                    Array.Copy(dataBytes, 0, commandData, 3, dataLen);

                    if (mSLIP)
                    {
                        uartData = buildSLIPData(commandData);
                    }
                    else
                    {
                        uartData = buildASCIIData(commandData);
                    }
                }

                byte[] extData = new byte[] { 0 };

                if (uartData != null)
                {
                    extData = new byte[uartData.Length + 1];

                    extData[0] = 0;
                    Array.Copy(uartData, 0, extData, 1, uartData.Length);
                }

                byte[] UART_COMMAND = new byte[] { 0x04, 0x00 };
                byte[] uartCommand = buildExtendedCommand(UART_COMMAND, extData);

                string extendedCommandString = MTParser.getHexString(uartCommand);

                sendDebugInfo("Send UART Extended Command: " + extendedCommandString);

                result = m_SCRA.sendExtendedCommand(extendedCommandString);

                if (result != 0)
                {
                    sendDebugInfo("SendExtendedCommand error=" + result);
                }
            }

            return result;
        }

        private byte[] buildSLIPData(byte[] data)
        {
            byte[] packet = new byte[2048];
            int packetSize = 0;

            if (data != null)
            {
                int len = data.Length;
                int i = 0;
                int j = 0;

                packet[j++] = SLIP_PACKET_START_END;

                while (i < len)
                {
                    if (data[i] == SLIP_PACKET_START_END)
                    {
                        packet[j++] = SLIP_ESCAPE;
                        packet[j++] = SLIP_ESCAPE_C0;
                    }
                    else if (data[i] == SLIP_ESCAPE)
                    {
                        packet[j++] = SLIP_ESCAPE;
                        packet[j++] = SLIP_ESCAPE_DB;
                    }
                    else
                    {
                        packet[j++] = data[i];
                    }

                    i++;
                }

                packet[j++] = SLIP_PACKET_START_END;

                packetSize = j;
            }

            byte[] slipData = null;

            if (packetSize > 0)
            {
                slipData = new byte[packetSize];
                Array.Copy(packet, 0, slipData, 0, packetSize);
            }

            return slipData;
        }

        private byte[] buildASCIIData(byte[] data)
        {
            byte[] outputData = null;

            if (data != null && data.Length > 0)
            {
                String dataString = MTParser.getHexString(data);
                byte[] asciiBytes = Encoding.UTF8.GetBytes(dataString);

                if (asciiBytes != null)
                {
                    int lenOutputData = asciiBytes.Length + 1;

                    outputData = new byte[lenOutputData];

                    Array.Copy(asciiBytes, 0, outputData, 0, asciiBytes.Length);

                    outputData[lenOutputData - 1] = 0x0D;
                }
            }

            return outputData;
        }

        public void processDeviceExtendedResponse(byte[] dataBytes)
        {
            if (dataBytes != null)
            {
                if (isUARTDataNotification(dataBytes))
                {
                    int offset = 5;
                    int newLen = dataBytes.Length - offset;

                    if (newLen > 0)
                    {
                        byte[] uartDataBytes = new byte[newLen];
                        Array.Copy(dataBytes, offset, uartDataBytes, 0, newLen);

                        if (uartDataBytes != null)
                        {
                            processUARTData(uartDataBytes);
                        }
                    }
                }
            }
        }

        protected bool isUARTDataNotification(byte[] dataBytes)
        {
            bool isUARTData = false;

            if ((dataBytes != null) && (dataBytes.Length >= 3))
            {
                //sendDebugInfo("UART NFC Data: " + MTParser.getHexString(dataBytes));

                if ((dataBytes[0] == 0x04) && (dataBytes[1] == 0x00))
                {
                    isUARTData = true;
                }
            }

            return isUARTData;
        }


        protected void processUARTData(byte[] dataBytes)
        {
            if (dataBytes != null)
            {
                //sendDebugInfo("UART=" + MTParser.getHexString(dataBytes));

                if (mSLIP)
                {
                    processSLIPData(dataBytes);
                }
                else
                {
                    processASCIIData(dataBytes);
                }
            }
        }

        private void resetSLIPDecoder()
        {
            mPacketStarted = false;
            mLastByteIsPacketStart = false;
            mLastByteIsEscape = false;

            mPacketSize = 0;
            mPacket = new byte[MAX_PACKET_SIZE];
        }


        private void processSLIPPacket(byte[] data)
        {
            if (data != null)
            {
                int msgLen = data.Length - 3;

                if (msgLen > 0)
                {
                    byte type = data[0];

                    byte[] msgBytes = new byte[msgLen];
                    Array.Copy(data, 3, msgBytes, 0, msgLen);

                    switch (type)
                    {
                        case 0x02: // Notification
                            processNotificationData(msgBytes);
                            break;
                        case 0x03: // Notification RLS
                            byte[] uncompressedBytes = MTRLEData.decodeRLEData(msgBytes);
                            processNotificationData(uncompressedBytes);
                            break;
                        case 0x04: // Response
                            if (OnDeviceResponse != null)
                            {
                                String msgString = MTParser.getHexString(msgBytes);
                                OnDeviceResponse(this, msgString);
                            }
                            break;
                    }
                }
            }
        }

        private void decodeSLIPByte(byte slipByte)
        {
            if (slipByte == SLIP_PACKET_START_END)
            {
                if (mPacketStarted)
                {
                    if (mLastByteIsPacketStart) // ANOTHER PACKET START 
                    {
                        mPacketStarted = true;
                    }
                    else // PACKET END 
                    {
                        mPacketStarted = false;

                        if (mPacketSize > 0)
                        {
                            byte[] data = new byte[mPacketSize];
                            Array.Copy(mPacket, 0, data, 0, mPacketSize);
                            processSLIPPacket(data);
                        }
                    }
                }
                else // PACKET START
                {
                    mPacketStarted = true;
                }

                mPacketSize = 0;
                mLastByteIsPacketStart = true;
            }
            else
            {
                if (mPacketStarted)
                {
                    if (mLastByteIsEscape)
                    {
                        if (slipByte == SLIP_ESCAPE_C0)
                        {
                            mPacket[mPacketSize] = (byte)0xC0;
                            mPacketSize++;
                        }
                        else if (slipByte == SLIP_ESCAPE_DB)
                        {
                            mPacket[mPacketSize] = (byte)0xDB;
                            mPacketSize++;
                        }

                        mLastByteIsEscape = false;
                    }
                    else
                    {
                        if (slipByte == SLIP_ESCAPE)
                        {
                            mLastByteIsEscape = true;
                        }
                        else
                        {
                            mPacket[mPacketSize] = slipByte;
                            mPacketSize++;

                            mLastByteIsEscape = false;
                        }
                    }
                }

                mLastByteIsPacketStart = false;
            }
        }

        private void processSLIPData(byte[] data)
        {
            if (data != null)
            {
                int len = data.Length;
                int i = 0;

                while (i < len)
                {
                    decodeSLIPByte(data[i]);
                    i++;
                }
            }
        }

        protected void processASCIIData(byte[] dataBytes)
        {
            if (dataBytes != null)
            {
                int newLen = dataBytes.Length;

                if (newLen > 0)
                {
                    byte[] bufferBytes = null;

                    if (m_uartDataReceived != null)
                    {
                        int oldLen = m_uartDataReceived.Length;
                        bufferBytes = new byte[oldLen + newLen];
                        Array.Copy(m_uartDataReceived, 0, bufferBytes, 0, oldLen);
                        Array.Copy(dataBytes, 0, bufferBytes, oldLen, newLen);
                    }
                    else
                    {
                        bufferBytes = new byte[newLen];
                        Array.Copy(dataBytes, 0, bufferBytes, 0, newLen);
                    }

                    int bufferLen = bufferBytes.Length;

                    if (bufferLen > 0)
                    {
                        int start = 0;
                        int i = 0;

                        while (i < bufferLen)
                        {
                            if (bufferBytes[i] == 0x0D)
                            {
                                int asciiLen = i - start;

                                if (asciiLen > 0)
                                {
                                    byte[] asciiBytes = new byte[asciiLen];
                                    Array.Copy(bufferBytes, start, asciiBytes, 0, asciiLen);

                                    String hexString = MTParser.getHexString(asciiBytes);

                                    sendDebugInfo("UART Data=" + hexString);

                                    String asciiString = System.Text.Encoding.UTF8.GetString(asciiBytes);

                                    sendDebugInfo("UART Data ASCII=" + asciiString);

                                    byte[] lineBytes = MTParser.getByteArrayFromHexString(asciiString);

                                    if (lineBytes != null)
                                    {
                                        int lineLen = lineBytes.Length;

                                        if (lineLen > 1)
                                        {
                                            byte type = lineBytes[0];
                                            byte[] lineDataBytes = new byte[lineLen - 1];
                                            Array.Copy(lineBytes, 1, lineDataBytes, 0, lineLen - 1);

                                            switch (type)
                                            {
                                                case 0x02: // Notification
                                                    processNotificationData(lineDataBytes);
                                                    break;
                                                case 0x04: // Response
                                                    String response = MTParser.getHexString(lineDataBytes);

                                                    if (OnDeviceResponse != null)
                                                    {
                                                        OnDeviceResponse(this, response);
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }

                                start = i + 1;
                            }

                            i++;
                        }

                        int remainingLen = bufferLen - start;

                        if (remainingLen > 0)
                        {
                            m_uartDataReceived = new byte[remainingLen];
                            Array.Copy(bufferBytes, start, m_uartDataReceived, 0, remainingLen);
                        }
                        else
                        {
                            m_uartDataReceived = null;
                        }
                    }
                }
            }
        }

        private void processNotificationData(byte[] data)
        {
            if (data != null)
            {
                if (data.Length >= 8)
                {
                    int partialLen = (data[0] & 0xFF);
                    partialLen <<= 8;
                    partialLen += (data[1] & 0xFF);

                    int offset = (data[2] & 0xFF);
                    offset <<= 8;
                    offset += (data[3] & 0xFF);

                    byte[] emvDataType = new byte[] { data[4], data[5] };

                    int totalLen = (data[6] & 0xFF);
                    totalLen <<= 8;
                    totalLen += (data[7] & 0xFF);

                    if (m_emvData == null)
                    {
                        m_emvDataLen = 0;
                        m_emvData = new byte[totalLen + 4];

                        m_emvData[0] = emvDataType[0];
                        m_emvData[1] = emvDataType[1];
                        m_emvData[2] = data[6];
                        m_emvData[3] = data[7];
                    }

                    if (data.Length >= (partialLen + 8))
                    {
                        Array.Copy(data, 8, m_emvData, offset + 4, partialLen);

                        m_emvDataLen += partialLen;
                    }

                    if (m_emvDataLen >= totalLen)
                    {
                        processEMVData(m_emvData);

                        m_emvDataLen = 0;
                        m_emvData = null;
                    }
                }
            }
        } 


        protected void processEMVData(byte[] data)
        {
            System.Diagnostics.Debug.WriteLine("UART EMV Data=" + MTParser.getHexString(data));

            if ((data != null) && (data.Length > 4))
            {
                byte[] emvData = null;

                int emvDataLen = data.Length - 4;

                if (emvDataLen > 0)
                {
                    emvData = new byte[emvDataLen];

                    Array.Copy(data, 4, emvData, 0, emvDataLen);
                }

                if (data[0] == 0x03)
                {
                    switch (data[1])
                    {
                        case 0x00:
                            if (OnTransactionStatus != null)
                            {
                                OnTransactionStatus(this, emvData);
                            }
                            break;
                        case 0x01:
                            if (OnDisplayMessageRequest != null)
                            {
                                OnDisplayMessageRequest(this, emvData);
                            }
                            break;
                        case 0x02:
                            if (OnUserSelectionRequest != null)
                            {
                                OnUserSelectionRequest(this, emvData);
                            }
                            break;
                        case 0x03:
                            if (OnARQCReceived != null)
                            {
                                OnARQCReceived(this, emvData);
                            }
                            break;
                        case 0x04:
                            if (OnTransactionResult != null)
                            {
                                OnTransactionResult(this, emvData);
                            }
                            break;
                    }
                }
            }
        }
    }
}
