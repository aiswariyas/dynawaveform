#!/usr/bin/env python3.7
import requests
import os 
from typing import NamedTuple
from Mppgv4Services import pfxtopemutil

class ProcessKeyPadEntryRequest(NamedTuple):
    """description of class"""
    customerCode :str
    userName :str 
    passWord :str
    deviceSn :str
    ksn :str
    magnePrint :str
    magnePrintStatus :str
    track1 :str
    track2 :str
    customerTransactionId :str
    amount :float
    processorName :str
    transactionType :str

class ProcessKeyPadEntry:
    def __init__(self,processKeyPadEntryRequest): 
        self.__processKeyPadEntryRequest = processKeyPadEntryRequest


    def CallService(self,webServiceUrl,certificateFileName,certificatePassword):
        soapRequest = f"""<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
  <soapenv:Header/>
  <soapenv:Body>
    <mpp:ProcessKeyPadEntry>
      <mpp:ProcessKeyPadEntryRequests>
        <mpp1:ProcessKeyPadEntryRequest>
          <mpp1:AdditionalRequestData>
            <sys:KeyValuePairOfstringstring>
              <sys:key></sys:key>
              <sys:value></sys:value>
            </sys:KeyValuePairOfstringstring>
          </mpp1:AdditionalRequestData>
          <mpp1:Authentication>
            <mpp1:CustomerCode>{self.__processKeyPadEntryRequest.customerCode}</mpp1:CustomerCode>
            <mpp1:Password>{self.__processKeyPadEntryRequest.passWord}</mpp1:Password>
            <mpp1:Username>{self.__processKeyPadEntryRequest.userName}</mpp1:Username>
          </mpp1:Authentication>
          <mpp1:CardSwipeInput>            
            <mpp1:EncryptedCardSwipe>
              <mpp1:DeviceSN>{self.__processKeyPadEntryRequest.deviceSn}</mpp1:DeviceSN>
              <mpp1:KSN>{self.__processKeyPadEntryRequest.ksn}</mpp1:KSN>
              <mpp1:MagnePrint>{self.__processKeyPadEntryRequest.magnePrint}</mpp1:MagnePrint>
              <mpp1:MagnePrintStatus>{self.__processKeyPadEntryRequest.magnePrintStatus}</mpp1:MagnePrintStatus>
              <mpp1:Track1>{self.__processKeyPadEntryRequest.track1}</mpp1:Track1>
              <mpp1:Track2>{self.__processKeyPadEntryRequest.track2}</mpp1:Track2>
              <mpp1:Track3/>
            </mpp1:EncryptedCardSwipe>          
          </mpp1:CardSwipeInput>
          <mpp1:CustomerTransactionID>{self.__processKeyPadEntryRequest.customerTransactionId}</mpp1:CustomerTransactionID>
          <mpp1:TransactionInput>
            <mpp1:Amount>{self.__processKeyPadEntryRequest.amount}</mpp1:Amount>
            <mpp1:ClientCertAsBase64String></mpp1:ClientCertAsBase64String>
            <mpp1:ClientCertPassword></mpp1:ClientCertPassword>
            <mpp1:ProcessorName>{self.__processKeyPadEntryRequest.processorName}</mpp1:ProcessorName>
            <mpp1:TransactionInputDetails>
              <sys:KeyValuePairOfstringstring>
                <sys:key></sys:key>
                <sys:value></sys:value>
              </sys:KeyValuePairOfstringstring>
            </mpp1:TransactionInputDetails>
            <mpp1:TransactionType>{self.__processKeyPadEntryRequest.transactionType}</mpp1:TransactionType>
          </mpp1:TransactionInput>
        </mpp1:ProcessKeyPadEntryRequest>
      </mpp:ProcessKeyPadEntryRequests>
    </mpp:ProcessKeyPadEntry>
  </soapenv:Body>
</soapenv:Envelope>"""

        headers = {
        "Content-Type": "text/xml;charset=utf-8",
        "Content-Length":  str(len(soapRequest)),
        "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessKeyPadEntry"
        }

        response = None

        if ((certificateFileName is None) or (certificateFileName.strip() == "")):
            #send soap request without attaching certificate
            response = requests.post(webServiceUrl,data=soapRequest,headers=headers)
        else:
            util = pfxtopemutil.PfxToPemUtility()
            try:
                util.Convert(certificateFileName, certificatePassword) 
                response = requests.post(webServiceUrl, cert=util.tempFileName, data=soapRequest,headers=headers)
            except Exception as ex:
                print(ex)
            finally:
                if ((not util.tempFileName is None) and (os.path.exists(util.tempFileName))):
                    os.remove(util.tempFileName)
        return response