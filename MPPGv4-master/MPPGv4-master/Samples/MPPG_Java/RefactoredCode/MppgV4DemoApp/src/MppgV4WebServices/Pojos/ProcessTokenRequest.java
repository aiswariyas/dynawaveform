/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package MppgV4WebServices.Pojos;

import MppgV4WebServices.Interfaces.IMagtekMppgV4WebServicesRequest;

/**
 *
 * @author gnagidi
 */
public class ProcessTokenRequest implements IMagtekMppgV4WebServicesRequest{

      /**
     * @return the token
     */
    public String getToken() {
        return token;
    }

    /**
     * @param Token the token to set
     */
    public void setToken(String Token) {
        this.token = Token;
    }
        /**
     * @return the transactionType
     */
    public String getTransactionType() {
        return transactionType;
    }

    /**
     * @param TransactionType the transactionType to set
     */
    public void setTransactionType(String TransactionType) {
        this.transactionType = TransactionType;
    }

    
    /**
     * @return the processorName
     */
    public String getProcessorName() {
        return processorName;
    }

    /**
     * @param processorName the processorName to set
     */
    public void setProcessorName(String processorName) {
        this.processorName = processorName;
    }

    /**
     * @return the amount
     */
    public double getAmount() {
        return amount;
    }

    /**
     * @param amount the amount to set
     */
    public void setAmount(double amount) {
        this.amount = amount;
    }

    /**
     * @return the customerTransactionID
     */
    public String getCustomerTransactionID() {
        return customerTransactionID;
    }

    /**
     * @param CustomerTransactionID the customerTransactionID to set
     */
    public void setCustomerTransactionID(String CustomerTransactionID) {
        this.customerTransactionID = CustomerTransactionID;
    }

    /**
     * @return the username
     */
    public String getUsername() {
        return username;
    }

    /**
     * @param Username the username to set
     */
    public void setUsername(String Username) {
        this.username = Username;
    }

    /**
     * @return the password
     */
    public String getPassword() {
        return password;
    }

    /**
     * @param Password the password to set
     */
    public void setPassword(String Password) {
        this.password = Password;
    }

    private String additionalRequestData_Key;
        
    /**
     * @return the additionalRequestData_Key
     */
    public String getAdditionalRequestData_Key() {
        return additionalRequestData_Key;
    }

    /**
     * @param AdditionalRequestData_Key the additionalRequestData_Key to set
     */
    public void setAdditionalRequestData_Key(String AdditionalRequestData_Key) {
        this.additionalRequestData_Key = AdditionalRequestData_Key;
    }
        
        private String additionalRequestData_Value;
        private String customerCode;
        private String password;
        private String username ;
        private String customerTransactionID ;
        private double amount;
        private String token;
        private String processorName;
      
        private String transactionType;
      
       
    /**
     * @return the additionalRequestData_Value
     */
    public String getAdditionalRequestData_Value() {
        return additionalRequestData_Value;
    }

    /**
     * @param AdditionalRequestData_Value the additionalRequestData_Value to set
     */
    public void setAdditionalRequestData_Value(String AdditionalRequestData_Value) {
        this.additionalRequestData_Value = AdditionalRequestData_Value;
    }

    /**
     * @return the customerCode
     */
    public String getCustomerCode() {
        return customerCode;
    }

    /**
     * @param CustomerCode the customerCode to set
     */
    public void setCustomerCode(String CustomerCode) {
        this.customerCode = CustomerCode;
    }
}
