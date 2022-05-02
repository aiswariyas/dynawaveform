const fs = require("fs");
const https = require("https");
const axios = require("axios");

//class for processcardswipe operation
class ProcessCardSwipe {
    constructor(customerCode, userName, passWord, deviceSn,
        ksn, magnePrint, magnePrintStatus,
        track1, track2, track3, customerTransactionId, amount, processorName,
        transactionType
    ) {
        this.customerCode = customerCode;
        this.userName = userName;
        this.passWord = passWord;
        this.deviceSn = deviceSn;
        this.ksn = ksn;
        this.magnePrint = magnePrint;
        this.magnePrintStatus = magnePrintStatus;
        this.track1 = track1;
        this.track2 = track2;
        this.track3 = track3;
        this.customerTransactionId = customerTransactionId;
        this.amount = amount;
        this.processorName = processorName;
        this.transactionType = transactionType;
    }



    async CallWebService(webServiceUrl, certificateFilePath = null, certificatePassword = null) {
        try {
            let xml = `<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:mpp="http://www.magensa.net/MPPGv4/" xmlns:mpp1="http://schemas.datacontract.org/2004/07/MPPGv4WS.Core" xmlns:sys="http://schemas.datacontract.org/2004/07/System.Collections.Generic">
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
            <mpp1:CustomerCode>${this.customerCode}</mpp1:CustomerCode>
            <mpp1:Password>${this.passWord}</mpp1:Password>
            <mpp1:Username>${this.userName}</mpp1:Username>
          </mpp1:Authentication>
          <mpp1:CardSwipeInput>
            <mpp1:EncryptedCardSwipe>
              <mpp1:DeviceSN>${this.deviceSn}</mpp1:DeviceSN>
              <mpp1:KSN>${this.ksn}</mpp1:KSN>
              <mpp1:MagnePrint>${this.magnePrint}</mpp1:MagnePrint>
              <mpp1:MagnePrintStatus>${this.magnePrintStatus}</mpp1:MagnePrintStatus>
              <mpp1:Track1>${this.track1}</mpp1:Track1>
              <mpp1:Track2>${this.track2}</mpp1:Track2>
              <mpp1:Track3>${this.track3}</mpp1:Track3>
            </mpp1:EncryptedCardSwipe>
          </mpp1:CardSwipeInput>
          <mpp1:CustomerTransactionID>${this.customerTransactionId}</mpp1:CustomerTransactionID>
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
        </mpp1:ProcessCardSwipeRequest>
      </mpp:ProcessCardSwipeRequests>
    </mpp:ProcessCardSwipe>
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
                    "SOAPAction": "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessCardSwipe"
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
module.exports = ProcessCardSwipe;
