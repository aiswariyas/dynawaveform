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
public class ProcessKeyPadEntryRequest implements IMagtekMppgV4WebServicesRequest{
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
    
   private String magnePrint;

    public String getMagnePrint() {
        return magnePrint;
    }

    public void setMagnePrint(String MagnePrint) {
        this.magnePrint = MagnePrint;
    }

    private String magnePrintStatus;

    public String getMagnePrintStatus() {
        return magnePrintStatus;
    }

    public void setMagnePrintStatus(String MagnePrintStatus) {
        this.magnePrintStatus = MagnePrintStatus;
    }

    private String track1;

    public String getTrack1() {
        return track1;
    }

    public void setTrack1(String Track1) {
        this.track1 = Track1;
    }

    private String track2;

    public String getTrack2() {
        return track2;
    }

    public void setTrack2(String Track2) {
        this.track2 = Track2;
    }

    private String track3;

    public String getTrack3() {
        return track3;
    }

        
    public void setTrack3(String Track3) {
        this.track3 = Track3;
    }
    private String deviceSN;
               public String getDeviceSN() {
        return deviceSN;
    } 
          public void setDeviceSN(String DeviceSN) {
        this.deviceSN = DeviceSN;
    }
    private String kSN;
         public String getKSN() {
        return kSN;
    } 
          public void setKSN(String KSN) {
        this.kSN = KSN;
    }
         
        /// <summary>
        /// CardSwipeInput Section
        /// </summary>
    
        public String cVV;
        public String getCVV() {
        return cVV;
    }

    public void setCVV(String CVV) {
        this.cVV = CVV;
    }
     public String zIP;
     public String getZIP() {
        return zIP;
    }

    public void setZIP(String ZIP) {
        this.zIP = ZIP;
    }
        
     
        /// <summary>
        /// Transaction Input section
        /// </summary>
    private String amount;
    
     public String getAmount() {
        return amount;
    }

    public void setAmount(String Amount) {
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
