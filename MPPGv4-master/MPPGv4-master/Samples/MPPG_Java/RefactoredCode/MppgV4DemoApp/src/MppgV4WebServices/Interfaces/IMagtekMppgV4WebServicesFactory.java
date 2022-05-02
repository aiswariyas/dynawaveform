package MppgV4WebServices.Interfaces;

public interface IMagtekMppgV4WebServicesFactory {

    public IMagtekMppgV4WebServicesResponse request(IMagtekMppgV4WebServicesRequest Request, String webServiceUrl, String certificateFileName, String certificatePassword) throws Exception;
}
