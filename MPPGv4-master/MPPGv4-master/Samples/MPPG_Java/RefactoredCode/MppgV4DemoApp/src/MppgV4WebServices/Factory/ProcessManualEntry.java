package MppgV4WebServices.Factory;

import MppgV4WebServices.Pojos.ProcessManualEntryRequest;
import MppgV4WebServices.Pojos.ProcessManualEntryResponse;
import java.net.URL;
import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import javax.net.ssl.HttpsURLConnection;

/**
 * Handles requests for the ProcessManualEntry operation from Mppgv4 web service
 * getting the Response .
 */
public class ProcessManualEntry {

    public ProcessManualEntryResponse CallWebService(ProcessManualEntryRequest request, String webServiceUrl, String certificateFileName, String certificatePassword) throws Exception {
        ProcessManualEntryResponse processManualEntryReponse = new ProcessManualEntryResponse();
        try {
            /* Calling the secure post Request by passing required perametres to get the response*/
            URL url = new URL(webServiceUrl);
            HttpsURLConnection con = (HttpsURLConnection) url.openConnection();
            String Soapbody = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:mpp=\"http://www.magensa.net/MPPGv4/\" xmlns:mpp1=\"http://schemas.datacontract.org/2004/07/MPPGv4WS.Core\" xmlns:sys=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">"
                    + "	<soapenv:Header/>"
                    + "	<soapenv:Body>"
                    + "		<mpp:ProcessManualEntry>"
                    + "			<mpp:ProcessManualEntryRequests>"
                    + "				<mpp1:ProcessManualEntryRequest>"
                    + "					<mpp1:AdditionalRequestData>"
                    + "						<sys:KeyValuePairOfstringstring>"
                    + "							<sys:key/>"
                    + "							<sys:value/>"
                    + "						</sys:KeyValuePairOfstringstring>"
                    + "					</mpp1:AdditionalRequestData>"
                    + "					<mpp1:Authentication>"
                    + "						<mpp1:CustomerCode>" + request.getCustomerCode() + "</mpp1:CustomerCode>"
                    + "						<mpp1:Password>" + request.getPassword() + "</mpp1:Password>"
                    + "						<mpp1:Username>" + request.getUserName() + "</mpp1:Username>"
                    + "					</mpp1:Authentication>"
                    + "					<mpp1:CustomerTransactionID>" + request.getCustomerTransactionID() + "</mpp1:CustomerTransactionID>"
                    + "					<mpp1:ManualEntryInput>"
                    + "						<mpp1:AddressLine1>" + request.getAddressLine1() + "</mpp1:AddressLine1>"
                    + "						<mpp1:AddressLine2>" + request.getAddressLine2() + "</mpp1:AddressLine2>"
                    + "						<mpp1:City>" + request.getCity() + "</mpp1:City>"
                    + "						<mpp1:Country>" + request.getCountry() + "</mpp1:Country>"
                    + "						<mpp1:ExpirationDate>" + request.getExpirationDate() + "</mpp1:ExpirationDate>"
                    + "						<mpp1:NameOnCard>" + request.getNameOnCard() + "</mpp1:NameOnCard>"
                    + "						<mpp1:PAN>" + request.getPAN() + "</mpp1:PAN>"
                    + "						<mpp1:State>" + request.getState() + "</mpp1:State>"
                    + "					</mpp1:ManualEntryInput>"
                    + "					<mpp1:TransactionInput>"
                    + "						<mpp1:Amount>" + request.getAmount() + "</mpp1:Amount>"
                    + "						<mpp1:ProcessorName>" + request.getProcessorName() + "</mpp1:ProcessorName>"
                    + "						<mpp1:TransactionInputDetails>"
                    + "							<sys:KeyValuePairOfstringstring>"
                    + "								<sys:key/>"
                    + "								<sys:value/>"
                    + "							</sys:KeyValuePairOfstringstring>"
                    + "						</mpp1:TransactionInputDetails>"
                    + "						<mpp1:TransactionType>" + request.getTransactionType() + "</mpp1:TransactionType>"
                    + "					</mpp1:TransactionInput>"
                    + "				</mpp1:ProcessManualEntryRequest>"
                    + "			</mpp:ProcessManualEntryRequests>"
                    + "		</mpp:ProcessManualEntry>"
                    + "	</soapenv:Body>"
                    + "</soapenv:Envelope>";
            con.setRequestMethod("POST");
            con.setRequestProperty("SOAPAction", "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessManualEntry");
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
            processManualEntryReponse.setStatusCode(con.getResponseCode());
            processManualEntryReponse.setRawResponse(response.toString());

        } catch (IOException ex) {
            processManualEntryReponse.setRawResponse(ex.getMessage());
        }
        return processManualEntryReponse;

    }
}
