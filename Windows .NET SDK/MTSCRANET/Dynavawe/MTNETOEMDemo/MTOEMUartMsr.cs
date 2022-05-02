using System;
using System.Text;

using MTSCRANET;

namespace MTNETOEMDemo
{
    class MTOEMUartMsr
    {
        private MTSCRA m_SCRA;

        private byte[] m_uartDataReceived;

        public delegate void DataReceivedHandler(object sender, string cardData);
        public delegate void DebugInfoHandler(object sender, string data);

        public event DataReceivedHandler OnDataReceived;
        public event DebugInfoHandler OnDebugInfo;

        public MTOEMUartMsr(MTSCRA scra)
        {
            m_SCRA = scra;

            m_uartDataReceived = null;
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

        public int sendData(String dataString)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;

            if (m_SCRA != null)
            {
                byte[] asciiBytes = Encoding.UTF8.GetBytes(dataString);

                int lenUartData = asciiBytes.Length + 2;
                byte[] uartData = new byte[lenUartData];
                uartData[0] = 0;
                Array.Copy(asciiBytes, 0, uartData, 1, asciiBytes.Length);
                uartData[lenUartData - 1] = 0x0D;

                byte[] UART_COMMAND = new byte[] { 0x04, 0x00 };
                byte[] uartCommand = buildExtendedCommand(UART_COMMAND, uartData);

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

        public void processDeviceExtendedResponse(byte[] dataBytes)
        {
            if (dataBytes != null)
            {
                if (isUARTDataNotification(dataBytes))
                {
                    processUARTData(dataBytes);
                }
            }
        }

        protected bool isUARTDataNotification(byte[] dataBytes)
        {
            bool isUARTData = false;

            if ((dataBytes != null) && (dataBytes.Length >= 3))
            {
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

                if (dataBytes.Length >= 5)
                {
                    const int offset = 5;

                    int newLen = dataBytes.Length - offset;

                    if (newLen > 0)
                    {
                        byte[] bufferBytes = null;

                        if (m_uartDataReceived != null)
                        {
                            int oldLen = m_uartDataReceived.Length;
                            bufferBytes = new byte[oldLen + newLen];
                            Array.Copy(m_uartDataReceived, 0, bufferBytes, 0, oldLen);
                            Array.Copy(dataBytes, offset, bufferBytes, oldLen, newLen);
                        }
                        else
                        {
                            bufferBytes = new byte[newLen];
                            Array.Copy(dataBytes, offset, bufferBytes, 0, newLen);
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

                                        if (OnDataReceived != null)
                                        {
                                            OnDataReceived(this, asciiString);
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
        }

    }
}
