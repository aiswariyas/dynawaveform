package MppgV4WebServices.Factory;

import MppgV4WebServices.Pojos.ProcessCardSwipeRequest;
import MppgV4WebServices.Pojos.ProcessCardSwipeResponse;
import MppgV4WebServices.Pojos.ProcessDataRequest;
import MppgV4WebServices.Pojos.ProcessDataResponse;
import MppgV4WebServices.Pojos.ProcessKeyPadEntryRequest;
import MppgV4WebServices.Pojos.ProcessKeyPadEntryResponse;
import MppgV4WebServices.Pojos.ProcessManualEntryRequest;
import MppgV4WebServices.Pojos.ProcessManualEntryResponse;
import MppgV4WebServices.Pojos.ProcessReferenceIDRequest;
import MppgV4WebServices.Pojos.ProcessReferenceIDResponse;
import MppgV4WebServices.Pojos.ProcessTokenRequest;
import MppgV4WebServices.Pojos.ProcessTokenResponse;
import MppgV4WebServices.Interfaces.IMagtekMppgV4WebServicesFactory;
import MppgV4WebServices.Interfaces.IMagtekMppgV4WebServicesRequest;
import MppgV4WebServices.Interfaces.IMagtekMppgV4WebServicesResponse;

public class MagtekMppgV4WebServicesFactory implements IMagtekMppgV4WebServicesFactory {

    @Override
    public IMagtekMppgV4WebServicesResponse request(IMagtekMppgV4WebServicesRequest request, String webServiceUrl, String certificateFileName, String certificatePassword) throws Exception {
        IMagtekMppgV4WebServicesResponse response = null;

        if (request instanceof ProcessCardSwipeRequest) {
            ProcessCardSwipe processCardSwipe = new ProcessCardSwipe();
            ProcessCardSwipeRequest processCardSwipeRequest = (ProcessCardSwipeRequest) request;
            ProcessCardSwipeResponse processCardSwipeSwipeResponse = processCardSwipe.CallWebService(processCardSwipeRequest, webServiceUrl, certificateFileName, certificatePassword);
            response = (IMagtekMppgV4WebServicesResponse) processCardSwipeSwipeResponse;
        } else if (request instanceof ProcessDataRequest) {
            ProcessData processData = new ProcessData();
            ProcessDataRequest processDataRequest = (ProcessDataRequest) request;
            ProcessDataResponse processDataResponse = processData.CallWebService(processDataRequest, webServiceUrl, certificateFileName, certificatePassword);
            response = (IMagtekMppgV4WebServicesResponse) processDataResponse;
        } else if (request instanceof ProcessManualEntryRequest) {
            ProcessManualEntry processManualEntry = new ProcessManualEntry();
            ProcessManualEntryRequest processManualEntryRequest = (ProcessManualEntryRequest) request;
            ProcessManualEntryResponse processManualEntryResponse = processManualEntry.CallWebService(processManualEntryRequest, webServiceUrl, certificateFileName, certificatePassword);
            response = (IMagtekMppgV4WebServicesResponse) processManualEntryResponse;
        } else if (request instanceof ProcessKeyPadEntryRequest) {
            ProcessKeyPadEntry processKeyPadEntry = new ProcessKeyPadEntry();
            ProcessKeyPadEntryRequest processKeyPadEntryRequest = (ProcessKeyPadEntryRequest) request;
            ProcessKeyPadEntryResponse processKeyPadEntryResponse = processKeyPadEntry.CallWebService(processKeyPadEntryRequest, webServiceUrl, certificateFileName, certificatePassword);
            response = (IMagtekMppgV4WebServicesResponse) processKeyPadEntryResponse;
        } else if (request instanceof ProcessReferenceIDRequest) {
            ProcessReferenceID processReferenceID = new ProcessReferenceID();
            ProcessReferenceIDRequest processReferenceIDRequest = (ProcessReferenceIDRequest) request;
            ProcessReferenceIDResponse processReferenceIDResponse = processReferenceID.CallWebService(processReferenceIDRequest, webServiceUrl, certificateFileName, certificatePassword);
            response = (IMagtekMppgV4WebServicesResponse) processReferenceIDResponse;
        } else if (request instanceof ProcessTokenRequest) {
            ProcessToken processToken = new ProcessToken();
            ProcessTokenRequest processTokenRequest = (ProcessTokenRequest) request;
            ProcessTokenResponse processTokenResponse = processToken.CallWebService(processTokenRequest, webServiceUrl, certificateFileName, certificatePassword);
            response = (IMagtekMppgV4WebServicesResponse) processTokenResponse;
        }
        return response;
    }
}
