package MppgV4WebServices.Factory;

import MppgV4WebServices.Pojos.ProcessDataRequest;
import MppgV4WebServices.Pojos.ProcessDataResponse;
import java.net.URL;
import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import javax.net.ssl.HttpsURLConnection;

/**
 * Handles requests for the ProcessData operation from Mppgv4 web service
 * getting the Response .
 */
public class ProcessData {

    public ProcessDataResponse CallWebService(ProcessDataRequest request, String webServiceUrl, String certificateFileName, String certificatePassword) throws Exception {
        ProcessDataResponse processDataReponse = new ProcessDataResponse();
        try {
            URL url = new URL(webServiceUrl);
            HttpsURLConnection con = (HttpsURLConnection) url.openConnection();
            String soapBody = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:mpp=\"http://www.magensa.net/MPPGv4/\" xmlns:mpp1=\"http://schemas.datacontract.org/2004/07/MPPGv4WS.Core\" xmlns:sys=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">"
                    + "   <soapenv:Header/>"
                    + "   <soapenv:Body>"
                    + "      <mpp:ProcessData>"
                    + "         <mpp:ProcessDataRequests>"
                    + "            <mpp1:ProcessDataRequest>"
                    + "               <mpp1:AdditionalRequestData>"
                    + "                  <sys:KeyValuePairOfstringstring>"
                    + "                     <sys:key/>"
                    + "                     <sys:value/>"
                    + "                  </sys:KeyValuePairOfstringstring>"
                    + "               </mpp1:AdditionalRequestData>"
                    + "               <mpp1:Authentication>"
                    + "                  <mpp1:CustomerCode>" + request.getCustomerCode() + "</mpp1:CustomerCode>"
                    + "                  <mpp1:Password>" + request.getPassword() + "</mpp1:Password>"
                    + "                  <mpp1:Username>" + request.getUserName() + "</mpp1:Username>"
                    + "               </mpp1:Authentication>"
                    + "               <mpp1:CustomerTransactionID>" + request.getCustomerTransactionID() + "</mpp1:CustomerTransactionID>"
                    + "               <mpp1:DataInput>"
                    + "                  <mpp1:Data>" + request.getData() + "</mpp1:Data>"
                    + "                  <mpp1:DataFormatType>" + request.getDataFormatType() + "</mpp1:DataFormatType>"
                    + "                  <mpp1:EncryptionInfo>"
                    + "                     <mpp1:EncryptionType>" + request.getEncryptionType() + "</mpp1:EncryptionType>"
                    + "                     <mpp1:KSN>" + request.getKSN() + "</mpp1:KSN>"
                    + "                     <mpp1:NumberOfPaddedBytes>" + request.getNumberOfPaddedBytes() + "</mpp1:NumberOfPaddedBytes>"
                    + "                  </mpp1:EncryptionInfo>"
                    + "                  <mpp1:IsEncrypted>" + request.getIsEncrypted() + "</mpp1:IsEncrypted>"
                    + "                  <mpp1:PaymentMode>" + request.getPaymentMode() + "</mpp1:PaymentMode>"
                    + "               </mpp1:DataInput>"
                    + "               <mpp1:TransactionInput>"
                    + "                  <mpp1:Amount>" + request.getAmount() + "</mpp1:Amount>"
                    + "                  <mpp1:ProcessorName>" + request.getProcessorName() + "</mpp1:ProcessorName>"
                    + "                  <mpp1:TransactionInputDetails>"
                    + "                     <sys:KeyValuePairOfstringstring>"
                    + "                        <sys:key/>"
                    + "                        <sys:value/>"
                    + "                     </sys:KeyValuePairOfstringstring>"
                    + "                  </mpp1:TransactionInputDetails>"
                    + "                  <mpp1:TransactionType>" + request.getTransactionType() + "</mpp1:TransactionType>"
                    + "               </mpp1:TransactionInput>"
                    + "            </mpp1:ProcessDataRequest>"
                    + "         </mpp:ProcessDataRequests>"
                    + "      </mpp:ProcessData>"
                    + "   </soapenv:Body>"
                    + "</soapenv:Envelope>";
            /* Calling the secure post Request by passing required perametres to get the response*/
            con.setRequestMethod("POST");
            con.setRequestProperty("SOAPAction", "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessData");
            con.setRequestProperty("Content-Type", "text/xml; charset=utf-8");
            if (!"".equals(certificateFileName.trim())) {
                con.setSSLSocketFactory(SSLUtils.getSSLSocketFactory(new File(certificateFileName), certificatePassword));
            }
            con.setDoOutput(true);
            try ( DataOutputStream wr = new DataOutputStream(con.getOutputStream())) {
                wr.writeBytes(soapBody);
                wr.flush();
            }
            InputStream inputstream = con.getInputStream();
            StringBuilder response = new StringBuilder();
            try ( BufferedReader inputBuffer = new BufferedReader(new InputStreamReader(inputstream))) {
                String inputLine;
                while ((inputLine = inputBuffer.readLine()) != null) {
                    response.append(inputLine);
                }
            }
            processDataReponse.setStatusCode(con.getResponseCode());
            processDataReponse.setRawResponse(response.toString());

        } catch (IOException ex) {
            processDataReponse.setRawResponse(ex.getMessage());
        }
        return processDataReponse;

    }
}
