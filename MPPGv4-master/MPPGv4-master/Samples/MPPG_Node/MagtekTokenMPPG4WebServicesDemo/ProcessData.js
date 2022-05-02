const fs = require("fs");
const https = require("https");
const axios = require("axios");
class ProcessData {
    constructor(customercode, username, password, customerTransactionId, data,
        dataformattype, encryptiontype, ksn, numberofpaddedbytes,
        isencrypted, paymentmode, amount, processorname, transactionType
    ) {
        this.customercode = customercode;
        this.username = username;
        this.password = password;
        this.customerTransactionId = customerTransactionId;
        this.data = data;
        this.dataformattype = dataformattype;
        this.encryptiontype = encryptiontype;
        this.ksn = ksn;
        this.numberofpaddedbytes = numberofpaddedbytes;
        this.isencrypted = isencrypted;
        this.paymentmode = paymentmode;
        this.amount = amount;
        this.processorname = processorname;
        this.transactionType = transactionType;
    }
    async CallWebService(webServiceUrl, certificateFilePath = null, certificatePassword = null) {
        try {
            let xml = `<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
            <soapenv:Header />
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
                <mpp1:CustomerCode>${this.customercode}</mpp1:CustomerCode>
                    <mpp1:Password>${this.password}</mpp1:Password>
                        <mpp1:Username>${this.username}</mpp1:Username>
			</mpp1:Authentication>
            <mpp1:CustomerTransactionID>${this.customerTransactionId}</mpp1:CustomerTransactionID>
                <mpp1:DataInput>
                    <mpp1:Data>${this.data}</mpp1:Data>
                     <mpp1:DataFormatType>${this.dataformattype}</mpp1:DataFormatType>
                      <mpp1:EncryptionInfo>
                        <mpp1:EncryptionType>${this.encryptiontype}</mpp1:EncryptionType>
                        <mpp1:KSN>${this.ksn}</mpp1:KSN>
                        <mpp1:NumberOfPaddedBytes>${this.numberofpaddedbytes}</mpp1:NumberOfPaddedBytes>
                      </mpp1:EncryptionInfo>
                <mpp1:IsEncrypted>${this.isencrypted}</mpp1:IsEncrypted>
                <mpp1:PaymentMode>${this.paymentmode}</mpp1:PaymentMode>
</mpp1:DataInput>
            <mpp1:TransactionInput>
                <mpp1:Amount>${this.amount}</mpp1:Amount>
                    <mpp1:ProcessorName>${this.processorname}</mpp1:ProcessorName>
                        <mpp1:TransactionInputDetails>
                            <sys:KeyValuePairOfstringstring>
								<sys:key/>
								<sys:value/>
							</sys:KeyValuePairOfstringstring>
						</mpp1:TransactionInputDetails>
            <mpp1:TransactionType>${this.transactionType}</mpp1:TransactionType>
					</mpp1:TransactionInput>
				</mpp1:ProcessDataRequest>
			</mpp:ProcessDataRequests>
		</mpp:ProcessData>
	</soapenv:Body>
</soapenv:Envelope>`;
            let agent = null;

            if (certificateFilePath) {
                agent = new https.Agent({
                    rejectUnauthorized: false,
                    strictSSL: false,
                    pfx: fs.readFileSync(certificateFilePath),
                    passphrase: certificatePassword
                });
            }

            let response = await axios({
                url: webServiceUrl,
                method: "post",
                httpsAgent: agent,
                data: xml,
                headers: {
                    "Content-Type": "text/xml;charset=utf-8",
                    "Content-Length": xml.length,
                    "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessData"
                }
            });

            return { statuscode: response.status, data: response.data };
        }
        catch (error) {
            if (error.response) {
                //The request was made and the server responded with a
                //status code that falls out of the range of 2xx
                return { statuscode: error.response.status, data: error.response.data };
            } else {
                //Something happened in setting up the request and triggered an Error
                throw error;
            }
        }
    }
}
module.exports = ProcessData;