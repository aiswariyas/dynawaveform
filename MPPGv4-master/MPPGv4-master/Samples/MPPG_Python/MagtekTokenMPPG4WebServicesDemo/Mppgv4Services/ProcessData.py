#!/usr/bin/env python3.7
import requests
import os 
from typing import NamedTuple
from Mppgv4Services import pfxtopemutil

class ProcessDataRequest(NamedTuple):
    """Description of class"""
    customerCode :str
    userName :str 
    passWord :str
    customerTransactionId :str
    data :str
    dataFormatType :str
    encryptionType :str
    ksn :str
    numberOfPaddedBytes :int
    isEncrypted :str
    paymentMode :str
    amount :float
    processorName :str
    transactionType :str


class ProcessData:
    def __init__(self,processDataRequest): 
        self.__processDataRequest = processDataRequest


    def CallService(self,webServiceUrl,certificateFileName,certificatePassword):
        soapRequest = f"""<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
	<soapenv:Header/>
	<soapenv:Body>
		<mpp:ProcessData>         
			<mpp:ProcessDataRequests>            
				<mpp1:ProcessDataRequest>               
					<mpp1:AdditionalRequestData>                  
						<sys:KeyValuePairOfstringstring>
							<sys:key/>
							<sys:value/>
						</sys:KeyValuePairOfstringstring>
					</mpp1:AdditionalRequestData>
					<mpp1:Authentication>
						<mpp1:CustomerCode>{self.__processDataRequest.customerCode}</mpp1:CustomerCode>
						<mpp1:Password>{self.__processDataRequest.passWord}</mpp1:Password>
						<mpp1:Username>{self.__processDataRequest.userName}</mpp1:Username>
					</mpp1:Authentication>               
					<mpp1:CustomerTransactionID>{self.__processDataRequest.customerTransactionId}</mpp1:CustomerTransactionID>
					<mpp1:DataInput>
						<mpp1:Data>{self.__processDataRequest.data}</mpp1:Data>
						<mpp1:DataFormatType>{self.__processDataRequest.dataFormatType}</mpp1:DataFormatType>                  
						<mpp1:EncryptionInfo>                     
							<mpp1:EncryptionType>{self.__processDataRequest.encryptionType}</mpp1:EncryptionType>                     
							<mpp1:KSN>{self.__processDataRequest.ksn}</mpp1:KSN>
							<mpp1:NumberOfPaddedBytes>{self.__processDataRequest.numberOfPaddedBytes}</mpp1:NumberOfPaddedBytes>
						</mpp1:EncryptionInfo>
						<mpp1:IsEncrypted>{self.__processDataRequest.isEncrypted}</mpp1:IsEncrypted>
						<mpp1:PaymentMode>{self.__processDataRequest.paymentMode}</mpp1:PaymentMode>
					</mpp1:DataInput>
					<mpp1:TransactionInput>
						<mpp1:Amount>{self.__processDataRequest.amount}</mpp1:Amount>
						<mpp1:ProcessorName>{self.__processDataRequest.processorName}</mpp1:ProcessorName>
						<mpp1:TransactionInputDetails>
							<sys:KeyValuePairOfstringstring>
								<sys:key/>
								<sys:value/>
							</sys:KeyValuePairOfstringstring>
						</mpp1:TransactionInputDetails>
						<mpp1:TransactionType>{self.__processDataRequest.transactionType}</mpp1:TransactionType>
					</mpp1:TransactionInput>
				</mpp1:ProcessDataRequest>
			</mpp:ProcessDataRequests>
		</mpp:ProcessData>
	</soapenv:Body>
</soapenv:Envelope>"""

        headers = {
        "Content-Type": "text/xml;charset=utf-8",
        "Content-Length":  str(len(soapRequest)),
        "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessData"
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
