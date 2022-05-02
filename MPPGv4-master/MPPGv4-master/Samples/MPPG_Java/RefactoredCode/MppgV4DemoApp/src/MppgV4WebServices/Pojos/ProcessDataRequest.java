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
public class ProcessDataRequest implements IMagtekMppgV4WebServicesRequest{
    /*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
      
    private String customerCode ;
    public String getCustomerCode() {
        return customerCode;
    }

    public void setCustomerCode(String CustomerCode) {
        this.customerCode = CustomerCode;
    }
    private String password;

    public String getPassword() {
        return password;
    }

    public void setPassword(String Password) {
        this.password = Password;
    }
    private String userName;

    public String getUserName() {
        return userName;
    }

    public void setUserName(String UserName) {
        this.userName = UserName;
    }
       
       
        private String  customerTransactionID;
        public String getCustomerTransactionID() {
        return customerTransactionID;
    }

    public void setCustomerTransactionID(String CustomerTransactionID) {
        this.customerTransactionID = CustomerTransactionID;
    }
    
   private String data;

    public String getData() {
        return data;
    }

    public void setData(String Data) {
        this.data = Data;
    }

    private String dataFormatType;

    public String getDataFormatType() {
        return dataFormatType;
    }

    public void setDataFormatType(String DataFormatType) {
        this.dataFormatType = DataFormatType;
    }

    private String encryptionType;

    public String getEncryptionType() {
        return encryptionType;
    }

    public void setEncryptionType(String EncryptionType) {
        this.encryptionType = EncryptionType;
    }

    private String kSN;

    public String getKSN() {
        return kSN;
    }

    public void setKSN(String KSN) {
        this.kSN = KSN;
    }

    private String numberOfPaddedBytes;

    public String getNumberOfPaddedBytes() {
        return numberOfPaddedBytes;
    }

    public void setNumberOfPaddedBytes(String NumberOfPaddedBytes) {
        this.numberOfPaddedBytes = NumberOfPaddedBytes;
    }
        /// <summary>
        /// CardSwipeInput Section
        /// </summary>
    
        public String isEncrypted;
        public String getIsEncrypted() {
        return isEncrypted;
    }

    public void setIsEncrypted(String IsEncrypted) {
        this.isEncrypted = IsEncrypted;
    }
     public String paymentMode;
     public String getPaymentMode() {
        return paymentMode;
    }

    public void setPaymentMode(String PaymentMode) {
        this.paymentMode = PaymentMode;
    }
        
     
        /// <summary>
        /// Transaction Input section
        /// </summary>
    private String amount;
    
     public String getAmount() {
        return amount;
    }

    public void  setAmount(String Amount) {
        this.amount = Amount;
    }
              
        private String processorName;
        public String getProcessorName() {
        return processorName;
    }

    public void setProcessorName(String ProcessorName) {
        this.processorName = ProcessorName;
    }
        private String transactionType;
        public String getTransactionType() {
        return transactionType;
    }

    public void setTransactionType(String TransactionType) {
        this.transactionType = TransactionType;
    }
}


