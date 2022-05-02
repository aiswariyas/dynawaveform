#!/usr/bin/env python3.7
import requests
import os 
from typing import NamedTuple
from Mppgv4Services import pfxtopemutil

class ProcessManualEntryRequest(NamedTuple):
    """Description Of ProcessManualEntryRequest"""
    customerCode :str
    userName :str
    passWord :str
    customerTransactionId :str
    addressline1 :str
    addressline2 :str
    city :str
    country :str
    expirationDate :str
    nameOnCard :str
    pan :str
    state :str
    amount :float
    processorName :str
    transactionType :str


class ProcessManualEntry:
    def __init__(self,processManualEntryRequest): 
        self.__processManualEntryRequest = processManualEntryRequest


    def CallService(self,webServiceUrl,certificateFileName,certificatePassword):
        soapRequest = f"""
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
	<soapenv:Header/>
	<soapenv:Body>
		<mpp:ProcessManualEntry>         
			<mpp:ProcessManualEntryRequests>            
				<mpp1:ProcessManualEntryRequest>               
					<mpp1:AdditionalRequestData>                  
						<sys:KeyValuePairOfstringstring>
							<sys:key/>
							<sys:value/>
						</sys:KeyValuePairOfstringstring>
					</mpp1:AdditionalRequestData>
					<mpp1:Authentication>
						<mpp1:CustomerCode>{self.__processManualEntryRequest.customerCode}</mpp1:CustomerCode>
						<mpp1:Password>{self.__processManualEntryRequest.passWord}</mpp1:Password>
						<mpp1:Username>{self.__processManualEntryRequest.userName}</mpp1:Username>
					</mpp1:Authentication>                 
					<mpp1:CustomerTransactionID>{self.__processManualEntryRequest.customerTransactionId}</mpp1:CustomerTransactionID>
					<mpp1:ManualEntryInput>						
						<mpp1:AddressLine1>{self.__processManualEntryRequest.addressline1}</mpp1:AddressLine1>						
						<mpp1:AddressLine2>{self.__processManualEntryRequest.addressline2}</mpp1:AddressLine2>						
						<mpp1:City>{self.__processManualEntryRequest.city}</mpp1:City>						
						<mpp1:Country>{self.__processManualEntryRequest.country}</mpp1:Country>
						<mpp1:ExpirationDate>{self.__processManualEntryRequest.expirationDate}</mpp1:ExpirationDate>						
						<mpp1:NameOnCard>{self.__processManualEntryRequest.nameOnCard}</mpp1:NameOnCard>
						<mpp1:PAN>{self.__processManualEntryRequest.pan}</mpp1:PAN>						
						<mpp1:State>{self.__processManualEntryRequest.state}</mpp1:State>						
					</mpp1:ManualEntryInput>
					<mpp1:TransactionInput>                  
						<mpp1:Amount>{self.__processManualEntryRequest.amount}</mpp1:Amount>                  
						<mpp1:ProcessorName>{self.__processManualEntryRequest.processorName}</mpp1:ProcessorName>                  
						<mpp1:TransactionInputDetails>                     
							<sys:KeyValuePairOfstringstring>
								<sys:key/>
								<sys:value/>
							</sys:KeyValuePairOfstringstring>
						</mpp1:TransactionInputDetails>
						<mpp1:TransactionType>{self.__processManualEntryRequest.transactionType}</mpp1:TransactionType>
					</mpp1:TransactionInput>
				</mpp1:ProcessManualEntryRequest>
			</mpp:ProcessManualEntryRequests>
		</mpp:ProcessManualEntry>
	</soapenv:Body>
</soapenv:Envelope>
        """

        headers = {
        "Content-Type": "text/xml;charset=utf-8",
        "Content-Length":  str(len(soapRequest)),
        "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessManualEntry"
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
