package MppgV4WebServices.Factory;

import MppgV4WebServices.Pojos.ProcessTokenRequest;
import MppgV4WebServices.Pojos.ProcessTokenResponse;
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
 * Handles requests for the ProcessToken operation from Mppgv4 web service
 * getting the Response .
 */
public class ProcessToken {

    public ProcessTokenResponse CallWebService(ProcessTokenRequest request, String webServiceUrl, String certificateFileName, String certificatePassword) throws Exception {
        ProcessTokenResponse processTokenReponse = new ProcessTokenResponse();
        try {

            String Soapbody = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:mpp=\"http://www.magensa.net/MPPGv4/\" xmlns:mpp1=\"http://schemas.datacontract.org/2004/07/MPPGv4WS.Core\" xmlns:sys=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">"
                    + "	<soapenv:Header/>"
                    + "	<soapenv:Body>"
                    + "		<mpp:ProcessToken>         "
                    + "			<mpp:ProcessTokenRequests>            "
                    + "				<mpp1:ProcessTokenRequest>               "
                    + "					<mpp1:AdditionalRequestData>                  "
                    + "						<sys:KeyValuePairOfstringstring>"
                    + "							<sys:key>PayloadResponse</sys:key>"
                    + "							<sys:value Encoding=\"cdata\">"
                    + "{ADDITIONALREQUESTDATA_VALUE}"
                    + "							</sys:value>"
                    + "						</sys:KeyValuePairOfstringstring>"
                    + "					</mpp1:AdditionalRequestData>"
                    + "					<mpp1:Authentication>"
                    + "						<mpp1:CustomerCode>" + request.getCustomerCode() + "</mpp1:CustomerCode>"
                    + "						<mpp1:Password>" + request.getPassword() + "</mpp1:Password>"
                    + "						<mpp1:Username>" + request.getUsername() + "</mpp1:Username>"
                    + "					</mpp1:Authentication>            "
                    + "					<mpp1:CustomerTransactionID>" + request.getCustomerTransactionID() + "</mpp1:CustomerTransactionID>"
                    + "					<mpp1:Token>" + request.getToken() + "</mpp1:Token>"
                    + "					<mpp1:TransactionInput>                  "
                    + "						<mpp1:Amount>" + request.getAmount() + "</mpp1:Amount>                  "
                    + "						<mpp1:ProcessorName>" + request.getProcessorName() + "</mpp1:ProcessorName>                  "
                    + "						<mpp1:TransactionInputDetails>                     "
                    + "							<sys:KeyValuePairOfstringstring>"
                    + "								<sys:key/>"
                    + "								<sys:value/>"
                    + "							</sys:KeyValuePairOfstringstring>"
                    + "						</mpp1:TransactionInputDetails>"
                    + "						<mpp1:TransactionType>" + request.getTransactionType() + "</mpp1:TransactionType>"
                    + "					</mpp1:TransactionInput>"
                    + "				</mpp1:ProcessTokenRequest>"
                    + "			</mpp:ProcessTokenRequests>"
                    + "		</mpp:ProcessToken>"
                    + "	</soapenv:Body>"
                    + "</soapenv:Envelope>";

            ///Developer comments - This below line Code is introduced as Payload xml response dynamic input is throwing prolog errors
            /// As payload has special char and long xml as normal text, so variable value replacing code is introduced.
            Soapbody = Soapbody.replace("{ADDITIONALREQUESTDATA_VALUE}", request.getAdditionalRequestData_Value());
            /* Calling the secure post Request by passing required perametres to get the response*/
            URL url = new URL(webServiceUrl);
            HttpsURLConnection con = (HttpsURLConnection) url.openConnection();
            con.setRequestMethod("POST");
            con.setRequestProperty("SOAPAction", "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessToken");
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
            processTokenReponse.setStatusCode(con.getResponseCode());
            processTokenReponse.setRawResponse(response.toString());

        } catch (IOException ex) {
            processTokenReponse.setRawResponse(ex.getMessage());
        }
        return processTokenReponse;

    }
}
