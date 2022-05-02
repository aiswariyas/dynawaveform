using System;
using System.Threading.Tasks;

using MTSCRANET;
using MTLIB;

namespace MTNETOEMDemo
{
    class MTOEMSpiMsr
    {
        private MTSCRA m_SCRA;

        private bool m_spiStatusRequestPending = false;
        private bool m_spiDataRequestPending = false;

        private byte[] m_spiDataReceived;
        private byte[] m_spiHeadData;

        private int m_spiHeadDataLength;

        public delegate void DataReceivedHandler(object sender, IMTCardData cardData);
        public delegate void ResponseReceivedHandler(object sender, string response);
        public delegate void DebugInfoHandler(object sender, string data);

        public event DataReceivedHandler OnDataReceived;
        public event ResponseReceivedHandler OnResponseReceived;
        public event DebugInfoHandler OnDebugInfo;

        public MTOEMSpiMsr(MTSCRA scra)
        {
            m_SCRA = scra;

            m_spiStatusRequestPending = false;
            m_spiDataRequestPending = false;
            m_spiDataReceived = null;
            m_spiHeadData = null;
            m_spiHeadDataLength = 0;

            requestSPIStatus();
        }

        protected void sendDebugInfo(string data)
        {
            if (OnDebugInfo != null)
            {
                OnDebugInfo(this, data);
            }
        }

        public void processDeviceExtendedResponse(byte[] dataBytes)
        {
            if (dataBytes != null)
            {
                if (m_spiStatusRequestPending)
                {
                    processSPIStatus(dataBytes);
                }
                else if (m_spiDataRequestPending)
                {
                    processSPIData(dataBytes);
                }
                else if (isSPIDataNotification(dataBytes))
                {
                    requestSPIData(5);
                }
            }
        }

        protected bool isSPIDataNotification(byte[] dataBytes)
        {
            bool spiDAV = false;

            if ((dataBytes != null) && (dataBytes.Length >= 6))
            {
                if ((dataBytes[0] == 0x05) && (dataBytes[1] == 0x00))
                {
                    byte statusByte = dataBytes[5];
                    if ((statusByte & (byte)0x02) != 0)
                    {
                        spiDAV = true;
                    }
                }
            }

            return spiDAV;
        }

        protected void requestSPIStatus()
        {
            Task task = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(100);

                if (m_SCRA != null)
                {
                    string commandString = "0501";
                    string sizeString = "0001";
                    string dataString = "00";

                    sendDebugInfo("[Request SPI Status]");

                    m_spiStatusRequestPending = true;

                    m_SCRA.sendExtendedCommand(commandString + sizeString + dataString);
                }
            });
        }

        protected void requestSPIData(int length)
        {
            Task task = Task.Factory.StartNew(async (param) =>
            {
                await Task.Delay(50);

                writeSPIData(length);

            }, length);
        }

        protected void writeSPIData(int len)
        {
            if (len > 0)
            {
                string dataString = new string('F', (len * 2));

                sendDebugInfo("[Request SPI Data] Length=" + len);

                m_spiDataRequestPending = true;

                int result = sendSPIData(dataString);

                if (result != 0)
                {
                    sendDebugInfo("[Request SPI Data] *** Result=" + result);
                }
            }
        }

        public int sendSPIData(String dataString)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;
            
            if ((m_SCRA != null) && (dataString != null) && (dataString.Length > 0))
            {
                string commandString = "0500";
                int len = dataString.Length / 2 + 1;
                string sizeString = getTwoBytesLengthString(len);

                String sendString = commandString + sizeString + "00" + dataString;

                sendDebugInfo("Send SPI Extended Command: " + sendString);

                result = m_SCRA.sendExtendedCommand(sendString);
            }

            return result;
        }

        public int sendData(String dataString)
        {
            int result = MTSCRA.SEND_COMMAND_ERROR;

            int len = dataString.Length / 2 + 1;
            string sizeString = getTwoBytesLengthString(len);

            String spiData = "01" + sizeString + "00" + dataString;

            result = sendSPIData(spiData);

            return result;
        }

        protected String getTwoBytesLengthString(int length)
        {
            byte[] lengthBytes = new byte[2];

            lengthBytes[1] = (byte)((length % 256) & 0xFF);
            lengthBytes[0] = (byte)(((length >> 8) % 256) & 0xFF);

            return MTParser.getHexString(lengthBytes);
        }

        protected void processSPIStatus(byte[] dataBytes)
        {
            bool spiDAV = false;

            if ((dataBytes != null) && (dataBytes.Length >= 5))
            {
                if (dataBytes[4] == 0x05)
                {
                    spiDAV = true;
                }
            }

            m_spiStatusRequestPending = false;

            if (spiDAV)
            {
                requestSPIData(5);
            }
        }

        protected void processSPIData(byte[] dataBytes)
        {
            m_spiDataRequestPending = false;

            if (dataBytes != null)
            {
                if (dataBytes.Length >= 2)
                {
                    if ((dataBytes[0] == 0) && (dataBytes[1] == 0)) // RC
                    {
                        int i = 4;
                        if (dataBytes.Length > i)
                        {
                            int newLen = dataBytes.Length - i;

                            byte[] allBytes = null;

                            if (m_spiDataReceived != null)
                            {
                                int oldLen = m_spiDataReceived.Length;
                                allBytes = new byte[oldLen + newLen];
                                Array.Copy(m_spiDataReceived, 0, allBytes, 0, oldLen);
                                Array.Copy(dataBytes, i, allBytes, oldLen, newLen);
                            }
                            else
                            {
                                allBytes = new byte[newLen];
                                Array.Copy(dataBytes, i, allBytes, 0, newLen);
                            }

                            m_spiDataReceived = allBytes;

                            processSPIDataReceived();
                        }
                    }
                }
            }
        }

        protected void processSPIDataReceived()
        {
            if (m_spiDataReceived != null)
            {
                if (m_spiHeadDataLength > 0)
                {
                    processSPIHeadData(m_spiDataReceived);
                    m_spiDataReceived = null;
                }
                else
                {
                    int i = 0;
                    for (; i < m_spiDataReceived.Length; i++)
                    {
                        if (m_spiDataReceived[i] != 0xFF) // IDLE
                        {
                            if (m_spiDataReceived[i] == 1) // SOF
                            {
                                i++;
                                if ((i + 1) < m_spiDataReceived.Length)
                                {
                                    byte lenHighByte = m_spiDataReceived[i++];
                                    byte lenLowByte = m_spiDataReceived[i++];
                                    m_spiHeadDataLength = (int)(lenHighByte << 8) + (int)lenLowByte;

                                    int trailingDataLen = m_spiDataReceived.Length - i;

                                    if (trailingDataLen > 0)
                                    {
                                        byte[] trailingData = new byte[trailingDataLen];
                                        Array.Copy(m_spiDataReceived, i, trailingData, 0, trailingDataLen);

                                        m_spiDataReceived = null;
                                        processSPIHeadData(trailingData);
                                    }
                                    else
                                    {
                                        m_spiDataReceived = null;
                                        processSPIHeadData(null);
                                    }
                                }
                                else
                                {
                                    sendDebugInfo("[processSPIDataReceived] Data Short");
                                    requestSPIData(5);
                                }
                            }
                            else
                            {
                                sendDebugInfo("[processSPIDataReceived] Not SOF");
                                //requestSPIData(5);
                            }
                        }
                    }
                }
            }
        }

        protected void processSPIHeadData(byte[] newData)
        {
            if (newData != null)
            {
                int newLen = newData.Length;

                if (m_spiHeadData != null)
                {
                    int oldLen = m_spiHeadData.Length;
                    byte[] allBytes = new byte[oldLen + newLen];
                    Array.Copy(m_spiHeadData, 0, allBytes, 0, oldLen);
                    Array.Copy(newData, 0, allBytes, oldLen, newLen);
                    m_spiHeadData = allBytes;
                }
                else
                {
                    m_spiHeadData = new byte[newLen];
                    Array.Copy(newData, 0, m_spiHeadData, 0, newLen);
                }
            }

            if (m_spiHeadDataLength > 0)
            {
                int bytesToRequest = m_spiHeadDataLength;

                if (m_spiHeadData != null)
                {
                    bytesToRequest -= m_spiHeadData.Length;
                }

                if (bytesToRequest > 0)
                {
                    requestSPIData(bytesToRequest);
                }
                else
                {
                    int dataLen = m_spiHeadData.Length;

                    if (dataLen > 1)
                    {
                        if (m_spiHeadData[0] == 2) // Notification
                        {
                            //String message = System.Text.Encoding.UTF8.GetString(m_spiHeadData);
                            //sendToDisplay(message);

                            MTSCRAASCCardData ascCardData = new MTSCRAASCCardData();
                            byte[] cardData = new byte[dataLen - 1];
                            Array.Copy(m_spiHeadData, 1, cardData, 0, dataLen - 1);

                            ascCardData.setData(cardData);

                            if (OnDataReceived != null)
                            {
                                OnDataReceived(this, ascCardData);
                            }

                            //sendToDisplay(getCardDataInfo(ascCardData));
                        }
                        else if (m_spiHeadData[0] == 1) // Response
                        {
                            byte[] responseBytes = new byte[dataLen - 1];
                            Array.Copy(m_spiHeadData, 1, responseBytes, 0, dataLen - 1);

                            if (OnResponseReceived != null)
                            {
                                String response = MTParser.getHexString(responseBytes);
                                OnResponseReceived(this, response);
                            }
                        }
                    }

                    m_spiHeadData = null;
                    m_spiHeadDataLength = 0;
                }
            }
        }
    }
}
