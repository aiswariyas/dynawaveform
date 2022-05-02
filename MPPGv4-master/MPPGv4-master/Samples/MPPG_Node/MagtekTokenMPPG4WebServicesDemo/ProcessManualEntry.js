const fs = require("fs");
const https = require("https");
const axios = require("axios");

class ProcessManualEntry {
    constructor(customerCode, userName, passWord, customerTransactionId, addressLine1,
        addressLine2, city, country, expirationDate, nameOnCard, pan,
        state, amount, processorName,
        transactionType
    ) {
        this.customerCode = customerCode;
        this.userName = userName;
        this.passWord = passWord;
        this.customerTransactionId = customerTransactionId;
        this.addressLine1 = addressLine1;
        this.addressLine2 = addressLine2;
        this.city = city;
        this.country = country;
        this.expirationDate = expirationDate;
        this.nameOnCard = nameOnCard;
        this.pan = pan;
        this.state = state;
        this.amount = amount;
        this.processorName = processorName;
        this.transactionType = transactionType;
    }

    async CallWebService(webServiceUrl, certificateFilePath = null, certificatePassword = null) {

        try {

            let xml = `<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
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
            <mpp1:CustomerCode>${this.customerCode}</mpp1:CustomerCode>
            <mpp1:Password>${this.passWord}</mpp1:Password>
            <mpp1:Username>${this.userName}</mpp1:Username>
          </mpp1:Authentication>
          <mpp1:CustomerTransactionID>${this.customerTransactionId}</mpp1:CustomerTransactionID>
          <mpp1:ManualEntryInput>
            <mpp1:AddressLine1>${this.addressLine1}</mpp1:AddressLine1>
            <mpp1:AddressLine2>${this.addressLine2}</mpp1:AddressLine2>
            <mpp1:City>${this.city}</mpp1:City>
            <mpp1:Country>${this.country}</mpp1:Country>
            <mpp1:ExpirationDate>${this.expirationDate}</mpp1:ExpirationDate>
            <mpp1:NameOnCard>${this.nameOnCard}</mpp1:NameOnCard>
            <mpp1:PAN>${this.pan}</mpp1:PAN>
            <mpp1:State>${this.state}</mpp1:State>
          </mpp1:ManualEntryInput>
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
        </mpp1:ProcessManualEntryRequest>
      </mpp:ProcessManualEntryRequests>
    </mpp:ProcessManualEntry>
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
                    "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessManualEntry"
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
module.exports = ProcessManualEntry;
