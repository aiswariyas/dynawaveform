#!/usr/bin/env python3.7
import requests
import os 
from typing import NamedTuple
from Mppgv4Services import pfxtopemutil

class ProcessTokenRequest(NamedTuple):
    additionalRequestDataKey :str
    additionalRequestDataValue :str
    userName :str 
    passWord :str
    customerCode :str
    customerTransactionId :str
    token :str
    amount :float
    processorName :str
    transactionType :str

class ProcessToken:
    def __init__(self,processTokenRequest): 
            self.__processTokenRequest = processTokenRequest


    def CallService(self,webServiceUrl,certificateFileName,certificatePassword):
        soapRequest = f"""<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
  <soapenv:Header/>
  <soapenv:Body>
    <mpp:ProcessToken>
      <mpp:ProcessTokenRequests>
        <mpp1:ProcessTokenRequest>
          <mpp1:AdditionalRequestData>
            <sys:KeyValuePairOfstringstring>
              <sys:key>{self.__processTokenRequest.additionalRequestDataKey}</sys:key>
              <sys:value Encoding="cdata">{self.__processTokenRequest.additionalRequestDataValue}</sys:value>
            </sys:KeyValuePairOfstringstring>
          </mpp1:AdditionalRequestData>
          <mpp1:Authentication>
            <mpp1:CustomerCode>{self.__processTokenRequest.customerCode}</mpp1:CustomerCode>
            <mpp1:Password>{self.__processTokenRequest.passWord}</mpp1:Password>
            <mpp1:Username>{self.__processTokenRequest.userName}</mpp1:Username>
          </mpp1:Authentication>
          <mpp1:CustomerTransactionID>{self.__processTokenRequest.customerTransactionId}</mpp1:CustomerTransactionID>
          <mpp1:Token>{self.__processTokenRequest.token}</mpp1:Token>
          <mpp1:TransactionInput>
            <mpp1:Amount>{self.__processTokenRequest.amount}</mpp1:Amount>
            <mpp1:ProcessorName>{self.__processTokenRequest.processorName}</mpp1:ProcessorName>
            <mpp1:TransactionInputDetails>
              <sys:KeyValuePairOfstringstring>
                <sys:key/>
                <sys:value/>
              </sys:KeyValuePairOfstringstring>
            </mpp1:TransactionInputDetails>
            <mpp1:TransactionType>{self.__processTokenRequest.transactionType}</mpp1:TransactionType>
          </mpp1:TransactionInput>
        </mpp1:ProcessTokenRequest>
      </mpp:ProcessTokenRequests>
    </mpp:ProcessToken>
  </soapenv:Body>
</soapenv:Envelope>"""

        headers = {
        "Content-Type": "text/xml;charset=utf-8",
        "Content-Length":  str(len(soapRequest)),
        "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessToken"
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