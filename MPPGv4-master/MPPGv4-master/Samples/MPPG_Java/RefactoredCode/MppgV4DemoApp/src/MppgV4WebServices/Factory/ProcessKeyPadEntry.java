package MppgV4WebServices.Factory;

import MppgV4WebServices.Pojos.ProcessKeyPadEntryRequest;
import MppgV4WebServices.Pojos.ProcessKeyPadEntryResponse;
import java.net.URL;
import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import javax.net.ssl.HttpsURLConnection;

/**
 * Handles requests for the ProcessKeyPadEntry operation from Mppgv4 web service
 * getting the Response .
 */
public class ProcessKeyPadEntry {

    public ProcessKeyPadEntryResponse CallWebService(ProcessKeyPadEntryRequest request, String webServiceUrl, String certificateFileName, String certificatePassword) throws Exception {
        ProcessKeyPadEntryResponse processKeyPadEntryReponse = new ProcessKeyPadEntryResponse();
        try {
            URL url = new URL(webServiceUrl);
            HttpsURLConnection con = (HttpsURLConnection) url.openConnection();
            String Soapbody = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:mpp=\"http://www.magensa.net/MPPGv4/\" xmlns:mpp1=\"http://schemas.datacontract.org/2004/07/MPPGv4WS.Core\" xmlns:sys=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">"
                    + "	<soapenv:Header/>"
                    + "	<soapenv:Body>"
                    + "		<mpp:ProcessKeyPadEntry>"
                    + "			<mpp:ProcessKeyPadEntryRequests>"
                    + "				<mpp1:ProcessKeyPadEntryRequest>"
                    + "					<mpp1:AdditionalRequestData>"
                    + "						<sys:KeyValuePairOfstringstring>"
                    + "							<sys:key></sys:key>"
                    + "							<sys:value></sys:value>"
                    + "						</sys:KeyValuePairOfstringstring>"
                    + "					</mpp1:AdditionalRequestData>"
                    + "					<mpp1:Authentication>"
                    + "					  <mpp1:CustomerCode>" + request.getCustomerCode() + "</mpp1:CustomerCode>"
                    + "						<mpp1:Password>" + request.getPassword() + "</mpp1:Password>"
                    + "						<mpp1:Username>" + request.getUserName() + "</mpp1:Username>"
                    + "					</mpp1:Authentication>"
                    + "					<mpp1:CardSwipeInput>"
                    + "						<mpp1:EncryptedCardSwipe>"
                    + "							<mpp1:DeviceSN>" + request.getDeviceSN() + "</mpp1:DeviceSN>"
                    + "							<mpp1:KSN>" + request.getKSN() + "</mpp1:KSN>"
                    + "							<mpp1:MagnePrint>" + request.getMagnePrint() + "</mpp1:MagnePrint>"
                    + "							<mpp1:MagnePrintStatus>" + request.getMagnePrintStatus() + "</mpp1:MagnePrintStatus>"
                    + "							<mpp1:Track1>" + request.getTrack1() + "</mpp1:Track1>"
                    + "							<mpp1:Track2>" + request.getTrack2() + "</mpp1:Track2>"
                    + "							<mpp1:Track3/>"
                    + "						</mpp1:EncryptedCardSwipe>"
                    + "					</mpp1:CardSwipeInput>"
                    + "					<mpp1:CustomerTransactionID>" + request.getCustomerTransactionID() + "</mpp1:CustomerTransactionID>"
                    + "					<mpp1:TransactionInput>"
                    + "						<mpp1:Amount>" + request.getAmount() + "</mpp1:Amount>"
                    + "						<mpp1:ClientCertAsBase64String></mpp1:ClientCertAsBase64String>"
                    + "						<mpp1:ClientCertPassword></mpp1:ClientCertPassword>"
                    + "						<mpp1:ProcessorName>" + request.getProcessorName() + "</mpp1:ProcessorName>"
                    + "						<mpp1:TransactionInputDetails>"
                    + "							<sys:KeyValuePairOfstringstring>"
                    + "								<sys:key/>"
                    + "								<sys:value/>"
                    + "							</sys:KeyValuePairOfstringstring>"
                    + "						</mpp1:TransactionInputDetails>"
                    + "						<mpp1:TransactionType>" + request.getTransactionType() + "</mpp1:TransactionType>"
                    + "					</mpp1:TransactionInput>"
                    + "				</mpp1:ProcessKeyPadEntryRequest>"
                    + "			</mpp:ProcessKeyPadEntryRequests>"
                    + "		</mpp:ProcessKeyPadEntry>"
                    + "	</soapenv:Body>"
                    + "</soapenv:Envelope>";
            /* Calling the secure post Request by passing required perametres to get the response*/
            con.setRequestMethod("POST");
            con.setRequestProperty("SOAPAction", "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessKeyPadEntry");
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
            processKeyPadEntryReponse.setStatusCode(con.getResponseCode());
            processKeyPadEntryReponse.setRawResponse(response.toString());

        } catch (IOException ex) {
            processKeyPadEntryReponse.setRawResponse(ex.getMessage());
        }
        return processKeyPadEntryReponse;

    }
}
