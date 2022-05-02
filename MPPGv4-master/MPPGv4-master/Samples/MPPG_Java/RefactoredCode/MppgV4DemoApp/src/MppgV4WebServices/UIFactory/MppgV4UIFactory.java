package MppgV4WebServices.UIFactory;

import MppgV4WebServices.Factory.MagtekMppgV4WebServicesFactory;
import MppgV4WebServices.Pojos.ProcessCardSwipeRequest;
import MppgV4WebServices.Pojos.ProcessCardSwipeResponse;
import MppgV4WebServices.Pojos.ProcessDataRequest;
import MppgV4WebServices.Pojos.ProcessDataResponse;
import MppgV4WebServices.Pojos.ProcessKeyPadEntryRequest;
import MppgV4WebServices.Pojos.ProcessManualEntryRequest;
import MppgV4WebServices.Pojos.ProcessManualEntryResponse;
import MppgV4WebServices.Pojos.ProcessReferenceIDRequest;
import MppgV4WebServices.Pojos.ProcessReferenceIDResponse;
import MppgV4WebServices.Pojos.ProcessTokenRequest;
import MppgV4WebServices.Pojos.ProcessTokenResponse;
import MppgV4WebServices.Interfaces.IMagtekMppgV4WebServicesFactory;
import MppgV4WebServices.Interfaces.IMagtekMppgV4WebServicesResponse;
import MppgV4WebServices.Pojos.ProcessKeyPadEntryResponse;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.Scanner;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Source;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;

public class MppgV4UIFactory implements IMppgV4UIFactory {

    public MppgV4UIFactory() {
    }

    @Override
    public void ShowUI(MppgV4UI mppgV4UI, String webServiceUrl, String certificateFileName, String certificatePassword) {

        switch (mppgV4UI) {
            case PROCESSCARDSWIPE:
                try {
                    ProcessCardSwipeRequest request = new ProcessCardSwipeRequest();
                    request.setCustomerTransactionID(readStringUserInput("Enter CustomerTransactionID:", true));
                    request.setCustomerCode(readStringUserInput("Enter CustomerCode:", false));
                    request.setUserName(readStringUserInput("Enter Username:", false));
                    request.setPassword(readStringUserInput("Enter PassWord:", false));
                    request.setDeviceSN(readStringUserInput("Enter DeviceSN:", false));
                    request.setkSN(readStringUserInput("Enter KSN:", false));
                    request.setMagnePrint(readStringUserInput("Enter MagnePrint:", false));
                    request.setMagnePrintStatus(readStringUserInput("Enter MagnePrintStatus:", false));
                    request.setTrack1(readStringUserInput("Enter Track1:", false));
                    request.setTrack2(readStringUserInput("Enter Track2:", false));
                    request.setTrack3(readStringUserInput("Enter Track3:", true));
                    double amt = read_Decimal_Input("Enter Amount:");
                    request.setAmount(amt);
                    request.setProcessorName(readStringUserInput("Enter ProcessorName:", false));
                    request.setTransactionType(read_TransactionType_Input("Enter TransactionType:"));
                    String url = webServiceUrl;
                    IMagtekMppgV4WebServicesFactory factory = new MagtekMppgV4WebServicesFactory();
                    IMagtekMppgV4WebServicesResponse iResponse = factory.request(request, url, certificateFileName, certificatePassword);
                    ProcessCardSwipeResponse response = (ProcessCardSwipeResponse) iResponse;
                    if (response != null) {
                        System.out.println("===============================Response Details====================================");
                        System.out.println(prettyFormat(response.getRawResponse(),2));
                        System.out.println("===============================Response Details====================================");
                    } else {
                        System.out.println("Response is null; check the input values ");
                    }
                } catch (Exception ex) {
                    System.out.println("Exception while ProcessCardSwipe" + ex);
                }
                break;
            case PROCESSDATA:
                try {
                    ProcessDataRequest request = new ProcessDataRequest();
                    request.setCustomerTransactionID(readStringUserInput("Enter CustomerTransactionID:", true));
                    request.setCustomerCode(readStringUserInput("Enter CustomerCode:", false));
                    request.setUserName(readStringUserInput("Enter Username:", false));
                    request.setPassword(readStringUserInput("Enter PassWord:", false));
                    request.setData(readStringUserInput("Enter Data:", false));
                    request.setKSN(readStringUserInput("Enter KSN:", false));
                    request.setDataFormatType(read_dataFormatType_Input("Enter DataFormatType:"));
                    request.setEncryptionType(readStringUserInput("Enter EncryptionType:", false));
                    request.setIsEncrypted(read_isEncrypted_Input("Enter IsEncrypted:"));
                    int NoBytes=read_Intuser_Input("Enter NumberOfPaddedBytes:");
                    request.setNumberOfPaddedBytes(String.valueOf(NoBytes));
                    request.setPaymentMode(read_paymentMode_Input("Enter PaymentMode:"));
                    request.setProcessorName(readStringUserInput("Enter ProcessorName:", false));
                    request.setTransactionType(read_TransactionType_Input("Enter TransactionType:"));
                    double amt = read_Decimal_Input("Enter Amount:");
                    request.setAmount(String.valueOf(amt));
                    String url = webServiceUrl;
                    IMagtekMppgV4WebServicesFactory factory = new MagtekMppgV4WebServicesFactory();
                    IMagtekMppgV4WebServicesResponse iResponse = factory.request(request, url, certificateFileName, certificatePassword);
                    ProcessDataResponse response = (ProcessDataResponse) iResponse;
                    if (response != null) {
                        System.out.println("===============================Response Details====================================");
                        System.out.println(prettyFormat(response.getRawResponse(), 2));
                        System.out.println("===============================Response Details====================================");

                    } else {
                        System.out.println("Response is null; check the input values ");
                    }
                } catch (Exception ex) {
                    System.out.println("Exception while ProcessData" + ex);
                }
                break;
            case PROCESSKEYPADENTRY:
                try {
                    ProcessKeyPadEntryRequest request = new ProcessKeyPadEntryRequest();
                    request.setCustomerTransactionID(readStringUserInput("Enter CustomerTransactionID:", true));
                    request.setCustomerCode(readStringUserInput("Enter CustomerCode:", false));
                    request.setUserName(readStringUserInput("Enter Username:", false));
                    request.setPassword(readStringUserInput("Enter PassWord:", false));
                    //request.setCVV(readStringUserInput("Enter CVV:", true));
                    request.setDeviceSN(readStringUserInput("Enter DeviceSN:", false));
                    request.setKSN(readStringUserInput("Enter KSN:", false));
                    request.setMagnePrint(readStringUserInput("Enter MagnePrint:", false));
                    request.setMagnePrintStatus(readStringUserInput("Enter MagnePrintStatus:", false));
                    request.setTrack1(readStringUserInput("Enter Track1:", false));
                    request.setTrack2(readStringUserInput("Enter Track2:", false));
                    request.setTrack3(readStringUserInput("Enter Track3:", true));
                    double amt = read_Decimal_Input("Enter Amount:");
                    request.setAmount(String.valueOf(amt));
                    request.setProcessorName(readStringUserInput("Enter ProcessorName:", false));
                    request.setTransactionType(read_TransactionType_Input("Enter TransactionType:"));
                   // request.setZIP(readStringUserInput("Enter ZIP:", true));
                    String url = webServiceUrl;
                    IMagtekMppgV4WebServicesFactory factory = new MagtekMppgV4WebServicesFactory();
                    IMagtekMppgV4WebServicesResponse iResponse = factory.request(request, url, certificateFileName, certificatePassword);
                    ProcessKeyPadEntryResponse response = (ProcessKeyPadEntryResponse) iResponse;
                    if (response != null) {
                        System.out.println("===============================Response Details====================================");
                        System.out.println(prettyFormat(response.getRawResponse(), 2));
                        System.out.println("===============================Response Details====================================");

                    } else {
                        System.out.println("Response is null; check the input values ");
                    }
                } catch (Exception ex) {
                    System.out.println("Exception while ProcessKeyPadEntry" + ex);
                }
                break;
            case PROCESSMANUALENTRY:
                try {
                    ProcessManualEntryRequest request = new ProcessManualEntryRequest();
                    request.setCustomerCode(readStringUserInput("Enter CustomerCode:", false));
                    request.setUserName(readStringUserInput("Enter Username:", false));
                    request.setPassword(readStringUserInput("Enter PassWord:", false));
                   // request.setCVV(readStringUserInput("Enter CVV:", true));
                    request.setAddressLine1(readStringUserInput("Enter AddressLine1:", true));
                    request.setAddressLine2(readStringUserInput("Enter AddressLine2:", true));
                    request.setCity(readStringUserInput("Enter City:", true));
                    request.setCountry(readStringUserInput("Enter Country:", true));
                    request.setState(readStringUserInput("Enter State:", true));
                    request.setCustomerTransactionID(readStringUserInput("Enter CustomerTransactionID:", true));
                    double amt = read_Decimal_Input("Enter Amount:");
                    request.setAmount(String.valueOf(amt));
                    request.setNameOnCard(readStringUserInput("Enter NameOnCard:", true));
                    request.setExpirationDate(read_Expirationdateuser_Input("Enter Expiration Date:", "Year(Ex:For Year 2025 Enter 25):", "Month:(Ex:Int Between 1-12 )"));
                    request.setPAN(readStringUserInput("Enter PAN:", false));
                    request.setProcessorName(readStringUserInput("Enter ProcessorName:", false));
                    request.setTransactionType(read_TransactionType_Input("Enter TransactionType:"));
                    String url = webServiceUrl;
                    IMagtekMppgV4WebServicesFactory factory = new MagtekMppgV4WebServicesFactory();
                    IMagtekMppgV4WebServicesResponse iResponse = factory.request(request, url, certificateFileName, certificatePassword);
                    ProcessManualEntryResponse response = (ProcessManualEntryResponse) iResponse;
                    if (response != null) {
                        System.out.println("===============================Response Details====================================");
                        System.out.println(prettyFormat(response.getRawResponse(), 2));
                        System.out.println("===============================Response Details====================================");

                    } else {
                        System.out.println("Response is null; check the input values ");
                    }
                } catch (Exception ex) {
                    System.out.println("Exception while ProcessManualEntry" + ex);
                }
                break;
            case PROCESSREFERENCEID:
                try {
                    ProcessReferenceIDRequest request = new ProcessReferenceIDRequest();
                    request.setCustomerTransactionID(readStringUserInput("Enter CustomerTransactionID:", true));
                    request.setCustomerCode(readStringUserInput("Enter CustomerCode:", false));
                    request.setUsername(readStringUserInput("Enter Username:", false));
                    request.setPassword(readStringUserInput("Enter Password:", false));
                    request.settransactionInputDetails_Key(readStringUserInput("Enter TransactionInputDetails_Key:", true));
                    request.setTransactionInputDetails_Value(readStringUserInput("Enter TransactionInputDetails_Value:", true));
                    double amt = read_Decimal_Input("Enter Amount:");
                    request.setAmount(String.valueOf(amt));
                    request.setReferenceAuthCode(readStringUserInput("Enter ReferenceAuthCode:", false));
                    request.setReferenceTransactionID(readStringUserInput("Enter ReferenceTransactionID:", false));
                    request.setProcessorName(readStringUserInput("Enter ProcessorName:", false));
                    request.setTransactionType(read_TransactionType_Input("Enter TransactionType:"));
                    String url = webServiceUrl;
                    IMagtekMppgV4WebServicesFactory factory = new MagtekMppgV4WebServicesFactory();
                    IMagtekMppgV4WebServicesResponse iResponse = factory.request(request, url, certificateFileName, certificatePassword);
                    ProcessReferenceIDResponse response = (ProcessReferenceIDResponse) iResponse;
                    if (response != null) {
                        System.out.println("===============================Response Details====================================");
                        System.out.println(prettyFormat(response.getRawResponse(), 2));
                        System.out.println("===============================Response Details====================================");

                    } else {
                        System.out.println("Response is null; check the input values ");
                    }
                } catch (Exception ex) {
                    System.out.println("Exception while ProcessReferenceID" + ex);
                }
                break;
            case PROCESSTOKEN:
                try {
                    ProcessTokenRequest request = new ProcessTokenRequest();
                    request.setCustomerTransactionID(readStringUserInput("Enter CustomerTransactionID:", true));
                    request.setCustomerCode(readStringUserInput("Enter CustomerCode:", false));
                    request.setUsername(readStringUserInput("Enter Username:", false));
                    request.setPassword(readStringUserInput("Enter Password:", false));
                    request.setAdditionalRequestData_Key(readStringUserInput("Enter setAdditionalRequestData_Key:", true));
                    request.setAdditionalRequestData_Value(ReadMultiLineInput("Enter setAdditionalRequestData_Value:"));
                    request.setToken(readStringUserInput("Enter Token:", false));
                    request.setAmount(read_Decimal_Input("Enter Amount:"));
                    request.setProcessorName(readStringUserInput("Enter ProcessorName:", false));
                    request.setTransactionType(read_TransactionType_Input("Enter TransactionType:"));
                    String url = webServiceUrl;
                    IMagtekMppgV4WebServicesFactory factory = new MagtekMppgV4WebServicesFactory();
                    IMagtekMppgV4WebServicesResponse iResponse = factory.request(request, url, certificateFileName, certificatePassword);
                   
                    ProcessTokenResponse response = (ProcessTokenResponse) iResponse;
                    if (response != null) {
                        System.out.println("===============================Response Details====================================");
                        System.out.println(prettyFormat(response.getRawResponse(), 2));
                        System.out.println("===============================Response Details====================================");

                    } else {
                        System.out.println("Response is null; check the input values ");
                    }
                } catch (Exception ex) {
                    System.out.println("Exception while ProcessToken" + ex);
                }
                break;

        }
    }

    private static String read_Expirationdate_Yearuser_Input(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();
        if (("".equals(answer.trim())) || (answer.length() != 2)) {
            System.out.println("Invalid Year.");
            return read_Expirationdate_Yearuser_Input(question);
        }
        return answer;
    }

    public static String read_Expirationdate_Monthuser_Input(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();
        if (("".equals(answer.trim())) || (answer.length() != 2)) {
            System.out.println("Invalid Month.");
            return read_Expirationdate_Yearuser_Input(question);
        }
        return answer;
    }

    public static String read_Expirationdateuser_Input(String question, String yearlabel, String monthlabel) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String year = read_Expirationdate_Yearuser_Input(yearlabel);
        String month = read_Expirationdate_Monthuser_Input(monthlabel);
        String answer = year + month;
        return answer;
    }

    public static boolean isInteger(String str) {
        try {
            int num = Integer.parseInt(str);
            return true;
        } catch (NumberFormatException e) {
            return false;
        }
    }

    public static String readYearUserInput(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();

        if ((!answer.equals("")) && (answer.length() == 4) && (isInteger(answer))) {
            return answer;
        }
        System.out.println("Invalid Input");
        return readYearUserInput(question);
    }

    public static boolean Between(int num, int lower, int upper, boolean inclusive) {
        return inclusive ? lower <= num && num <= upper : lower < num && num < upper;
    }

    public static String readMonthUserInput(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();

        if ((!answer.equals("")) && (answer.length() == 2) && (isInteger(answer)) && Between(Integer.parseInt(answer), 1, 12, true)) {
            return answer;
        }
        System.out.println("Invalid Input");
        return readMonthUserInput(question);
    }

    public static String readDayUserInput(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();

        if ((!answer.equals("")) && (answer.length() == 2) && (isInteger(answer)) && Between(Integer.parseInt(answer), 1, 31, true)) {
            return answer;
        }
        System.out.println("Invalid Input");
        return readDayUserInput(question);
    }

    public static String readHourUserInput(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();

        if ((!answer.equals("")) && (answer.length() == 2) && (isInteger(answer)) && Between(Integer.parseInt(answer), 0, 23, true)) {
            return answer;
        }
        System.out.println("Invalid Input");
        return readHourUserInput(question);
    }

    public static String readMinuteUserInput(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();

        if ((!answer.equals("")) && (answer.length() == 2) && (isInteger(answer)) && Between(Integer.parseInt(answer), 0, 59, true)) {
            return answer;
        }
        System.out.println("Invalid Input");
        return readMinuteUserInput(question);
    }

    public static String readSecondUserInput(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();

        if ((!answer.equals("")) && (answer.length() == 2) && (isInteger(answer)) && Between(Integer.parseInt(answer), 0, 59, true)) {
            return answer;
        }
        System.out.println("Invalid Input");
        return readSecondUserInput(question);
    }

    public static String read_TransactionType_Input(String question) {
        Scanner input = new Scanner(System.in);
        List<String> transactiontypes = new ArrayList<>();
        transactiontypes.add("SALE");
        transactiontypes.add("AUTHORIZE");
        transactiontypes.add("CAPTURE");
        transactiontypes.add("VOID");
        transactiontypes.add("REFUND");
        transactiontypes.add("FORCE");
        transactiontypes.add("REJECT");
        transactiontypes.add("TOKEN");
        System.out.println(question);
        String ans = input.nextLine();
        if (transactiontypes.contains(ans)) {
            return ans;
        } else {
            System.out.println("Invalid TransactionType.");
            return read_TransactionType_Input(question);
        }
    }

    public static String read_dataFormatType_Input(String question) {
        Scanner input = new Scanner(System.in);
        List<String> dataformatTypes = new ArrayList<>();
        dataformatTypes.add("TLV");
        System.out.println(question);
        String ans = input.nextLine();
        if (dataformatTypes.contains(ans)) {
            return ans;
        } else {
            System.out.println("Invalid DataFormatType.");
            return read_dataFormatType_Input(question);
        }
    }
public static String read_paymentMode_Input(String question) {
        Scanner input = new Scanner(System.in);
        List<String> paymentModes = new ArrayList<>();
        paymentModes.add("EMV");
        paymentModes.add("Magstripe");
        System.out.println(question);
        String ans = input.nextLine();
        if (paymentModes.contains(ans)) {
            return ans;
        } else {
            System.out.println("Invalid PaymentMode.");
            return read_paymentMode_Input(question);
        }
    }
public static String read_isEncrypted_Input(String question) {
        Scanner input = new Scanner(System.in);
        List<String> paymentModes = new ArrayList<>();
        paymentModes.add("true");
        paymentModes.add("false");
        System.out.println(question);
        String ans = input.nextLine();
        if (paymentModes.contains(ans)) {
            return ans;
        } else {
            System.out.println("Invalid IsEncrypted value.");
            return read_isEncrypted_Input(question);
        }
    }
    public static double read_Decimal_Input(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();
        try {
            double temp = Double.parseDouble(answer);
            return temp;
        } catch (NumberFormatException ex) {
            System.out.println("Invalid Input.");
            return read_Decimal_Input(question);
        }

    }

    public static String readStringUserInput(String question, boolean isOptional) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();
        if ((!isOptional) && (answer.equals(""))) {
            return readStringUserInput(question, isOptional);
        }
        return answer;
    }

    public static String readUserInput(String question, boolean isMandatory) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String answer = input.nextLine();
        if ((answer.equals("")) && (isMandatory == true)) {
            answer = readUserInput(question, true);
        }
        return answer;
    }

    public static int read_Intuser_Input(String question) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String ans = input.nextLine();
        try {
            int temp = Integer.parseInt(ans);
            return temp;
        } catch (NumberFormatException ex) {
            System.out.println("Invalid Input.");
            return read_Intuser_Input(question);
        }
    }

    public static String ReadMultiLineInput(String question) throws Exception {
        System.out.println(question);
        System.out.println("Press Q to quit");
        List<String> tokens = new ArrayList<>();
        Scanner scanner = new Scanner(System.in);

        while (scanner.hasNext()) {
            String temp = scanner.next();
            if ("Q".equals(temp.toUpperCase())) {
                break;
            } else {
                tokens.add(temp);
            }
        }
        //scanner.close();
        return tokens.toString();

    }

    public static String prettyFormat(String input, int indent) throws TransformerConfigurationException, TransformerException {
        try {
            Source xmlInput = new StreamSource(new StringReader(input));
            StringWriter stringWriter = new StringWriter();
            StreamResult xmlOutput = new StreamResult(stringWriter);
            TransformerFactory transformerFactory = TransformerFactory.newInstance();
            transformerFactory.setAttribute("indent-number", indent);
            Transformer transformer = transformerFactory.newTransformer();
            transformer.setOutputProperty(OutputKeys.INDENT, "yes");
            transformer.transform(xmlInput, xmlOutput);
            return xmlOutput.getWriter().toString();
        } catch (IllegalArgumentException | TransformerException e) {
            throw new RuntimeException(e); // simple exception handling, please review it
        }
    }

    public static String readuserMultilineInput(String question, boolean isMandatory) {
        Scanner input = new Scanner(System.in);
        System.out.println(question);
        String line = input.nextLine();
        try {
            if (line.equals("") && (isMandatory == true)) {
                line = readUserInput(question, true);
            } else {
                BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));
                line = reader.readLine();
                // Printing the read line 
                //System.out.println(line);  
                reader.close();
            }

            return line;
        } catch (IOException e) {
            e.printStackTrace();
        }
        return line;
    }
}
