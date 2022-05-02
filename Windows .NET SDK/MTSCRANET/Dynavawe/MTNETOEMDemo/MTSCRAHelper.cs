using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;
using MTSCRANET;
using MTLIB;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.ServiceModel;

namespace MagneFlexV2
{
    public class MTSCRAHelper
    {
        private MTSCRANET.MTSCRA m_SCRA;
        private MTSCRASPIUART m_MTSCRASPIUART;
        private MagneFlexCommon m_MagneFlexCommon;
        private MTSCRACardData m_CardData;
        private MTSCRADeviceResponseData m_DeviceResponseData;
        private MTSCRADeviceResponseData m_DeviceExtendedResponseData;
        private MTSCRASmartCardResponseData m_DeviceSmartCardResponseData;
        private EventWaitHandle m_HandleSmartCardWaitComplete;
        private EventWaitHandle m_HandleConnectWaitComplete;
        private EventWaitHandle m_HandleCardDataWaitComplete;
        private EventWaitHandle m_HandleDeviceListWaitComplete;
        private EventWaitHandle m_HandleSendCommandWaitComplete;
        private EventWaitHandle m_HandleSendExtendedCommandWaitComplete;
        private List<MTLIB.MTDeviceInformation> m_DeviceList;
        private int THREAD_WAIT_TIME = 2;
        private int DEVICE_CONNECT_TIMEOUT = 5;
        private int MAX_RETRY_COUNT = 5;
        private int SENDEXTENDEDCOMMAND_WAITTIME = 5000;
        private string m_WaitTimeoutAfterOpen = ConfigurationManager.AppSettings["WaitTimeoutAfterOpen"];
        private frmMain m_frmMain;

        public const string ERROR_DEVICE_OPERATION_FAILED = "Device Operation Failed";
        public const string ERROR_DEVICE_OPERATION_TIMEOUT = "Device Operation Timeout";
        public const string ERROR_DEVICE_OPEN_FAILED = "Device Open Failed";
        public const string ERROR_GENERIC = "Unknown Error";
        public const string ERROR_SUCCESS = "OK";
        public const string ERROR_INVALID_AMOUNT = "Invalid Amount";
        public const string ERROR_BUILD_ARPC = "Error Building ARPC Data";
        public const string ERROR_BUILD_ARQC = "Error Building ARQC Data";
        public const string ERROR_BUILD_BATCH = "Error Building BATCH Data";
        public const string ERROR_INVALID_CASHBACK = "Invalid Cashback";
        public const string ERROR_READING_CONFIG = "Error Reading Config File";
        public const string ERROR_INVALID_REQUEST_TYPE = "Invalid Request Type";
        public const string ERROR_NO_DATA = "No Data To Process";
        frmSelection SelectionForm = null;

        public class MTSCRADeviceResponseData
        {
            public string CommandID { get; set; }
            public string Data { get; set; }

        }
        public class MTSCRASmartCardResponseData
        {
            public string ARQCData { get; set; }
            public string BATCHData { get; set; }
            public string UserSelection { get; set; }
            public string CommandResult { get; set; }
        }
        public class MTSCRASendAcquirerResponseData
        {
            public string BATCHData { get; set; }
            public string UserSelection { get; set; }
            public string CommandResult { get; set; }
        }
        public class MTSCRACardData
        {
            public long BatteryLevel { get; set; }
            public string CapMagnePrint { get; set; }
            public string CapMagnePrintEncryption { get; set; }
            public string CapMagnePrint20Encryption { get; set; }
            public string CapMagneStripreEncryption { get; set; }
            public string CapMSR { get; set; }
            public string CapTracks { get; set; }
            public long CardDataCRC { get; set; }
            public string CardEncodeType { get; set; }
            public string CardExpDate { get; set; }
            public string CardIIN { get; set; }
            public string CardLast4 { get; set; }
            public string CardName { get; set; }
            public int CardPANLength { get; set; }
            public string CardServiceCode { get; set; }
            public string ResponseData { get; set; }
            public int DataFieldCount { get; set; }
            public string DeviceConfig { get; set; }
            public string DeviceName { get; set; }
            public string DeviceSerial { get; set; }
            public string EncryptionStatus { get; set; }
            public string Firmware { get; set; }
            public string HashCode { get; set; }
            public string KSN { get; set; }
            public string MagnePrint { get; set; }
            public string MagnePrintStatus { get; set; }
            public string MagTekDeviceSerial { get; set; }
            public string MaskedTracks { get; set; }
            public string ResponseType { get; set; }
            public long SwipeCount { get; set; }
            public string TLVVersion { get; set; }
            public string Track1 { get; set; }
            public string Track1Masked { get; set; }
            public string Track2 { get; set; }
            public string Track2Masked { get; set; }
            public string Track3 { get; set; }
            public string Track3Masked { get; set; }
            public string TrackDecodeStatus { get; set; }
            public MTSCRADeviceResponseData DeviceResponseData;
        }
        public class MTPPSCRAFaultException : FaultException
        {
            public string TransactionUTCTimestamp { get; set; }
            public KeyValuePair<string, string>[] AdditionalData { get; set; }
            internal MTPPSCRAFaultException(string FaultCode, string FaultReason, KeyValuePair<string, string>[] AdditionalData)
                : base(new FaultReason(FaultReason), new FaultCode(FaultCode))
            {
                this.AdditionalData = AdditionalData;
                this.TransactionUTCTimestamp = DateTime.UtcNow.ToString("u");
            }
        }

        public enum MTSCRAFaultCode : int
        {
            SUCCESS = 100,
            GENERIC,
            DEVICE_OPERATION_FAILED,
            DEVICE_OPERATION_TIMEOUT,
            DEVICE_OPEN_FAILED,
            INVALID_AMOUNT,
            INVALID_CASHBACK,
            MPPG_CALL_FAILED
        }


        public class MTSCRAFaultException : FaultException
        {
            public string TransactionUTCTimestamp { get; set; }
            public KeyValuePair<string, string>[] AdditionalData { get; set; }
            internal MTSCRAFaultException(string FaultCode, string FaultReason, KeyValuePair<string, string>[] AdditionalData)
                : base(new FaultReason(FaultReason), new FaultCode(FaultCode))
            {
                this.AdditionalData = AdditionalData;
                this.TransactionUTCTimestamp = DateTime.UtcNow.ToString("u");
            }
        }

        public class RequestUserSelectionRequest
        {
            public string DeviceID { get; set; }
            public MTLIB.MTConnectionType ConnectionType { get; set; }
            public int WaitTime { get; set; }
            public int Status { get; set; }
            public int Selection { get; set; }
            public KeyValuePair<string, string>[] AdditionalRequestData { get; set; }
        }
        public class RequestUserSelectionResponse
        {
            public MTSCRASmartCardResponseData ResponseOutput { get; set; }
            public KeyValuePair<string, string>[] AdditionalOutputData { get; set; }
        }


        public class RequestDeviceListRequest
        {
            public MTLIB.MTConnectionType ConnectionType { get; set; }
            public int WaitTime { get; set; }
            public KeyValuePair<string, string>[] AdditionalRequestData { get; set; }
        }
        public class RequestDeviceListResponse
        {
            public KeyValuePair<string, string>[] DeviceList { get; set; }
            public KeyValuePair<string, string>[] AdditionalOutputData { get; set; }
        }
        public class RequestSendCommandRequest
        {
            public string DeviceID { get; set; }
            public MTLIB.MTConnectionType ConnectionType { get; set; }
            public int WaitTime { get; set; }
            public string Command { get; set; }
            public KeyValuePair<string, string>[] AdditionalRequestData { get; set; }
        }

        public class RequestSendCommandResponse
        {
            public MTSCRADeviceResponseData ResponseOutput { get; set; }
            public KeyValuePair<string, string>[] AdditionalOutputData { get; set; }
        }
        public class RequestSendAcquirerResponseRequest
        {
            public string DeviceID { get; set; }
            public MTLIB.MTConnectionType ConnectionType { get; set; }
            public int WaitTime { get; set; }
            public string IssuerAuthenticationData { get; set; }
            public string IssuerScriptTemplate1 { get; set; }
            public string IssuerScriptTemplate2 { get; set; }
            public string DeviceSerialNumber { get; set; }
            public Byte ApprovalStatus { get; set; }
            public KeyValuePair<string, string>[] AdditionalRequestData { get; set; }
        }
        public class RequestSendAcquirerResponseResponse
        {
            public MTSCRASendAcquirerResponseData ResponseOutput { get; set; }
            public KeyValuePair<string, string>[] AdditionalOutputData { get; set; }
        }

        public class RequestSendExtendedCommandRequest : RequestSendCommandRequest
        {
        }
        public class RequestSendExtendedCommandResponse : RequestSendCommandResponse
        {
        }

        public class RequestCardSwipeRequest
        {
            public string DeviceID { get; set; }
            public MTLIB.MTConnectionType ConnectionType { get; set; }
            public int WaitTime { get; set; }
            public string FieldSeparator { get; set; }
            public KeyValuePair<string, string>[] AdditionalRequestData { get; set; }
        }

        public class RequestCardSwipeResponse
        {
            public MTSCRACardData CardSwipeOutput { get; set; }
            public KeyValuePair<string, string>[] AdditionalOutputData { get; set; }
        }
        public class MTSCRAException
        {
            public string TransactionUTCTimestamp { get; set; }
            public KeyValuePair<string, string>[] AdditionalData { get; set; }
        }
        public class RequestSmartCardRequest
        {
            public string DeviceID { get; set; }
            public MTLIB.MTConnectionType ConnectionType { get; set; }
            public byte TransactionType { get; set; }
            public int WaitTime { get; set; }
            public byte CardType { get; set; }
            public decimal? Amount { get; set; }
            public decimal? CashBack { get; set; }
            public string CurrencyCode { get; set; }
            public byte ReportOptions { get; set; }
            public byte Options { get; set; }
            public bool QwickChipMode { get; set; }
            public KeyValuePair<string, string>[] AdditionalRequestData { get; set; }
        }
        public class RequestSmartCardResponse
        {
            public MTSCRASmartCardResponseData ResponseOutput { get; set; }
            public KeyValuePair<string, string>[] AdditionalOutputData { get; set; }
        }

        private void Initialize()
        {
            m_SCRA = new MTSCRANET.MTSCRA();
            m_MagneFlexCommon = new MagneFlexCommon();
            m_CardData = new MTSCRACardData();
            m_DeviceResponseData = new MTSCRADeviceResponseData();
            m_DeviceExtendedResponseData = new MTSCRADeviceResponseData();
            m_DeviceSmartCardResponseData = new MTSCRASmartCardResponseData();
            m_DeviceList = new List<MTDeviceInformation>();
            m_MTSCRASPIUART = new MTSCRASPIUART(m_SCRA, this);
            m_HandleSmartCardWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
            m_HandleConnectWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
            m_HandleCardDataWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
            m_HandleDeviceListWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
            m_HandleSendCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
            m_HandleSendExtendedCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
            m_SCRA.OnDeviceList += OnDeviceList;
            m_SCRA.OnDeviceConnectionStateChanged += OnDeviceConnectionStateChanged;
            m_SCRA.OnCardDataState += OnCardDataStateChanged;
            m_SCRA.OnDataReceived += OnDataReceived;
            m_SCRA.OnDeviceResponse += OnDeviceResponse;
            m_SCRA.OnDeviceExtendedResponse += OnDeviceExtendedResponse;
            m_SCRA.OnTransactionStatus += OnTransactionStatus;
            m_SCRA.OnDisplayMessageRequest += OnDisplayMessageRequest;
            m_SCRA.OnUserSelectionRequest += OnUserSelectionRequest;
            m_SCRA.OnARQCReceived += OnARQCReceived;
            m_SCRA.OnEMVCommandResult += OnEMVCommandResult;
            m_SCRA.OnTransactionResult += OnTransactionResult;
        }

        public MTSCRAHelper(frmMain Form)
        {
            m_frmMain = Form;
            Initialize();
        }

        public MTSCRAHelper()
        {
            Initialize();
        }

        public RequestDeviceListResponse RequestDeviceList(RequestDeviceListRequest RequestDeviceListRequest)
        {
            m_MagneFlexCommon.debugMessage(" + MTSCRAHelper:RequestDeviceList");
            RequestDeviceListResponse tResponse = new RequestDeviceListResponse();
            List<KeyValuePair<string, string>> tDeviceList = new List<KeyValuePair<string, string>>();

            try
            {
                if (RequestDeviceListRequest != null)
                {
                    clearAllData();
                    m_HandleDeviceListWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    m_MagneFlexCommon.debugMessage(" : MTSCRAHelper:RequestDeviceList:+ requestDeviceList");
                    m_SCRA.requestDeviceList(RequestDeviceListRequest.ConnectionType);
                    m_MagneFlexCommon.debugMessage(" : MTSCRAHelper:RequestDeviceList:- requestDeviceList");
                    m_MagneFlexCommon.debugMessage(" : MTSCRAHelper:RequestDeviceList:WaitTime:" + RequestDeviceListRequest.WaitTime + "");
                    if (m_HandleDeviceListWaitComplete.WaitOne((RequestDeviceListRequest.WaitTime + THREAD_WAIT_TIME) * 1000))
                    {
                        m_MagneFlexCommon.debugMessage(" : MTSCRAHelper:RequestDeviceList: m_HandleSendCommandWaitComplete Done");
                        if (m_DeviceList != null)
                        {
                            if (m_DeviceList.Count > 0)
                            {
                                foreach (var device in m_DeviceList)
                                {
                                    tDeviceList.Add(new KeyValuePair<string, string>(device.Name, device.Address));
                                }
                                tResponse.DeviceList = tDeviceList.ToArray();
                            }
                        }

                        clearDeviceList();
                    }
                    else
                    {
                        clearDeviceList();
                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_TIMEOUT), ERROR_DEVICE_OPERATION_TIMEOUT, null);
                    }
                }
                else
                {
                    MTSCRAException tException = new MTSCRAException();
                    throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), "Invalid Parameter", null);
                }
            }
            catch (MTSCRAFaultException ex)
            {
                m_MagneFlexCommon.debugMessage(" - MTSCRAHelper:RequestDeviceList" + ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                MTSCRAException tException = new MTSCRAException();
                m_MagneFlexCommon.debugMessage(" - MTSCRAHelper:RequestDeviceList" + ex.Message);
                throw new MTSCRAFaultException(Convert.ToString(MTSCRAFaultCode.GENERIC), ex.Message, null);
            }
            m_MagneFlexCommon.debugMessage(" - MTSCRAHelper:RequestDeviceList");
            return tResponse;

        }
        public RequestSendExtendedCommandResponse RequestSendExtendedCommand(MagneFlexCommon.DeviceInformation DeviceInformation, bool CloseDevice)
        {
            m_MagneFlexCommon.debugMessage("+ MTSCRAHelper:RequestSendExtendedCommand");
            RequestSendExtendedCommandResponse tResponse = new RequestSendExtendedCommandResponse();
            tResponse.ResponseOutput = new MTSCRADeviceResponseData();
            m_CardData.DeviceResponseData = m_DeviceResponseData;
            try
            {
                openDevice(DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.ConnectionType, DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.DeviceID);
                if (m_SCRA.isDeviceConnected())
                {
                    clearAllData();
                    tResponse.ResponseOutput.CommandID = DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.Command;
                    m_HandleSendCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    m_HandleSendExtendedCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSendExtendedCommand:Command:" + DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.Command);
                    int tStatus = 0;
                    int iRetryCount = 0;
                    for (;;)
                    {
                        tStatus = m_SCRA.sendExtendedCommand(DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.Command);
                        m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSendExtendedCommand:Command:" + DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.Command + ":" + tStatus);
                        if (tStatus == 0) break;
                        Thread.Sleep(100);
                        iRetryCount++;
                        if (iRetryCount > MAX_RETRY_COUNT) break;
                    }
                    if (m_HandleSendExtendedCommandWaitComplete.WaitOne((DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.WaitTime + THREAD_WAIT_TIME) * 1000))
                    {
                        tResponse.ResponseOutput = m_DeviceExtendedResponseData;
                        if(DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.Command.Length >=4)
                        {
                            tResponse.ResponseOutput.CommandID = DeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.Command.Substring(0, 4);
                        }
                        //clearAllData();
                        if (DeviceInformation.CloseDeviceAfter)
                        {
                            if (CloseDevice)
                            {
                                closeDevice();
                            }
                        }
                    }
                    else
                    {
                        clearAllData();
                        closeDevice();
                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_TIMEOUT), ERROR_DEVICE_OPERATION_TIMEOUT, null);
                    }
                }//if (m_SCRA.isDeviceConnected())
            }
            catch (MTSCRAFaultException ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendExtendedCommand");
                clearAllData();
                closeDevice();
                throw ex;
            }
            catch (Exception ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendExtendedCommand");
                clearAllData();
                closeDevice();
                MTSCRAException tException = new MTSCRAException();
                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), ex.Message, null);
            }
            m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendExtendedCommand");
            return tResponse;


        }
        ////public RequestUserSelectionResponse RequestUserSelection(RequestUserSelectionRequest lpRequest)
        ////{
        ////    RequestUserSelectionResponse tResponse = new RequestUserSelectionResponse();
        ////    tResponse.ResponseOutput = new MTSCRASmartCardResponseData();
        ////    try
        ////    {
        ////        openDevice(lpRequest.ConnectionType, lpRequest.DeviceID);
        ////        if (m_SCRA.isDeviceConnected())
        ////        {
        ////            clearAllData();
        ////            m_HandleSmartCardWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
        ////            m_HandleSendCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
        ////            int tStatus = 0;
        ////            int iRetryCount = 0;
        ////            for (;;)
        ////            {
        ////                tStatus = m_SCRA.setUserSelectionResult((byte)lpRequest.Status, (byte)lpRequest.Selection);
        ////                m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSendExtendedCommand:Status:" + lpRequest.Status + ",Selection:" + lpRequest.Selection + ":" + tStatus + " ");
        ////                if (tStatus == 0) break;
        ////                Thread.Sleep(100);
        ////                iRetryCount++;
        ////                if (iRetryCount > MAX_RETRY_COUNT) break;
        ////            }
        ////            if (m_HandleSmartCardWaitComplete.WaitOne((lpRequest.WaitTime + THREAD_WAIT_TIME) * 1000))
        ////            {
        ////                tResponse.ResponseOutput = m_DeviceSmartCardResponseData;
        ////                clearSDKData();
        ////                if (m_DeviceSmartCardResponseData.BATCHData != null)
        ////                {
        ////                    if (m_DeviceSmartCardResponseData.BATCHData.Length > 0)
        ////                    {
        ////                        closeDevice();
        ////                    }
        ////                }
        ////            }
        ////            else
        ////            {
        ////                clearSDKData();
        ////                closeDevice();
        ////                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_TIMEOUT), ERROR_DEVICE_OPERATION_TIMEOUT, null);
        ////            }
        ////        }//if (m_SCRA.isDeviceConnected())
        ////    }
        ////    catch (MTSCRAFaultException ex)
        ////    {
        ////        m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestCardSwipe");
        ////        clearAllData();
        ////        closeDevice();
        ////        throw ex;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestCardSwipe");
        ////        clearAllData();
        ////        closeDevice();
        ////        MTSCRAException tException = new MTSCRAException();
        ////        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), ex.Message, null);
        ////    }
        ////    m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestCardSwipe");
        ////    return tResponse;

        ////}
        public string GetDeviceSerial()
        {
            m_MagneFlexCommon.debugMessage("+ MTSCRAHelper:GetDeviceSerial");
            string tResult = string.Empty;
            tResult = m_SCRA.getDeviceSerial();
            if (tResult.Length > 0)
            {
                tResult = m_MagneFlexCommon.byteToHex(Encoding.ASCII.GetBytes(tResult));
                m_MagneFlexCommon.debugMessage(": MTSCRAHelper:GetDeviceSerial:" + tResult);
                while (tResult.Length < 32)
                {
                    tResult += "0";
                }
            }
            m_MagneFlexCommon.debugMessage("- MTSCRAHelper:GetDeviceSerial:" + tResult);
            return tResult;
        }

        public RequestSmartCardResponse RequestSmartCard(MagneFlexCommon.DeviceInformation DeviceInformation)
        {
            m_MagneFlexCommon.debugMessage("+ MTSCRAHelper:RequestSmartCard");
            RequestSmartCardResponse tResponse = new RequestSmartCardResponse();
            tResponse.ResponseOutput = new MTSCRASmartCardResponseData();
            try
            {
                openDevice(DeviceInformation.SCRAMSR.RequestSmartCardRequest.ConnectionType, DeviceInformation.SCRAMSR.RequestSmartCardRequest.DeviceID);
                if (m_SCRA.isDeviceConnected())
                {
                    m_frmMain.setStatus("Please Insert a Card....");
                    clearAllData();
                    if (m_SCRA.isDeviceOEM())
                    {
                        DateTime now = DateTime.Now;
                        int month = now.Month;
                        int day = now.Day;
                        int hour = now.Hour;
                        int minute = now.Minute;
                        int second = now.Second;
                        int year = now.Year - 2008;
                        string dateTimeString = String.Format("{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}00{5:X2}", month, day, hour, minute, second, year);
                        string tCommand = "030C001800" + GetDeviceSerial() + dateTimeString + "00000000";
                        RequestSendExtendedCommandResponse tSendCommandResponse = new RequestSendExtendedCommandResponse();
                        MagneFlexCommon.DeviceInformation tDeviceInformation = new MagneFlexCommon.DeviceInformation();
                        tDeviceInformation.SCRADevice = DeviceInformation.SCRADevice;
                        tDeviceInformation.SCRAMSR = DeviceInformation.SCRAMSR;
                        tDeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest = new RequestSendExtendedCommandRequest();
                        tDeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.DeviceID = "";
                        tDeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.ConnectionType = DeviceInformation.SCRADevice.ConnectionType;
                        tDeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.Command = tCommand;
                        tDeviceInformation.SCRAMSR.RequestSendExtendedCommandRequest.WaitTime = SENDEXTENDEDCOMMAND_WAITTIME;
                        tSendCommandResponse = RequestSendExtendedCommand(tDeviceInformation, false);
                        clearAllData();
                    }

                    byte[] tAryAmount = new byte[6];
                    byte[] tAryCashback = new byte[6];
                    bool bResult = false;
                    string tStrAmount = Convert.ToString(DeviceInformation.SCRAMSR.RequestSmartCardRequest.Amount);
                    tStrAmount = tStrAmount.Replace(".", "");
                    bResult = m_MagneFlexCommon.stringTo6Bytes(0, tStrAmount, ref tAryAmount);
                    if (!bResult)
                    {
                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.INVALID_AMOUNT), ERROR_INVALID_AMOUNT, null);
                    }
                    string tStrCashBack = Convert.ToString(DeviceInformation.SCRAMSR.RequestSmartCardRequest.CashBack);
                    tStrCashBack = tStrCashBack.Replace(".", "");
                    bResult = m_MagneFlexCommon.stringTo6Bytes(0, tStrCashBack, ref tAryCashback);
                    if (!bResult)
                    {
                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.INVALID_CASHBACK), ERROR_INVALID_CASHBACK, null);
                    }
                    m_HandleSmartCardWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    m_HandleSendCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    int tStatus = 0;
                    int iRetryCount = 0;
                    for (;;)
                    {
                        tStatus = m_SCRA.startTransaction((byte)DeviceInformation.SCRAMSR.RequestSmartCardRequest.WaitTime, DeviceInformation.SCRAMSR.RequestSmartCardRequest.CardType, DeviceInformation.SCRAMSR.RequestSmartCardRequest.Options, tAryAmount, DeviceInformation.SCRAMSR.RequestSmartCardRequest.TransactionType, tAryCashback, m_MagneFlexCommon.hexToByteArray(DeviceInformation.SCRAMSR.RequestSmartCardRequest.CurrencyCode), DeviceInformation.SCRAMSR.RequestSmartCardRequest.ReportOptions);
                        m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSmartCard: " + tStatus);
                        if (tStatus == 0) break;
                        Thread.Sleep(100);
                        iRetryCount++;
                        if (iRetryCount > MAX_RETRY_COUNT) break;
                    }

                    if (m_HandleSmartCardWaitComplete.WaitOne((DeviceInformation.SCRAMSR.RequestSmartCardRequest.WaitTime + THREAD_WAIT_TIME) * 1000))
                    {
                        tResponse.ResponseOutput = m_DeviceSmartCardResponseData;
                        clearSDKData();
                        if (m_DeviceSmartCardResponseData.BATCHData != null)
                        {
                            if (m_DeviceSmartCardResponseData.BATCHData.Length > 0)
                            {
                                m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSmartCard: Closed Device");
                                if (DeviceInformation.CloseDeviceAfter)
                                {
                                    closeDevice();
                                }
                            }
                        }
                    }
                    else
                    {
                        clearSDKData();
                        closeDevice();
                        if(SelectionForm!=null)
                        {
                            SelectionForm.Close();
                            SelectionForm = null;
                        }

                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_TIMEOUT), ERROR_DEVICE_OPERATION_TIMEOUT, null);
                    }
                }//if (m_SCRA.isDeviceConnected())
                else
                {
                    clearSDKData();
                    closeDevice();
                    throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPEN_FAILED), ERROR_DEVICE_OPEN_FAILED, null);
                }
            }
            catch (MTSCRAFaultException ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSmartCard:" + ex.Message);
                clearAllData();
                closeDevice();
                throw ex;
            }
            catch (Exception ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSmartCard");
                clearAllData();
                closeDevice();
                MTSCRAException tException = new MTSCRAException();
                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), ex.Message, null);
            }
            m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSmartCard");
            return tResponse;


        }
        public RequestSendAcquirerResponseResponse RequestSendAcquirerResponse(MagneFlexCommon.DeviceInformation DeviceInformation)
        {
            m_MagneFlexCommon.debugMessage("+ MTSCRAHelper:RequestSendAcquirerResponse");
            RequestSendAcquirerResponseResponse tResponse = new RequestSendAcquirerResponseResponse();
            tResponse.ResponseOutput = new MTSCRASendAcquirerResponseData();
            Byte[] aryARPC;

            try
            {
                //openDevice(lpRequest.ConnectionType, lpRequest.DeviceID);
                if (m_SCRA.isDeviceConnected())
                {
                    clearAllData();
                    m_HandleSmartCardWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    m_HandleSendCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    aryARPC = m_MagneFlexCommon.hexToByteArray(DeviceInformation.SCRAMSR.RequestSendAcquirerResponseRequest.IssuerAuthenticationData);
                    if (aryARPC != null)
                    {
                        int tStatus = 0;
                        int iRetryCount = 0;
                        for (;;)
                        {
                            m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSendAcquirerResponse:Command:+:" + m_MagneFlexCommon.byteToHex(aryARPC) + ":" + tStatus);
                            tStatus = m_SCRA.setAcquirerResponse(aryARPC);
                            m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSendAcquirerResponse:Command:-:" + m_MagneFlexCommon.byteToHex(aryARPC) + ":" + tStatus);
                            if (tStatus == 0) break;
                            Thread.Sleep(100);
                            iRetryCount++;
                            if (iRetryCount > MAX_RETRY_COUNT) break;
                        }

                        if (m_HandleSmartCardWaitComplete.WaitOne((DeviceInformation.SCRAMSR.RequestSendAcquirerResponseRequest.WaitTime + THREAD_WAIT_TIME) * 1000))
                        {
                            tResponse.ResponseOutput.BATCHData = m_DeviceSmartCardResponseData.BATCHData;
                            tResponse.ResponseOutput.CommandResult = m_DeviceSmartCardResponseData.CommandResult;
                            clearSDKData();
                            if (DeviceInformation.CloseDeviceAfter)
                            {
                                closeDevice();
                            }
                            if (tResponse.ResponseOutput.BATCHData == null)
                            {
                                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_FAILED), ERROR_DEVICE_OPERATION_FAILED, null);
                            }
                            else if (tResponse.ResponseOutput.BATCHData.Length==0)
                            {
                                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_FAILED), ERROR_DEVICE_OPERATION_FAILED, null);
                            }

                        }
                        else
                        {
                            clearSDKData();
                            closeDevice();
                            throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_TIMEOUT), ERROR_DEVICE_OPERATION_TIMEOUT, null);
                        }
                    }
                    else
                    {
                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), ERROR_BUILD_ARPC, null);

                    }


                }//if (m_SCRA.isDeviceConnected())
                else
                {
                        clearSDKData();
                        closeDevice();
                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPEN_FAILED), ERROR_DEVICE_OPEN_FAILED, null);
                }

            }
            catch (MTSCRAFaultException ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendAcquirerResponse");
                clearAllData();
                closeDevice();
                throw ex;
            }
            catch (Exception ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendAcquirerResponse");
                clearAllData();
                closeDevice();
                MTSCRAException tException = new MTSCRAException();
                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), ex.Message, null);
            }
            m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendAcquirerResponse");
            return tResponse;

        }

        public RequestSendCommandResponse RequestSendCommand(MagneFlexCommon.DeviceInformation DeviceInformation)
        {
            m_MagneFlexCommon.debugMessage("+ MTSCRAHelper:RequestSendCommand");
            RequestSendCommandResponse tResponse = new RequestSendCommandResponse();
            tResponse.ResponseOutput = new MTSCRADeviceResponseData();
            m_CardData.DeviceResponseData = m_DeviceResponseData;
            try
            {
                openDevice(DeviceInformation.SCRAMSR.RequestSendCommandRequest.ConnectionType, DeviceInformation.SCRAMSR.RequestSendCommandRequest.DeviceID);
                if (m_SCRA.isDeviceConnected())
                {
                    clearAllData();
                    m_HandleCardDataWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    m_HandleSendCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    //m_MagneFlexCommon.debugMessage(": MTSCRAHelper:Command:" + lpRequest.Command+ " ");
                    int tStatus = 0;
                    int iRetryCount = 0;
                    for (;;)
                    {
                        tStatus = m_SCRA.sendCommandToDevice(DeviceInformation.SCRAMSR.RequestSendCommandRequest.Command);
                        m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSendCommand:Command:" + DeviceInformation.SCRAMSR.RequestSendCommandRequest.Command + ":" + tStatus);
                        if (tStatus == 0) break;
                        Thread.Sleep(100);
                        iRetryCount++;
                        if (iRetryCount > MAX_RETRY_COUNT) break;
                    }

                    if (m_HandleSendCommandWaitComplete.WaitOne((DeviceInformation.SCRAMSR.RequestSendCommandRequest.WaitTime + THREAD_WAIT_TIME) * 1000))
                    {
                        tResponse.ResponseOutput = m_DeviceResponseData;
                        if(DeviceInformation.SCRAMSR.RequestSendCommandRequest.Command.Length >=4)
                        {
                            tResponse.ResponseOutput.CommandID = DeviceInformation.SCRAMSR.RequestSendCommandRequest.Command;
                        }
                        clearSDKData();
                        if(DeviceInformation.CloseDeviceAfter)
                        {
                            m_MagneFlexCommon.debugMessage(": MTSCRAHelper:RequestSendCommand:CloseDevice");
                            closeDevice();
                        }
                    }
                    else
                    {
                        clearSDKData();
                        closeDevice();
                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_TIMEOUT), ERROR_DEVICE_OPERATION_TIMEOUT, null);
                    }
                }//if (m_SCRA.isDeviceConnected())
                else
                {
                    clearSDKData();
                    closeDevice();
                    throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPEN_FAILED), ERROR_DEVICE_OPEN_FAILED, null);
                }
            }
            catch (MTSCRAFaultException ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendCommand");
                clearAllData();
                closeDevice();
                throw ex;
            }
            catch (Exception ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendCommand");
                clearAllData();
                closeDevice();
                MTSCRAException tException = new MTSCRAException();
                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), ex.Message, null);
            }
            m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestSendCommand");
            return tResponse;


        }
        public void ReleaseDevice()
        {
            m_MagneFlexCommon.debugMessage("+ MTSCRAHelper:ReleaseDevice");
            try
            {
                closeDevice();
            }
            catch (MTSCRAFaultException ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:ReleaseDevice");
                clearAllData();
                closeDevice();
                throw ex;
            }
            catch (Exception ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:ReleaseDevice");
                clearAllData();
                closeDevice();
                MTSCRAException tException = new MTSCRAException();
                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), ex.Message, null);
            }
            m_MagneFlexCommon.debugMessage("- MTSCRAHelper:ReleaseDevice");
        }
        public RequestCardSwipeResponse RequestCardSwipe(MagneFlexCommon.DeviceInformation DeviceInformation)
        {
            m_MagneFlexCommon.debugMessage("+ MTSCRAHelper:RequestCardSwipe");
            RequestCardSwipeResponse tResponse = new RequestCardSwipeResponse();
            tResponse.CardSwipeOutput = new MTSCRACardData();
            m_CardData.DeviceResponseData = m_DeviceResponseData;
            try
            {
                openDevice(DeviceInformation.SCRAMSR.RequestCardSwipeRequest.ConnectionType, DeviceInformation.SCRAMSR.RequestCardSwipeRequest.DeviceID);
                if (m_SCRA.isDeviceConnected())
                {
                    m_frmMain.setStatus("Please Swipe a Card.....");
                    clearAllData();
                    m_HandleSendCommandWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    m_HandleCardDataWaitComplete = new EventWaitHandle(false, EventResetMode.ManualReset);
                    if (m_HandleCardDataWaitComplete.WaitOne((DeviceInformation.SCRAMSR.RequestCardSwipeRequest.WaitTime + THREAD_WAIT_TIME) * 1000))
                    {
                        tResponse.CardSwipeOutput = m_CardData;
                        clearSDKData();
                        if (DeviceInformation.CloseDeviceAfter)
                        {
                            closeDevice();
                        }
                    }
                    else
                    {
                        clearSDKData();
                        closeDevice();
                        throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPERATION_TIMEOUT), ERROR_DEVICE_OPERATION_TIMEOUT, null);
                    }
                }//if (m_SCRA.isDeviceConnected())
                else
                {

                    clearSDKData();
                    closeDevice();
                    throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.DEVICE_OPEN_FAILED),ERROR_DEVICE_OPEN_FAILED, null);
                }
            }
            catch (MTSCRAFaultException ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestCardSwipe");
                clearAllData();
                closeDevice();
                throw ex;
            }
            catch (Exception ex)
            {
                m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestCardSwipe");
                clearAllData();
                closeDevice();
                MTSCRAException tException = new MTSCRAException();
                throw new MTSCRAFaultException(Convert.ToString((int)MTSCRAFaultCode.GENERIC), ex.Message, null);
            }
            m_MagneFlexCommon.debugMessage("- MTSCRAHelper:RequestCardSwipe");
            return tResponse;


        }
        void clearSDKData()
        {
            m_SCRA.clearBuffers();
        }

        void clearDeviceResponseData()
        {
            m_DeviceResponseData.CommandID = string.Empty;
            m_DeviceResponseData.Data = string.Empty;
        }
        void clearDeviceSmartCardResponseData()
        {
            m_DeviceSmartCardResponseData.ARQCData = null;
            m_DeviceSmartCardResponseData.BATCHData = null;
            m_DeviceSmartCardResponseData.CommandResult = string.Empty;
            m_DeviceSmartCardResponseData.UserSelection = string.Empty;
        }
        void clearDeviceExtendedResponseData()
        {
            m_DeviceExtendedResponseData.Data = string.Empty;
            m_DeviceExtendedResponseData.CommandID = string.Empty;
        }
        void clearCardSwipeData()
        {
            m_CardData.BatteryLevel = 0;
            m_CardData.CapMagnePrint = string.Empty;
            m_CardData.CapMagnePrint20Encryption = string.Empty;
            m_CardData.CapMagnePrintEncryption = string.Empty;
            m_CardData.CapMagneStripreEncryption = string.Empty;
            m_CardData.CapMSR = string.Empty;
            m_CardData.CapTracks = string.Empty;
            m_CardData.CardDataCRC = 0;
            m_CardData.CardEncodeType = string.Empty;
            m_CardData.CardExpDate = string.Empty;
            m_CardData.CardIIN = string.Empty;
            m_CardData.CardLast4 = string.Empty;
            m_CardData.CardName = string.Empty;
            m_CardData.CardPANLength = 0;
            m_CardData.CardServiceCode = string.Empty;
            m_CardData.ResponseData = string.Empty;
            m_CardData.DataFieldCount = 0;
            m_CardData.DeviceName = string.Empty;
            m_CardData.DeviceSerial = string.Empty;
            m_CardData.EncryptionStatus = string.Empty;
            m_CardData.Firmware = string.Empty;
            m_CardData.TrackDecodeStatus = string.Empty;
            m_CardData.HashCode = string.Empty;
            m_CardData.KSN = string.Empty;
            m_CardData.MagnePrint = string.Empty;
            m_CardData.MagnePrintStatus = string.Empty;
            m_CardData.MagTekDeviceSerial = string.Empty;
            m_CardData.MaskedTracks = string.Empty;
            m_CardData.ResponseType = string.Empty;
            m_CardData.SwipeCount = 0;
            m_CardData.TLVVersion = string.Empty;
            m_CardData.Track1 = string.Empty;
            m_CardData.Track1Masked = string.Empty;
            m_CardData.Track2 = string.Empty;
            m_CardData.Track2Masked = string.Empty;
            m_CardData.Track3 = string.Empty;
            m_CardData.Track3Masked = string.Empty;
        }
        void clearAllData()
        {
            clearSDKData();
            clearCardSwipeData();
            clearDeviceResponseData();
            clearDeviceExtendedResponseData();
            clearDeviceList();
            clearDeviceSmartCardResponseData();
        }
        void clearDeviceList()
        {
            if (m_DeviceList != null) m_DeviceList.Clear();
        }

        public void openDevice(MTConnectionType lpConnectionType, string lpstrDeviceAddress)
        {
            m_MagneFlexCommon.debugMessage("+ MTSCRAHelper:openDevice");
            if (!m_SCRA.isDeviceConnected())
            {
                try
                {
                    m_SCRA.setConnectionType(lpConnectionType);
                    if (lpstrDeviceAddress != null)
                    {
                        if (lpstrDeviceAddress.Trim().Length > 0)
                        {
                            m_SCRA.setAddress(lpstrDeviceAddress);
                        }
                    }
                    m_SCRA.openDevice();
                    if (m_WaitTimeoutAfterOpen != null)
                    {
                        if (m_WaitTimeoutAfterOpen.Length > 0)
                        {
                            int tTimeout = 0;
                            if (Int32.TryParse(m_WaitTimeoutAfterOpen, out tTimeout))
                            {
                                Thread.Sleep(tTimeout);
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                    m_MagneFlexCommon.debugMessage(": MTSCRAHelper:openDevice:+ Wait");
                    if (m_HandleConnectWaitComplete.WaitOne((DEVICE_CONNECT_TIMEOUT) * 1000))
                    {

                    }
                    m_MagneFlexCommon.debugMessage(": MTSCRAHelper:openDevice:- Wait");
                }
                catch (Exception ex)
                {
                    m_MagneFlexCommon.debugMessage("- MTSCRAHelper:openDevice");
                    throw ex;
                }
            }
            else
            {
                m_MagneFlexCommon.debugMessage(": MTSCRAHelper:openDevice:Device Already Opened");

            }
            m_MagneFlexCommon.debugMessage("- MTSCRAHelper:openDevice");
        }
        public void closeDevice()
        {
            try
            {
                if (!m_SCRA.isDeviceConnected()) return;
                m_SCRA.closeDevice();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void OnTransactionResult(object sender, byte[] data)
        {
            m_MagneFlexCommon.debugMessage("+ OnTransactionResult");
            m_MagneFlexCommon.debugMessage(": OnTransactionResult:" + m_MagneFlexCommon.byteToHex(data) + "");
            m_DeviceSmartCardResponseData.BATCHData = m_MagneFlexCommon.byteToHex(data);
            m_HandleSmartCardWaitComplete.Set();
            m_MagneFlexCommon.debugMessage("- OnTransactionResult");
        }

        protected void OnEMVCommandResult(object sender, byte[] data)
        {
            m_MagneFlexCommon.debugMessage("+ OnEMVCommandResult");
            m_MagneFlexCommon.debugMessage(": OnEMVCommandResult:" + m_MagneFlexCommon.byteToHex(data) + "");
            m_DeviceSmartCardResponseData.CommandResult = m_MagneFlexCommon.byteToHex(data);
            if (!m_DeviceSmartCardResponseData.CommandResult.Equals("00000000"))
            {
                m_HandleSmartCardWaitComplete.Set();
            }
            m_MagneFlexCommon.debugMessage("- OnEMVCommandResult");
        }

        protected void OnARQCReceived(object sender, byte[] data)
        {
            m_MagneFlexCommon.debugMessage("+ OnARQCReceived");
            m_MagneFlexCommon.debugMessage(": OnARQCReceived:" + m_MagneFlexCommon.byteToHex(data) + "");
            if(m_DeviceSmartCardResponseData.ARQCData!=null)
            {
                if (m_DeviceSmartCardResponseData.ARQCData.Length == 0)
                {
                    m_DeviceSmartCardResponseData.ARQCData = m_MagneFlexCommon.byteToHex(data);
                    m_HandleSmartCardWaitComplete.Set();
                }
                else
                {
                    m_DeviceSmartCardResponseData.ARQCData = m_MagneFlexCommon.byteToHex(data);
                    m_HandleSmartCardWaitComplete.Set();
                }

            }
            else
            {
                m_DeviceSmartCardResponseData.ARQCData = m_MagneFlexCommon.byteToHex(data);
                m_HandleSmartCardWaitComplete.Set();

            }
            m_MagneFlexCommon.debugMessage("- OnARQCReceived");
        }

        protected void OnDeviceList(object sender, MTLIB.MTConnectionType connectionType, List<MTLIB.MTDeviceInformation> deviceList)
        {
            m_MagneFlexCommon.debugMessage("+ OnDeviceList");
            m_DeviceList = deviceList;
            m_HandleDeviceListWaitComplete.Set();
            m_MagneFlexCommon.debugMessage("- OnDeviceList");
        }

        protected void OnDeviceConnectionStateChanged(object sender, MTLIB.MTConnectionState state)
        {
            m_MagneFlexCommon.debugMessage("+ OnDeviceConnectionStateChanged");
            switch (state)
            {
                case MTConnectionState.Connected:
                    m_MagneFlexCommon.debugMessage(": OnDeviceConnectionStateChanged:Connected");
                    Thread.Sleep(100);
                    m_HandleConnectWaitComplete.Set();
                    break;
                case MTConnectionState.Connecting:
                    m_MagneFlexCommon.debugMessage(": OnDeviceConnectionStateChanged:Connecting");
                    break;
                case MTConnectionState.Disconnected:
                    m_HandleConnectWaitComplete.Set();
                    m_MagneFlexCommon.debugMessage(": OnDeviceConnectionStateChanged:Disconnected");
                    break;
                case MTConnectionState.Disconnecting:
                    m_MagneFlexCommon.debugMessage(": OnDeviceConnectionStateChanged:Disconnecting");
                    break;
                case MTConnectionState.Error:
                    m_HandleConnectWaitComplete.Set();
                    m_MagneFlexCommon.debugMessage(": OnDeviceConnectionStateChanged:Disconnecting");
                    break;
            }
            m_MagneFlexCommon.debugMessage("- OnDeviceConnectionStateChanged");
        }

        protected void OnCardDataStateChanged(object sender, MTLIB.MTCardDataState state)
        {
            m_MagneFlexCommon.debugMessage("+ OnCardDataStateChanged");
            switch (state)
            {
                case MTCardDataState.DataError:
                    m_MagneFlexCommon.debugMessage(": OnCardDataStateChanged:DataError");
                    break;
                case MTCardDataState.DataNotReady:
                    m_MagneFlexCommon.debugMessage(": OnCardDataStateChanged:DataNotReady");
                    break;
                case MTCardDataState.DataReady:
                    m_MagneFlexCommon.debugMessage(": OnCardDataStateChanged:DataReady");
                    break;
            }
            m_MagneFlexCommon.debugMessage("- OnCardDataStateChanged");
        }
        public void OnDataReceivedSPI(MTSCRAASCCardData cardData, string responseData)
        {
            m_MagneFlexCommon.debugMessage("+ OnDataReceivedSPIUART");
            m_MagneFlexCommon.debugMessage(": OnDataReceivedSPIUART: + Reset Event");
            m_CardData.BatteryLevel = cardData.getBatteryLevel();
            m_CardData.CapMagnePrint = cardData.getCapMagnePrint();
            m_CardData.CapMagnePrint20Encryption = cardData.getCapMagneSafe20Encryption();
            m_CardData.CapMagnePrintEncryption = cardData.getCapMagnePrintEncryption();
            m_CardData.CapMagneStripreEncryption = cardData.getCapMagStripeEncryption();
            m_CardData.CapMSR = cardData.getCapMSR();
            m_CardData.CapTracks = cardData.getCapTracks();
            m_CardData.CardDataCRC = cardData.getCardDataCRC();
            m_CardData.CardEncodeType = cardData.getCardEncodeType();
            m_CardData.CardExpDate = cardData.getCardExpDate();
            m_CardData.CardIIN = cardData.getCardIIN();
            m_CardData.CardLast4 = cardData.getCardLast4();
            m_CardData.CardName = cardData.getCardName();
            m_CardData.CardPANLength = cardData.getCardPANLength();
            m_CardData.CardServiceCode = cardData.getCardServiceCode();
            m_CardData.ResponseData = responseData;
            m_CardData.DeviceResponseData.Data = responseData;
            m_CardData.DataFieldCount = cardData.getDataFieldCount();
            m_CardData.DeviceName = cardData.getDeviceName();
            m_CardData.DeviceSerial = cardData.getDeviceSerial();
            m_CardData.EncryptionStatus = cardData.getEncryptionStatus();
            m_CardData.Firmware = cardData.getFirmware();
            m_CardData.TrackDecodeStatus = cardData.getTrackDecodeStatus();
            m_CardData.HashCode = cardData.getHashCode();
            m_CardData.KSN = cardData.getKSN();
            m_CardData.MagnePrint = cardData.getMagnePrint();
            m_CardData.MagnePrintStatus = cardData.getMagnePrintStatus();
            m_CardData.MagTekDeviceSerial = cardData.getMagTekDeviceSerial();
            m_CardData.MaskedTracks = cardData.getMaskedTracks();
            m_CardData.ResponseType = cardData.getResponseType();
            m_CardData.SwipeCount = cardData.getSwipeCount();
            m_CardData.TLVVersion = cardData.getTLVVersion();
            m_CardData.Track1 = cardData.getTrack1();
            m_CardData.Track1Masked = cardData.getTrack1Masked();
            m_CardData.Track2 = cardData.getTrack2();
            m_CardData.Track2Masked = cardData.getTrack2Masked();
            m_CardData.Track3 = cardData.getTrack3();
            m_CardData.Track3Masked = cardData.getTrack3Masked();
            m_HandleCardDataWaitComplete.Set();
            m_MagneFlexCommon.debugMessage(": OnDataReceivedSPIUART: - Reset Event");
            m_MagneFlexCommon.debugMessage("- OnDataReceivedSPIUART");
        }
        public void OnDataReceivedUART(string data)
        {
            m_MagneFlexCommon.debugMessage("+ OnDataReceivedUART:" + data);
            m_CardData.ResponseData += data;
            m_CardData.DeviceResponseData.Data += data;
            if (data.EndsWith("0D"))
            {
                /*
                m_MagneFlexCommon.debugMessage(": OnDataReceivedUART: + Reset Event");
                m_HandleCardDataWaitComplete.Set();
                m_MagneFlexCommon.debugMessage(": OnDataReceivedUART: - Reset Event");
                */
            }
            m_MagneFlexCommon.debugMessage("- OnDataReceivedUART");
        }


        protected void OnDataReceived(object sender, IMTCardData cardData)
        {
            m_MagneFlexCommon.debugMessage("+ OnDataReceived");
            string tCardData = "";
            m_CardData.BatteryLevel = m_SCRA.getBatteryLevel();
            m_CardData.CapMagnePrint = m_SCRA.getCapMagnePrint();
            m_CardData.CapMagnePrint20Encryption = m_SCRA.getCapMagneSafe20Encryption();
            m_CardData.CapMagnePrintEncryption = m_SCRA.getCapMagnePrintEncryption();
            m_CardData.CapMagneStripreEncryption = m_SCRA.getCapMagStripeEncryption();
            m_CardData.CapMSR = m_SCRA.getCapMSR();
            m_CardData.CapTracks = m_SCRA.getCapTracks();
            m_CardData.CardDataCRC = m_SCRA.getCardDataCRC();
            m_CardData.CardEncodeType = m_SCRA.getCardEncodeType();
            m_CardData.CardExpDate = m_SCRA.getCardExpDate();
            m_CardData.CardIIN = m_SCRA.getCardIIN();
            m_CardData.CardLast4 = m_SCRA.getCardLast4();
            m_CardData.CardName = m_SCRA.getCardName();
            m_CardData.CardPANLength = m_SCRA.getCardPANLength();
            m_CardData.CardServiceCode = m_SCRA.getCardServiceCode();
            m_CardData.ResponseData = m_SCRA.getResponseData();
            m_CardData.DataFieldCount = m_SCRA.getDataFieldCount();
            m_CardData.DeviceName = m_SCRA.getDeviceName();
            m_CardData.DeviceSerial = m_SCRA.getDeviceSerial();
            m_CardData.EncryptionStatus = m_SCRA.getEncryptionStatus();
            m_CardData.Firmware = m_SCRA.getFirmware();
            m_CardData.TrackDecodeStatus = m_SCRA.getTrackDecodeStatus();
            m_CardData.HashCode = m_SCRA.getHashCode();
            m_CardData.KSN = m_SCRA.getKSN();
            m_CardData.MagnePrint = m_SCRA.getMagnePrint();
            m_CardData.MagnePrintStatus = m_SCRA.getMagnePrintStatus();
            m_CardData.MagTekDeviceSerial = m_SCRA.getMagTekDeviceSerial();
            m_CardData.MaskedTracks = m_SCRA.getMaskedTracks();
            m_CardData.ResponseType = m_SCRA.getResponseType();
            m_CardData.SwipeCount = m_SCRA.getSwipeCount();
            m_CardData.TLVVersion = m_SCRA.getTLVVersion();
            m_CardData.Track1 = m_SCRA.getTrack1();
            m_CardData.Track1Masked = m_SCRA.getTrack1Masked();
            m_CardData.Track2 = m_SCRA.getTrack2();
            m_CardData.Track2Masked = m_SCRA.getTrack2Masked();
            m_CardData.Track3 = m_SCRA.getTrack3();
            m_CardData.Track3Masked = m_SCRA.getTrack3Masked();

            tCardData += string.Format("SDK.Version={0}\n", m_SCRA.getSDKVersion());

            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("TLV.Version={0}\n", m_CardData.TLVVersion);

            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Response.Type={0}\n", m_SCRA.getResponseType());

            tCardData += string.Format("Tracks.Masked={0}\n", m_CardData.MaskedTracks);
            tCardData += string.Format("Track1.Encrypted={0}\n", m_CardData.Track1);
            tCardData += string.Format("Track2.Encrypted={0}\n", m_CardData.Track2);
            tCardData += string.Format("Track3.Encrypted={0}\n", m_CardData.Track3);
            tCardData += string.Format("Track1.Masked={0}\n", m_CardData.Track1Masked);
            tCardData += string.Format("Track2.Masked={0}\n", m_CardData.Track2Masked);
            tCardData += string.Format("Track3.Masked={0}\n", m_CardData.Track3Masked);
            tCardData += string.Format("MagnePrint.Encrypted={0}\n", m_CardData.MagnePrint);
            tCardData += string.Format("MagnePrint.Length={0} bytes\n", (m_CardData.MagnePrint.Length / 2));
            tCardData += string.Format("MagnePrint.Status={0}\n", m_CardData.MagnePrintStatus);
            tCardData += string.Format("Device.Serial={0}\n", m_CardData.DeviceSerial);
            tCardData += string.Format("Session.ID={0}\n", m_SCRA.getSessionID());
            tCardData += string.Format("KSN={0}\n", m_CardData.KSN);

            if (m_SCRA.getSwipeCount() >= 0)
            {
                tCardData += string.Format("Swipe.Count={0}\n", m_CardData.SwipeCount);
            }

            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Cap.MagnePrint={0}\n", m_CardData.CapMagnePrint);
            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Cap.MagnePrintEncryption={0}\n", m_CardData.CapMagnePrintEncryption);
            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Cap.MagneSafe20Encryption={0}\n", m_CardData.CapMagnePrint20Encryption);
            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Cap.MagStripeEncryption={0}\n", m_CardData.CapMagneStripreEncryption);
            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Cap.MSR={0}\n", m_CardData.CapMSR);
            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Cap.Tracks={0}\n", m_CardData.CapTracks);

            tCardData += string.Format("Card.Data.CRC={0}\n", m_CardData.CardDataCRC);
            tCardData += string.Format("Card.Exp.Date={0}\n", m_CardData.CardExpDate);
            tCardData += string.Format("Card.IIN={0}\n", m_CardData.CardIIN);
            tCardData += string.Format("Card.Last4={0}\n", m_CardData.CardLast4);
            tCardData += string.Format("Card.Name={0}\n", m_CardData.CardName);
            tCardData += string.Format("Card.PAN.Length={0}\n", m_CardData.CardPANLength);
            tCardData += string.Format("Card.Service.Code={0}\n", m_CardData.CardServiceCode);

            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Card.Status={0}\n", m_SCRA.getCardStatus());
            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("Card.EncodeType={0}\n", m_SCRA.getCardEncodeType());

            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("HashCode={0}\n", m_CardData.HashCode);

            if (m_CardData.DataFieldCount != 0)
            {
                tCardData += string.Format("Data.Field.Count={0}\n", m_CardData.DataFieldCount);
            }

            tCardData += string.Format("Encryption.Status={0}\n", m_CardData.EncryptionStatus);

            tCardData += m_MagneFlexCommon.formatStringIfNotEmpty("MagTek.Device.Serial={0}\n", m_CardData.MagnePrintStatus);

            tCardData += string.Format("Track.Decode.Status={0}\n", m_CardData.TrackDecodeStatus);
            string tkStatus = m_CardData.TrackDecodeStatus;

            string tk1Status = "01";
            string tk2Status = "01";
            string tk3Status = "01";

            if (tkStatus.Length >= 6)
            {
                tk1Status = tkStatus.Substring(0, 2);
                tk2Status = tkStatus.Substring(2, 2);
                tk3Status = tkStatus.Substring(4, 2);

                tCardData += string.Format("Track1.Status={0}\n", tk1Status);
                tCardData += string.Format("Track2.Status={0}\n", tk2Status);
                tCardData += string.Format("Track3.Status={0}\n", tk3Status);
            }

            tCardData += string.Format("Battery.Level={0}\n", m_CardData.BatteryLevel);
            //m_MagneFlexCommon.debugMessage(string.Format(":OnDataReceived={0}\n", tCardData));
            m_MagneFlexCommon.debugMessage(": OnDataReceived: + Reset Event");
            m_HandleCardDataWaitComplete.Set();
            m_MagneFlexCommon.debugMessage(": OnDataReceived: - Reset Event");
            m_MagneFlexCommon.debugMessage("- OnDataReceived");
        }
        protected void OnDeviceExtendedResponse(object sender, string data)
        {
            m_MagneFlexCommon.debugMessage("+ OnDeviceExtendedResponse");
            m_MagneFlexCommon.debugMessage(": OnDeviceExtendedResponse:" + data + "");
            if (!m_SCRA.isDeviceOEM())
            {
                m_DeviceExtendedResponseData.Data = data;
            }
            else
            {
                m_MTSCRASPIUART.processDeviceExtendedResponse(data);
            }
            m_HandleSendExtendedCommandWaitComplete.Set();
            m_MagneFlexCommon.debugMessage("- OnDeviceExtendedResponse");

        }

        protected void OnDeviceResponse(object sender, string data)
        {
            m_MagneFlexCommon.debugMessage("+ OnDeviceResponse");
            m_MagneFlexCommon.debugMessage(": OnDeviceResponse:" + data + "");
            if (data.Equals("0000"))
            {
                if (string.IsNullOrEmpty(m_DeviceResponseData.Data))
                {
                    m_DeviceResponseData.Data = data;
                }
            }
            else
            {
                m_DeviceResponseData.Data = data;
            }
            m_CardData.DeviceResponseData = m_DeviceResponseData;
            m_HandleSendCommandWaitComplete.Set();
            m_MagneFlexCommon.debugMessage("- OnDeviceResponse");
        }

        protected void OnTransactionStatus(object sender, byte[] data)
        {
            m_MagneFlexCommon.debugMessage("+ OnTransactionStatus");
            m_MagneFlexCommon.debugMessage("- OnTransactionStatus");
        }

        protected void OnDisplayMessageRequest(object sender, byte[] data)
        {
            m_MagneFlexCommon.debugMessage("+ OnDisplayMessageRequest");
            if (data != null)
            {
                if (data.Length > 0)
                {
                    m_MagneFlexCommon.debugMessage(": OnDisplayMessageRequest" + m_MagneFlexCommon.byteToHex(data) + "");
                }
            }
            m_MagneFlexCommon.debugMessage("- OnDisplayMessageRequest");
        }


        protected void OnUserSelectionRequest(object sender, byte[] data)
        {
            m_MagneFlexCommon.debugMessage("+ OnUserSelectionRequest");
            if (data != null)
            {
                string tUserSelection = m_MagneFlexCommon.byteToHex(data);
                m_MagneFlexCommon.debugMessage(": OnUserSelectionRequest:" + tUserSelection + "");
                string tUserSelectionStatus = ConfigurationManager.AppSettings["USER_SELECTION_" + tUserSelection + "_STATUS"];
                string tUserSelectionSelection = ConfigurationManager.AppSettings["USER_SELECTION_" + tUserSelection + "_SELECTION"];
                int iData = 0;
                int iUserSelectionStatus = 0;
                int iUserSelection = 0;

                if (int.TryParse(tUserSelectionStatus, out iData))
                {
                    iUserSelectionStatus = iData;
                }//if (!int.TryParse(ConfigurationManager.AppSettings["ProcessCardSwipe_Tones"], out iData))
                if (int.TryParse(tUserSelectionSelection, out iData))
                {
                    iUserSelection = iData;
                }//if (!int.TryParse(ConfigurationManager.AppSettings["ProcessCardSwipe_Tones"], out iData))
                //StringBuilder tSelectionList = new StringBuilder();
                Dictionary<string, string> tSelectionList =
                            new Dictionary<string, string>();
                if (data != null)
                {
                    if (data.Length > 0)
                    {
                        byte[] aryTemp = new byte[data.Length];
                        int j = 0;
                        for (int i = 2; i < data.Length; i++)
                        {
                            if (data[i] != 0)
                            {
                                aryTemp[j++] = data[i];
                            }
                            else
                            {
                                tSelectionList.Add(i.ToString(),System.Text.Encoding.UTF8.GetString(aryTemp));
                                j = 0;
                                Array.Clear(aryTemp, 0, aryTemp.Length);
                            }
                        }
                    }
                }
                if(tSelectionList.Keys.Count > 0)
                {
                    SelectionForm = new frmSelection();
                    SelectionForm.m_SelectionData = tSelectionList;
                    SelectionForm.ShowDialog();
                    if(SelectionForm.DialogResult== System.Windows.Forms.DialogResult.OK)
                    {
                        iUserSelectionStatus = 0;
                        iUserSelection = SelectionForm.m_SelectionIndex;
                    }
                    else
                    {
                        iUserSelectionStatus = MTEMVDeviceConstants.SELECTION_STATUS_CANCELLED;
                    }
                    SelectionForm = null;

                }
                int tStatus = 0;
                int iRetryCount = 0;
                for (;;)
                {
                    tStatus = m_SCRA.setUserSelectionResult((byte)iUserSelectionStatus, (byte)iUserSelection);
                    m_MagneFlexCommon.debugMessage(": MTSCRAHelper:OnUserSelectionRequest:Status:" + iUserSelectionStatus + ",Selection:" + iUserSelection + ":" + tStatus + " ");
                    if (tStatus == 0) break;
                    Thread.Sleep(100);
                    iRetryCount++;
                    if (iRetryCount > MAX_RETRY_COUNT) break;
                }

                m_DeviceSmartCardResponseData.UserSelection = m_MagneFlexCommon.byteToHex(data);
                //m_HandleSmartCardWaitComplete.Set();
            }
            m_MagneFlexCommon.debugMessage("- OnUserSelectionRequest");
        }


    }


}
