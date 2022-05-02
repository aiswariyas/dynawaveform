/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package MppgV4WebServices.Pojos;

import MppgV4WebServices.Interfaces.IMagtekMppgV4WebServicesResponse;

/**
 *
 * @author gnagidi
 */
public class ProcessDataResponse implements IMagtekMppgV4WebServicesResponse{
    private String rawResponse;

    public String getRawResponse() {
        return rawResponse;
    }

    public void setRawResponse(String rawResponse) {
        this.rawResponse = rawResponse;
    }

    private int statusCode;

    public int getStatusCode() {
        return statusCode;
    }

    public void setStatusCode(int statusCode) {
        this.statusCode = statusCode;
    }
}
