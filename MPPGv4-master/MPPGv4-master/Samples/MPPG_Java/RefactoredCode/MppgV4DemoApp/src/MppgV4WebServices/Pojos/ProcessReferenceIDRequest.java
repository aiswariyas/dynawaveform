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
public class ProcessReferenceIDRequest implements IMagtekMppgV4WebServicesRequest {

     /**
     * @return the referenceTransactionID
     */
    public String getReferenceTransactionID() {
        return referenceTransactionID;
    }

    /**
     * @param ReferenceTransactionID the referenceTransactionID to set
     */
    public void setReferenceTransactionID(String ReferenceTransactionID) {
        this.referenceTransactionID =ReferenceTransactionID;
    }

    /**
     * @return the referenceAuthCode
     */
    public String getReferenceAuthCode() {
        return referenceAuthCode;
    }

    /**
     * @param ReferenceAuthCode the referenceAuthCode to set
     */
    public void setReferenceAuthCode(String ReferenceAuthCode) {
        this.referenceAuthCode = ReferenceAuthCode;
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
     * @return the transactionInputDetails_Key
     */
    public String getTransactionInputDetails_Key() {
        return transactionInputDetails_Key;
    }

    /**
     * 
     
     * @param TransactionInputDetails_Key the transactionInputDetails_Key to set
     */
    public void settransactionInputDetails_Key(String TransactionInputDetails_Key) {
        this.transactionInputDetails_Key = TransactionInputDetails_Key;
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
    public String getAmount() {
        return amount;
    }

    /**
     * @param amount the amount to set
     */
    public void setAmount(String amount) {
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

    

   
        private String customerCode ;
        private String password ;
        private String username ;
        private String customerTransactionID ;
        private String amount;
        private String processorName;
        private String transactionInputDetails_Key ;
         private String transactionInputDetails_Value ;
        private String transactionType;
        private String referenceAuthCode ;
        private String  referenceTransactionID ;
       
    
    

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

    /**
     * @return the transactionInputDetails_Value
     */
    public String getTransactionInputDetails_Value() {
        return transactionInputDetails_Value;
    }

    /**
     * @param TransactionInputDetails_Value the transactionInputDetails_Value to set
     */
    public void setTransactionInputDetails_Value(String TransactionInputDetails_Value) {
        this.transactionInputDetails_Value = TransactionInputDetails_Value;
    }
}
