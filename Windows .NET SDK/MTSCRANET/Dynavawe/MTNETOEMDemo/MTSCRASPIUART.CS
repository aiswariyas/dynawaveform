using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTSCRANET;
using MTLIB;

namespace MTNETOEMDemo
{
    class MTSCRASPIUART
    {
        private bool m_spiDataRequestPending = false;
        private bool m_spiStatusRequestPending = false;
        private byte[] m_spiDataReceived;
        private byte[] m_spiHeadData;
        private int m_spiHeadDataLength;
        private MTSCRANET.MTSCRA m_MTSCRA;
//        private MTSCRAHelper m_MTSCRAHelper;

        public MTSCRASPIUART(MTSCRA MTSCRA)
        {
            m_MTSCRA = MTSCRA;
        }

        private byte[] m_uartDataReceived;

        protected void processSPIStatus(string data)
        {
            bool spiDAV = false;

            byte[] dataBytes = MTParser.getByteArrayFromHexString(data);

            if (data.Length >= 5)
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

        protected void processSPIData(string data)
        {
            m_spiDataRequestPending = false;

            if (data.Length > 0)
            {
                byte[] dataBytes = MTParser.getByteArrayFromHexString(data);

                if (dataBytes != null)
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

        public void resetStates()
        {
            m_spiStatusRequestPending = false;
            m_spiDataRequestPending = false;
            m_spiDataReceived = null;
            m_spiHeadData = null;
            m_spiHeadDataLength = 0;
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
                            MTSCRAASCCardData ascCardData = new MTSCRAASCCardData();
                            byte[] cardData = new byte[dataLen - 1];
                            Array.Copy(m_spiHeadData, 1, cardData, 0, dataLen - 1);
                            ascCardData.setData(cardData);
                            String message = System.Text.Encoding.UTF8.GetString(cardData);
                            m_MTSCRAHelper.OnDataReceivedSPI(ascCardData, message);
                        }
                    }

                    m_spiHeadData = null;
                    m_spiHeadDataLength = 0;
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
                                    requestSPIData(5);
                                }
                            }
                            else
                            {
                                requestSPIData(5);
                            }
                        }
                    }
                }
            }
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
            if (m_MTSCRA != null)
            {
                if (len > 0)
                {
                    string commandString = "0500";
                    string sizeString = getTwoBytesLengthString(len + 1);
                    string dataString = "00" + new string('F', (len * 2));

                    m_spiDataRequestPending = true;

                    String sendString = commandString + sizeString + dataString;

                    //sendToDisplay("[Request SPI Data] Length=" + len + ", data=" + sendString);

                    int result = m_MTSCRA.sendExtendedCommand(sendString);

                    if (result != 0)
                    {
                        //sendToDisplay("[Request SPI Data] *** Result=" + result);
                    }
                }
            }
        }

        protected String getTwoBytesLengthString(int length)
        {
            byte[] lengthBytes = new byte[2];

            lengthBytes[1] = (byte)((length % 256) & 0xFF);
            lengthBytes[0] = (byte)(((length >> 8) % 256) & 0xFF);

            return MTParser.getHexString(lengthBytes);
        }
        protected bool isSPIDataNofitication(string data)
        {
            bool spiDAV = false;

            byte[] dataBytes = MTParser.getByteArrayFromHexString(data);

            if ((dataBytes != null) && (data.Length >= 6))
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

        protected bool isUARTDataNofitication(string data)
        {
            bool isUARTData = false;

            byte[] dataBytes = MTParser.getByteArrayFromHexString(data);

            if ((dataBytes != null) && (data.Length >= 3))
            {
                if ((dataBytes[0] == 0x04) && (dataBytes[1] == 0x00))
                {
                    isUARTData = true;
                }
            }

            return isUARTData;
        }

        protected void processUARTData(string data)
        {
            if (data.Length > 0)
            {
                byte[] dataBytes = MTParser.getByteArrayFromHexString(data);

                if (dataBytes != null)
                {
                    int newLen = dataBytes.Length - 3;

                    if (newLen > 0)
                    {
                        byte[] allBytes = null;

                        if (m_uartDataReceived != null)
                        {
                            int oldLen = m_uartDataReceived.Length;
                            allBytes = new byte[oldLen + newLen];
                            Array.Copy(m_spiDataReceived, 0, allBytes, 0, oldLen);
                            Array.Copy(dataBytes, 3, allBytes, oldLen, newLen);
                        }
                        else
                        {
                            allBytes = new byte[newLen];
                            Array.Copy(dataBytes, 3, allBytes, 0, newLen);
                        }
                        m_uartDataReceived = allBytes;

                        String message = MTParser.getHexString(allBytes);

                        if (message.Length > 4)
                        {
                            m_MTSCRAHelper.OnDataReceivedUART(message.Substring(4));
                        }
                        else
                        {
                            m_MTSCRAHelper.OnDataReceivedUART(message);

                        }
                        m_uartDataReceived = null;
                    }
                }
            }
        }


        public void processDeviceExtendedResponse(string data)
        {
            if (data.Length > 0)
            {
                if (m_spiStatusRequestPending)
                {
                    processSPIStatus(data);
                }
                else if (m_spiDataRequestPending)
                {
                    processSPIData(data);
                }
                else if (isSPIDataNofitication(data))
                {
                    requestSPIData(5);
                }
                else if (isUARTDataNofitication(data))
                {
                    processUARTData(data);
                }
            }
        }
    }//class MTSCRASPIUART
}
