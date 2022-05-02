package MppgV4WebServices.Factory;

import MppgV4WebServices.Pojos.ProcessReferenceIDRequest;
import MppgV4WebServices.Pojos.ProcessReferenceIDResponse;
import java.net.URL;
import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.lang.reflect.Field;
import java.nio.charset.StandardCharsets;
import javax.net.ssl.HttpsURLConnection;

/**
 * Handles requests for the ProcessReferenceID operation from Mppgv4 web service
 * getting the Response .
 */
public class ProcessReferenceID {

    public ProcessReferenceIDResponse CallWebService(ProcessReferenceIDRequest request, String webServiceUrl, String certificateFileName, String certificatePassword) throws Exception {
        ProcessReferenceIDResponse processReferenceIDReponse = new ProcessReferenceIDResponse();
        try {
            URL url = new URL(webServiceUrl);
            HttpsURLConnection con = (HttpsURLConnection) url.openConnection();
            String Soapbody = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:mpp=\"http://www.magensa.net/MPPGv4/\" xmlns:mpp1=\"http://schemas.datacontract.org/2004/07/MPPGv4WS.Core\" xmlns:sys=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">"
                    + "   <soapenv:Header/>"
                    + "   <soapenv:Body>"
                    + "      <mpp:ProcessReferenceID>"
                    + "         <mpp:ProcessReferenceIDRequests>"
                    + "            <mpp1:ProcessReferenceIDRequest>"
                    + "               <mpp1:AdditionalRequestData>"
                    + "                  <sys:KeyValuePairOfstringstring>"
                    + "                     <sys:key></sys:key>"
                    + "                     <sys:value></sys:value>"
                    + "                  </sys:KeyValuePairOfstringstring>"
                    + "               </mpp1:AdditionalRequestData>"
                    + "               <mpp1:Authentication>"
                    + "                  <mpp1:CustomerCode>" + request.getCustomerCode() + "</mpp1:CustomerCode>"
                    + "                  <mpp1:Password>" + request.getPassword() + "</mpp1:Password>"
                    + "                  <mpp1:Username>" + request.getUsername() + "</mpp1:Username>"
                    + "               </mpp1:Authentication>"
                    + "               <mpp1:CustomerTransactionID>" + request.getCustomerTransactionID() + "</mpp1:CustomerTransactionID>"
                    + "               <mpp1:TransactionInput>"
                    + "                  <mpp1:Amount>" + request.getAmount() + "</mpp1:Amount>"
                    + "                  <mpp1:ProcessorName>" + request.getProcessorName() + "</mpp1:ProcessorName>"
                    + "                  <mpp1:TransactionInputDetails>"
                    + "                  <sys:KeyValuePairOfstringstring>"
                    + "                     <sys:key>" + request.getTransactionInputDetails_Key() + "</sys:key>"
                    + "                     <sys:value>" + request.getTransactionInputDetails_Value() + "</sys:value>"
                    + "                  </sys:KeyValuePairOfstringstring>"
                    + "                  </mpp1:TransactionInputDetails>"
                    + "                  <mpp1:TransactionType>" + request.getTransactionType() + "</mpp1:TransactionType>"
                    + "               <mpp1:ReferenceAuthCode>" + request.getReferenceAuthCode() + "</mpp1:ReferenceAuthCode>"
                    + "               <mpp1:ReferenceTransactionID>" + request.getReferenceTransactionID() + "</mpp1:ReferenceTransactionID>"
                    + "               </mpp1:TransactionInput>"
                    + "                   </mpp1:ProcessReferenceIDRequest>"
                    + "          </mpp:ProcessReferenceIDRequests>"
                    + "      </mpp:ProcessReferenceID>"
                    + "   </soapenv:Body>"
                    + "</soapenv:Envelope>";

            /* Calling the secure post Request by passing required perametres to get the response*/
            con.setRequestMethod("POST");
            con.setRequestProperty("SOAPAction", "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessReferenceID");
            con.setRequestProperty("Content-Type", "text/xml; charset=utf-8");
            if (!"".equals(certificateFileName.trim())) {
                con.setSSLSocketFactory(SSLUtils.getSSLSocketFactory(new File(certificateFileName), certificatePassword));
            }
            con.setDoOutput(true);

            try ( DataOutputStream wr = new DataOutputStream(con.getOutputStream())) {
                wr.writeBytes(Soapbody);
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
            processReferenceIDReponse.setStatusCode(con.getResponseCode());
            processReferenceIDReponse.setRawResponse(response.toString());

        } catch (IOException ex) {
            processReferenceIDReponse.setRawResponse(ex.getMessage());
        }
        return processReferenceIDReponse;

    }
}
