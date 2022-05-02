#!/usr/bin/env python3.7
import requests
import os 
from typing import NamedTuple
from Mppgv4Services import pfxtopemutil

class ProcessCardSwipeRequest(NamedTuple):
    """Description Of CreateTokensRequest"""
    customerCode :str
    userName :str
    passWord :str
    deviceSN :str
    ksn :str
    magnePrint :str
    magnePrintStatus :str
    track1 :str
    track2 :str
    customerTransactionID :str
    amount :float
    processorName :str
    transactionType :str


class ProcessCardSwipe:
    def __init__(self,processCardSwipeRequest): 
        self.__processCardSwipeRequest = processCardSwipeRequest


    def CallService(self,webServiceUrl,certificateFileName,certificatePassword):
        soapRequest = f"""
        <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
	<soapenv:Header/>
	<soapenv:Body>
		<mpp:ProcessCardSwipe>
			<mpp:ProcessCardSwipeRequests>
				<mpp1:ProcessCardSwipeRequest>
					<mpp1:AdditionalRequestData>
						<sys:KeyValuePairOfstringstring>
							<sys:key/>
							<sys:value/>
						</sys:KeyValuePairOfstringstring>
					</mpp1:AdditionalRequestData>
					<mpp1:Authentication>
						<mpp1:CustomerCode>{self.__processCardSwipeRequest.customerCode}</mpp1:CustomerCode>
						<mpp1:Password>{self.__processCardSwipeRequest.passWord}</mpp1:Password>
						<mpp1:Username>{self.__processCardSwipeRequest.userName}</mpp1:Username>
					</mpp1:Authentication>   
					<mpp1:CardSwipeInput>
						<mpp1:EncryptedCardSwipe>
							<mpp1:DeviceSN>{self.__processCardSwipeRequest.deviceSN}</mpp1:DeviceSN>
							<mpp1:KSN>{self.__processCardSwipeRequest.ksn}</mpp1:KSN>
							<mpp1:MagnePrint>{self.__processCardSwipeRequest.magnePrint}</mpp1:MagnePrint>
							<mpp1:MagnePrintStatus>{self.__processCardSwipeRequest.magnePrintStatus}</mpp1:MagnePrintStatus>
							<mpp1:Track1>{self.__processCardSwipeRequest.track1}</mpp1:Track1>
							<mpp1:Track2>{self.__processCardSwipeRequest.track2}</mpp1:Track2>
							<mpp1:Track3/>
						</mpp1:EncryptedCardSwipe>
					</mpp1:CardSwipeInput>
					<mpp1:CustomerTransactionID>{self.__processCardSwipeRequest.customerTransactionID}</mpp1:CustomerTransactionID>
					<mpp1:TransactionInput>
						<mpp1:Amount>{self.__processCardSwipeRequest.amount}</mpp1:Amount>
						<mpp1:ProcessorName>{self.__processCardSwipeRequest.processorName}</mpp1:ProcessorName>
						<mpp1:TransactionInputDetails>
							<sys:KeyValuePairOfstringstring>
								<sys:key/>
								<sys:value/>
							</sys:KeyValuePairOfstringstring>
						</mpp1:TransactionInputDetails>
						<mpp1:TransactionType>{self.__processCardSwipeRequest.transactionType}</mpp1:TransactionType>
					</mpp1:TransactionInput>
				</mpp1:ProcessCardSwipeRequest>
			</mpp:ProcessCardSwipeRequests>
		</mpp:ProcessCardSwipe>
	</soapenv:Body>
</soapenv:Envelope>
        """

        headers = {
        "Content-Type": "text/xml;charset=utf-8",
        "Content-Length":  str(len(soapRequest)),
        "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessCardSwipe"
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
