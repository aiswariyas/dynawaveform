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
public class ProcessManualEntryRequest implements IMagtekMppgV4WebServicesRequest{

     private String customerCode ;
     private String password;
     private String userName;
     private String customerTransactionID;
     private String addressLine1;
     private String addressLine2;
     private String cVV;
     private String city;
     private String country;
     private String expirationDate;
     private String nameOnCard;
     public String pAN;
     public String state;
     private String amount;
     private String processorName;
     private String transactionType; 
     public String getCustomerCode() {
        return customerCode;
    }

    public void setCustomerCode(String CustomerCode) {
        this.customerCode = CustomerCode;
    }
   
     /**
     * @return the cVV
     */
    public String getCVV() {
        return cVV;
    }
    /**
     * @param CVV the cVV to set
     */
    public void setCVV(String CVV) {
        this.cVV = CVV;
    }
   
    
    public String getPassword() {
        return password;
    }

    public void setPassword(String Password) {
        this.password = Password;
    }
   
    public String getUserName() {
        return userName;
    }

    public void setUserName(String UserName) {
        this.userName = UserName;
    }
       
    public String getCustomerTransactionID() {
        return customerTransactionID;
    }

    public void setCustomerTransactionID(String CustomerTransactionID) {
        this.customerTransactionID = CustomerTransactionID;
    }
    
    public String getCity() {
        return city;
    }
    
    public void setCity(String City) {
        this.city = City;
    }
   
    public String getCountry() {
        return country;
    }
   
    public void setCountry(String Country) {
        this.country = Country;
    }
    
    public String getExpirationDate() {
        return expirationDate;
    }
   
    public void setExpirationDate(String ExpirationDate) {
        this.expirationDate = ExpirationDate;
    }
    
    public String getAddressLine1() {
        return addressLine1;
    }
  
    public void setAddressLine1(String AddressLine1) {
        this.addressLine1 = AddressLine1;
    }
  
    public String getAddressLine2() {
        return addressLine2;
    }

    public void setAddressLine2(String AddressLine2) {
        this.addressLine2 = AddressLine2;
    }
 
    public String getNameOnCard() {
        return nameOnCard;
    }

    public void setNameOnCard(String NameOnCard) {
        this.nameOnCard = NameOnCard;
    }
          
      
     public String getPAN() {
        return pAN;
    }

    public void setPAN(String PAN) {
        this.pAN = PAN;
    }
     
     public String getState() {
        return state;
    }

    public void setState(String State) {
        this.state = State;
    }
       
     public String getAmount() {
        return amount;
    }

    public void setAmount(String Amount) {
        this.amount = Amount;
    }
              
        
    public String getProcessorName() {
        return processorName;
    }

    public void setProcessorName(String ProcessorName) {
        this.processorName = ProcessorName;
    }
       
    public String getTransactionType() {
        return transactionType;
    }

    public void setTransactionType(String TransactionType) {
        this.transactionType = TransactionType;
    }
    
    
}
