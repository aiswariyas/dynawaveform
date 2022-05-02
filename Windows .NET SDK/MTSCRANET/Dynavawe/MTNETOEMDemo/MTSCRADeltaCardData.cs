using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MTNETOEMDemo
{
    public class MTSCRADeltaCardData : MTLIB.IMTCardData
    {
        private int m_threshold = 280;

        private byte[] m_rawData;

        private ReaderWriterLockSlim m_dataLock;

        public MTSCRADeltaCardData()
        {
            m_dataLock = new ReaderWriterLockSlim();

            clearData();
        }

        ~MTSCRADeltaCardData()
        {

        }

        public void clearData()
        {
            m_dataLock.EnterReadLock();

            m_rawData = null;

            m_dataLock.ExitReadLock();
        }

        public void setDataThreshold(int nBytes)
        {
            m_threshold = nBytes;
        }

        public bool isDataReady()
        {
            m_dataLock.EnterReadLock();

            if (m_rawData != null)
            {
                if (m_rawData.Length >= m_threshold)
                {
                    m_dataLock.ExitReadLock();

                    return true;
                }
            }

            m_dataLock.ExitReadLock();

            return false;
        }

        public void setData(byte[] data)
        {
            m_dataLock.EnterReadLock();
            byte[] existingData = m_rawData;
            m_dataLock.ExitReadLock();

            try
            {
                m_dataLock.EnterWriteLock();

                if (m_rawData == null)
                {
                    m_rawData = data;
                }
                else
                {
                    int len = m_rawData.Length + data.Length;
                    byte[] completeData = new byte[len];

                    Array.Copy(m_rawData, 0, completeData, 0, m_rawData.Length);
                    Array.Copy(data, 0, completeData, m_rawData.Length, data.Length);

                    m_rawData = completeData;
                }
            }
            catch (Exception ex)
            {
            }

            m_dataLock.ExitWriteLock();
        }

        public byte[] getData()
        {
            return m_rawData;
        }

        public void clearBuffers()
        {
            clearData();
        }

        protected byte[] getData(int offsetLength, int offsetStart)
        {
            byte[] resultArray = null;

            try
            {
                if (m_rawData != null)
                {
                    int lenData = 0;

                    if (m_rawData.Length >= offsetLength)
                    {
                        lenData = m_rawData[offsetLength];
                    }

                    if (lenData > 0)
                    {
                        resultArray = new byte[lenData];

                        Array.Copy(m_rawData, offsetStart, resultArray, 0, lenData);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return resultArray;
        }

        protected String getDataAsHexString(int offsetLength, int offsetStart)
        {
            string result = "";

            try
            {
                byte[] resultArray = getData(offsetLength, offsetStart);

                if ((resultArray != null) && (resultArray.Length > 0))
                {
                    result = MTParser.getHexString(resultArray);
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        protected String getDataAsString(int offsetLength, int offsetStart)
        {
            string result = "";

            try
            {
                byte[] resultArray = getData(offsetLength, offsetStart);

                if ((resultArray != null) && (resultArray.Length > 0))
                {
                    result = System.Text.Encoding.UTF8.GetString(resultArray, 0, resultArray.Length);
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        protected byte[] getDataWithLength(int offsetStart, int lenData)
        {
            byte[] resultArray = null;

            try
            {
                if (m_rawData != null)
                {
                    if (lenData > 0)
                    {
                        resultArray = new byte[lenData];

                        Array.Copy(m_rawData, offsetStart, resultArray, 0, lenData);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return resultArray;
        }

        protected String getDataWithLengthAsHexString(int offsetStart, int lenData)
        {
            string result = "";

            try
            {
                byte[] resultArray = getDataWithLength(offsetStart, lenData);

                if ((resultArray != null) && (resultArray.Length > 0))
                {
                    result = MTParser.getHexString(resultArray);
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        protected String getDataWithLengthAsString(int offsetStart, int lenData)
        {
            string result = "";

            try
            {
                byte[] resultArray = getDataWithLength(offsetStart, lenData);

                if ((resultArray != null) && (resultArray.Length > 0))
                {
                    if (resultArray[0] != 0)
                    {
                        result = System.Text.Encoding.UTF8.GetString(resultArray, 0, resultArray.Length);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public string getMaskedTracks()
        {
            return getTrack1Masked() + getTrack2Masked() + getTrack3Masked();
        }

        public string getTrack1()
        {
            return "";
        }

        public string getTrack2()
        {
            return "";
        }

        public string getTrack3()
        {
            return "";
        }

        public string getTrack1Masked()
        {
            return getDataWithLengthAsHexString(16, 88);
        }

        public string getTrack2Masked()
        {
            return getDataWithLengthAsHexString(104, 88);
        }

        public string getTrack3Masked()
        {
            return getDataWithLengthAsHexString(192, 88);
        }

        public string getMagnePrint()
        {
            return "";
        }

        public string getMagnePrintStatus()
        {
            return "";
        }

        public string getDeviceSerial()
        {
            return "";
        }

        public string getSessionID()
        {
            return "";
        }

        public string getKSN()
        {
            return getDataWithLengthAsHexString(0, 8);
        }

        public string getDeviceName()
        {
            return "";
        }

        public long getBatteryLevel()
        {
            return -1;
        }

        public long getSwipeCount()
        {
            return 0;
        }

        public string getCapMagnePrint()
        {
            return "";
        }

        public string getCapMagnePrintEncryption()
        {
            return "";
        }

        public string getCapMagneSafe20Encryption()
        {
            return "";
        }

        public string getCapMagStripeEncryption()
        {
            return "";
        }

        public string getCapMSR()
        {
            return "";
        }

        public string getCapTracks()
        {
            return "";
        }

        public long getCardDataCRC()
        {
            return 0;
        }

        public string getCardExpDate()
        {
            return "";
        }

        public string getCardIIN()
        {
            return "";
        }

        public string getCardLast4()
        {
            return "";
        }

        public string getCardName()
        {
            return "";
        }

        public string getCardPAN()
        {
            return "";
        }

        public int getCardPANLength()
        {
            int result = 0;

            return result;
        }

        public string getCardServiceCode()
        {
            string result = "";

            return result;
        }

        public string getCardStatus()
        {
            return "";
        }

        public string getCardEncodeType()
        {
            return "";
        }

        public int getDataFieldCount()
        {
            return 0;
        }

        public string getHashCode()
        {
            return "";
        }

        public string getDeviceConfig(string configType)
        {
            return "";
        }

        public string getEncryptionStatus()
        {
            return "";
        }

        public string getFirmware()
        {
            return "";
        }

        public string getMagTekDeviceSerial()
        {
            return "";
        }

        public string getResponseType()
        {
            return "";
        }

        public string getTagValue(string tag, string data)
        {
            return "";
        }

        public string getTLVVersion()
        {
            return "";
        }

        public string getTrackDecodeStatus()
        {
            return "";
        }

        public string getTLVPayload()
        {
            return "";
        }
    }
}
