const fs = require("fs");
const https = require("https");
const axios = require("axios");

class ProcessToken {
    constructor(customerCode, userName, passWord, customerTransactionId, additionalRequestData_key,
        additionalRequestData_value, token, amount, processorName, transactionType
    ) {
        this.customerTransactionId = customerTransactionId;
        this.customerCode = customerCode;
        this.userName = userName;
        this.passWord = passWord;
        this.additionalRequestData_key = additionalRequestData_key;
        this.additionalRequestData_value = additionalRequestData_value;
        this.token = token;
        this.amount = amount;
        this.processorName = processorName;
        this.transactionType = transactionType;
    }
    async CallWebService(webServiceUrl, certificateFilePath = null, certificatePassword = null) {
        try {
            let xml = `<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
<soapenv:Header/>
<soapenv:Body>
 <mpp:ProcessToken>        
  <mpp:ProcessTokenRequests>            
   <mpp1:ProcessTokenRequest>              
    <mpp1:AdditionalRequestData>                  
     <sys:KeyValuePairOfstringstring>
      <sys:key>${this.additionalRequestData_key}</sys:key>
      <sys:value Encoding="cdata">
      ${this.additionalRequestData_value}
      </sys:value>
     </sys:KeyValuePairOfstringstring>
    </mpp1:AdditionalRequestData>
    <mpp1:Authentication>
     <mpp1:CustomerCode>${this.customerCode}</mpp1:CustomerCode>
     <mpp1:Password>${this.passWord}</mpp1:Password>
     <mpp1:Username>${this.userName}</mpp1:Username>
    </mpp1:Authentication>
    <mpp1:CustomerTransactionID>${this.customerTransactionId}</mpp1:CustomerTransactionID>
    <mpp1:Token>${this.token}</mpp1:Token>
    <mpp1:TransactionInput>
     <mpp1:Amount>${this.amount}</mpp1:Amount>                  
     <mpp1:ProcessorName>${this.processorName}</mpp1:ProcessorName>                  
     <mpp1:TransactionInputDetails>                    
      <sys:KeyValuePairOfstringstring>
       <sys:key/>
       <sys:value/>
      </sys:KeyValuePairOfstringstring>
     </mpp1:TransactionInputDetails>
     <mpp1:TransactionType>${this.transactionType}</mpp1:TransactionType>
    </mpp1:TransactionInput>
   </mpp1:ProcessTokenRequest>
  </mpp:ProcessTokenRequests>
 </mpp:ProcessToken>
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
                    "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessToken"
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
module.exports = ProcessToken;