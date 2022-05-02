#!/usr/bin/env python3.7
import json
from xml.dom.minidom import parseString
from Mppgv4Services import ProcessCardSwipe
from Mppgv4Services import ProcessData
from Mppgv4Services import ProcessKeyPadEntry
from Mppgv4Services import ProcessManualEntry
from Mppgv4Services import ProcessToken

#region HelperMethods
def read_struser_input(question,isOptional=True):
    answer = str(input(question))
    if((not isOptional) and (answer.strip() == "")):
        return read_struser_input(question,isOptional)
    return answer

def read_intuser_input(question):
    answer = str(input(question))
    try:
        int(answer)
    except ValueError:
        print("Invalid Input. Int Is Required.")
        return read_intuser_input(question)
    return answer

def read_floatuser_input(question):
    answer = str(input(question))
    try:
        float(answer)
    except ValueError:
        print("Invalid Input. Float Is Required.")
        return read_floatuser_input(question)
    return answer

def read_expirationdate_yearuser_input(question):
    answer = str(input(question))
    if((answer.strip() == "") or (len(answer) != 2) or (not answer.isnumeric())):
        print("Invalid Year.")
        return read_expirationdate_yearuser_input(question)
    return int(answer)

def read_expirationdate_monthuser_input(question):
    answer = str(input(question))
    if((answer.strip() == "") or (len(answer) != 2) or (not answer.isnumeric()) or (int(answer) not in range(1, 13))):
        print("Invalid Input.")
        return read_expirationdate_monthuser_input(question)
    return int(answer)


def read_expirationdateuser_input(question,yearlabel,monthlabel):
    '''ex: expiration date 2503 where 25 is Year and 03 is Month'''
    print(question)
    year = read_expirationdate_yearuser_input(yearlabel)
    month = read_expirationdate_monthuser_input(monthlabel)
    answer = "{0:0=2d}".format(year) + "{0:0=2d}".format(month)
    return answer


def read_transactiontype_input(question):
    transactiontypes = ["SALE", "AUTHORIZE", "CAPTURE", "VOID", "REFUND", "FORCE","REJECT","TOKEN"]
    answer = str(input(question))
    if(answer in transactiontypes):
        return answer
    else:
        print('Invalid TransactionType.')
        return read_transactiontype_input(question)

def read_paymentmode_input(question):
    paymentmodes = ["EMV","MagStripe"]
    answer = str(input(question))
    if(answer in paymentmodes):
        return answer
    else:
        print("Invalid Paymentmode.")
        return read_paymentmode_input(question)
    return int(answer)

def read_isecrypted_input(question):
    isencrypts = ["true", "false"]
    answer = str(input(question))
    if(answer in isencrypts):
        return answer
    else:
        print('Invalid EncryptType.')
        return read_isecrypted_input(question)

def read_dataformattype_input(question):
    dataformattypes = ["TLV"]
    answer = str(input(question))
    if(answer in dataformattypes):
        return answer
    else:
        print('Invalid DataFormatType.')
        return read_dataformattype_input(question)
#endregion

#region display operation specific UI
def processcardswipe_ui():
    try:
        config = json.loads(open('config.json').read())
        webServiceUrl = config["MppgV4ServiceUrl"]
        #Currently mppg operations don't require certificate
        #In future if certificate file needs to be supported.
        #pass the appropriate certificate filename and password
        certificateFileName = None
        certificatePassword = None
        print("=" * 10 + "Process Card Swipe Operation" + "=" * 10)
        customerCode = read_struser_input("Enter Customer Code:",False)
        userName = read_struser_input("Enter Username:",False)
        passWord = read_struser_input("Enter PassWord:",False)
        deviceSN = read_struser_input("Enter DeviceSN:",False)
        ksn = read_struser_input("Enter KSN:",False)
        magnePrint = read_struser_input("Enter MagnePrint:",False)
        magnePrintStatus = read_struser_input("Enter MagnePrintStatus:",False)
        track1 = read_struser_input("Enter track1:",False)
        track2 = read_struser_input("Enter track2:",False)
        customerTransactionID = read_struser_input("Enter CustomerTransactionID:",True)
        amount = read_floatuser_input("Enter Amount:") 
        processorName = read_struser_input("Enter processorName:",False)
        transactionType = read_transactiontype_input("Enter TransactionType:")

        req = ProcessCardSwipe.ProcessCardSwipeRequest(customerCode=customerCode,userName=userName,
                                                       passWord=passWord,deviceSN=deviceSN,ksn=ksn,
                                                       magnePrint=magnePrint,magnePrintStatus=magnePrintStatus,
                                                       track1=track1,track2=track2,
                                                       customerTransactionID=customerTransactionID,
                                                       amount=amount,processorName=processorName,
                                                       transactionType=transactionType)

        obj = ProcessCardSwipe.ProcessCardSwipe(req)
        response = obj.CallService(webServiceUrl,certificateFileName,certificatePassword)
        print('Response:-')
        print('StatusCode:' + str(response.status_code))  if (not response is None) else print('StatusCode:' + str(response))
        print('Content:\n' + parseString(response.content).toprettyxml())  if (not response is None) else print('Content:' + str(response))
    except Exception as ex:
        print(ex)

def processdataemvauth_ui():
     try:
        config = json.loads(open('config.json').read())
        webServiceUrl = config["MppgV4ServiceUrl"]
        #Currently mppg operations don't require certificate
        #In future if certificate file needs to be supported.
        #pass the appropriate certificate filename and password
        certificateFileName = None
        certificatePassword = None
        customerCode = read_struser_input("Enter Customer Code:",False)
        userName = read_struser_input("Enter Username:",False)
        passWord = read_struser_input("Enter PassWord:",False)
        customerTransactionId = read_struser_input("Enter CustomerTransactionId:",True)
        data = read_struser_input("Enter Data:",False)
        dataFormatType = read_dataformattype_input("Enter DataFormatType:")
        encryptionType = read_struser_input("Enter EncryptionType :",False)
        ksn = read_struser_input("Enter KSN:",False)
        numberOfPaddedBytes = read_intuser_input("Enter NumberOfPaddedBytes:")
        isEncrypted = read_isecrypted_input("Enter IsEncrypted:")
        paymentMode = read_paymentmode_input("Enter PaymentMode:")
        amount = read_floatuser_input("Enter Amount:")
        processorName = read_struser_input("Enter ProcessorName:",False)
        transactionType = read_transactiontype_input("Enter transactionType:")
        
        req = ProcessData.ProcessDataRequest(customerCode=customerCode,userName=userName,
                                            passWord=passWord,customerTransactionId=customerTransactionId,
                                            data=data,dataFormatType=dataFormatType,encryptionType=encryptionType,
                                            ksn=ksn,numberOfPaddedBytes=numberOfPaddedBytes,isEncrypted=isEncrypted,
                                            paymentMode=paymentMode,amount=amount,processorName = processorName,transactionType=transactionType)
        
        obj = ProcessData.ProcessData(req)
        response = obj.CallService(webServiceUrl,certificateFileName,certificatePassword)
        print('Response:-')
        print('StatusCode:' + str(response.status_code))  if (not response is None) else print('StatusCode:' + str(response))
        print('Content:\n' + parseString(response.content).toprettyxml())  if (not response is None) else print('Content:' + str(response))
     except Exception as ex:
        print(ex)

def processmanualentry_ui():
    try:
        config = json.loads(open('config.json').read())
        webServiceUrl = config["MppgV4ServiceUrl"]
        #Currently mppg operations don't require certificate
        #In future if certificate file needs to be supported.
        #pass the appropriate certificate filename and password
        certificateFileName = None
        certificatePassword = None
        customerCode = read_struser_input("Enter Customer Code:",False)
        userName = read_struser_input("Enter Username:",False)
        passWord = read_struser_input("Enter PassWord:",False)
        customerTransactionId = read_struser_input("Enter CustomerTransactionId:",True)
        addressline1 = read_struser_input("Enter AddressLine1:",True)
        addressline2 = read_struser_input("Enter AddressLine2:",True)
        city = read_struser_input("Enter City:",True)
        country = read_struser_input("Enter Country:",True)
        expirationDate = read_expirationdateuser_input("Enter Expiration Date:","Year(Ex:For Year 2025 Enter 25):","Month:(Ex:Int Between 1-12 )")
        nameOnCard = read_struser_input("Enter NameOnCard:",True)
        pan = read_struser_input("Enter PAN:",False)
        state = read_struser_input("Enter State:",True)
        amount = read_floatuser_input("Enter Amount:")
        processorName = read_struser_input("Enter ProcessorName:",False)
        transactionType = read_transactiontype_input("Enter TransactionType:")
        req = ProcessManualEntry.ProcessManualEntryRequest(customerCode=customerCode,userName=userName,
                                            passWord=passWord,customerTransactionId=customerTransactionId,
                                            addressline1=addressline1,addressline2=addressline2,
                                            city=city,country=country,expirationDate=expirationDate,
                                            nameOnCard=nameOnCard,pan=pan,state=state,amount=amount,
                                            processorName = processorName,transactionType=transactionType)
        
        obj = ProcessManualEntry.ProcessManualEntry(req)
        response = obj.CallService(webServiceUrl,certificateFileName,certificatePassword)
        print('Response:-')
        print('StatusCode:' + str(response.status_code))  if (not response is None) else print('StatusCode:' + str(response))
        print('Content:\n' + parseString(response.content).toprettyxml())  if (not response is None) else print('Content:' + str(response))
    except Exception as ex:
        print(ex)

def processtoken_ui():
       try:
         config = json.loads(open('config.json').read())
         webServiceUrl = config["MppgV4ServiceUrl"]
         #Currently mppg operations don't require certificate
         #In future if certificate file needs to be supported.
         #pass the appropriate certificate filename and password
         certificateFileName = None
         certificatePassword = None
         additionalRequestDataKey = read_struser_input("Enter AdditionalRequestData Key:")         
         additionalRequestDataValue = read_struser_input("Enter AdditionalRequestData Value:")
         customerCode = read_struser_input("Enter Customer Code:",False)
         userName = read_struser_input("Enter Username:",False)
         passWord = read_struser_input("Enter PassWord:",False)
         customerTransactionId = read_struser_input("Enter CustomerTransactionId:",True)         
         token = read_struser_input("Enter token:",False)
         amount = read_floatuser_input("Enter Amount:")
         processorName = read_struser_input("Enter ProcessorName:")
         transactionType = read_transactiontype_input("Enter transactionType:")  
         req = ProcessToken.ProcessTokenRequest(additionalRequestDataKey=additionalRequestDataKey
                                                ,additionalRequestDataValue=additionalRequestDataValue
                                                ,userName=userName
                                                ,passWord=passWord
                                                ,customerCode=customerCode
                                                ,customerTransactionId=customerTransactionId
                                                ,token=token
                                                ,amount=amount
                                                ,processorName=processorName
                                                ,transactionType=transactionType)
        
         obj = ProcessToken.ProcessToken(req)
         response = obj.CallService(webServiceUrl,certificateFileName,certificatePassword)
         print('Response:-')
         print('StatusCode:' + str(response.status_code))  if (not response is None) else print('StatusCode:' + str(response))
         print('Content:\n' + parseString(response.content).toprettyxml())  if (not response is None) else print('Content:' + str(response))
       except Exception as ex:
        print(ex)


def processkeypadentry_ui():
    try:
        config = json.loads(open('config.json').read())
        webServiceUrl = config["MppgV4ServiceUrl"]
        #Currently mppg operations don't require certificate
        #In future if certificate file needs to be supported.
        #pass the appropriate certificate filename and password
        certificateFileName = None
        certificatePassword = None
        customerCode = read_struser_input("Enter Customer Code:",False)
        userName = read_struser_input("Enter Username:",False)
        passWord = read_struser_input("Enter PassWord:",False)
        devicesn = read_struser_input("Enter DeviceSN:",False)
        ksn = read_struser_input("Enter KSN:",False)
        magneprint = read_struser_input("Enter MagnePrint:",False)
        magneprintstatus = read_struser_input("Enter MagnePrintStatus:",False)
        track1 = read_struser_input("Enter Track1:")
        track2 = read_struser_input("Enter Track2:")
        customertransactionid = read_struser_input("Enter CustomerTransactionId:")
        amount = read_floatuser_input("Enter Amount:")
        processorName = read_struser_input("Enter ProcessorName:")
        transactionType = read_transactiontype_input("Enter transactionType:")
       
        req = ProcessKeyPadEntry.ProcessKeyPadEntryRequest(customerCode=customerCode,userName=userName,
        passWord=passWord,deviceSn=devicesn,ksn=ksn,magnePrint=magneprint,magnePrintStatus=magneprintstatus,track1=track1,track2=track2,customerTransactionId=customertransactionid,amount=amount,processorName=processorName,transactionType=transactionType)
        obj = ProcessKeyPadEntry.ProcessKeyPadEntry(req)
        response = obj.CallService(webServiceUrl,certificateFileName,certificatePassword)
        print('Response:-')
        print('StatusCode:' + str(response.status_code))  if (not response is None) else print('StatusCode:' + str(response))
        print('Content:\n' + parseString(response.content).toprettyxml())  if (not response is None) else print('Content:' + str(response))
    except Exception as ex:
        print(ex)

#endregion

# method for choosing any option
def choose_option():
    print("Please Select Service Option number")
    print("1.ProcessCardSwipe")
    print("2:ProcessData")
    print("3:ProcessKeyPadEntry")
    print("4:ProcessManualEntry")
    print("5:ProcessToken")
    input_option = input()
    if(input_option.isdigit()):
        main(str(input_option)) #this function is used to redirect to the service
    else:
        print("Please enter only interger value")
        choose_option()

# go to particular option and process
def main(opt):
    option = int(opt)
    if option == 1:
        processcardswipe_ui()
        choose_option()
    elif option == 2:
        processdataemvauth_ui()
        choose_option()
    elif option == 3:
        processkeypadentry_ui()
        choose_option()
    elif option == 4:
        processmanualentry_ui()
        choose_option()
    elif option == 5:
        processtoken_ui()
        choose_option()
    else:
        print("Invalid Option")
     

if __name__ == "__main__":
    choose_option()
