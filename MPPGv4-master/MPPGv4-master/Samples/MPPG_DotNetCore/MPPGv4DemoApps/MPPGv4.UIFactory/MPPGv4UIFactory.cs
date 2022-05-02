using MPPGv4.Dtos;
using MPPGv4.ServiceFactory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

namespace MPPGv4.UIFactory
{
    public class Mppgv4UIfactory : IMppgv4UIFactory
    {
        IServiceProvider _serviceProvider;

        public Mppgv4UIfactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public void ShowUI(MPPGv4UI mPPGv4UI)
        {
            switch (mPPGv4UI)
            {
                case MPPGv4UI.PROCESSCARDSWIPE:
                    ShowProcessCardSwipeUI();
                    break;
                case MPPGv4UI.PROCESSMANUALENTRY:
                    ShowProcessManualEntryUI();
                    break;
                case MPPGv4UI.PROCESSDATA:
                    ShowProcessDataUI();
                    break;
                case MPPGv4UI.PROCESSKEYPADENTRY:
                    ShowProcessKeyPadEntryUI();
                    break;
                case MPPGv4UI.PROCESSTOKEN:
                    ShowProcessTokenUI();
                    break;
                //case MPPGv4UI.PROCESSREFERENCEID:
                //    break;
                default:
                    break;
            }
        }

        private void ShowProcessCardSwipeUI()
        {
            var requestDto = new ProcessCardSwipeRequestDto();
            try
            {
                requestDto.CustomerCode = Read_String_Input("Please enter value for CustomerCode", false);
                requestDto.Username = Read_String_Input("Please enter value for username", false);
                requestDto.Password = Read_String_Input("Please enter value for password", false);
                requestDto.DeviceSN = Read_String_Input("Please enter value for DeviceSN", true);
                requestDto.KSN = Read_String_Input("Please enter value for KSN", false);
                requestDto.MagnePrint = Read_String_Input("Please enter value for MagnePrint", false);
                requestDto.MagnePrintStatus = Read_String_Input("Please enter value for MagnePrintStatus", false);
                requestDto.Track1 = Read_String_Input("Please enter value for Track1", true);
                requestDto.Track2 = Read_String_Input("Please enter value for Track2", false);
                requestDto.Track3 = Read_String_Input("Please Enter value for Track3", true);
                requestDto.CustomerTransactionID = Read_String_Input("Please enter value for CustomerTransactionID", true);
                requestDto.Amount = Read_Decimal_Input("Please enter value for Amount");
                requestDto.ProcessorName = Read_String_Input("Please enter value for ProcessorName", false);
                requestDto.TransactionType = Read_TransactionType_Input("Please enter value for TransactionType");
                var svc = _serviceProvider.GetService<IProcessCardSwipeClient>();
                var responseDto = svc.ProcessCardSwipe(requestDto);
                if (responseDto != null)
                {
                    var response = responseDto as ProcessCardSwipeResponseDto;
                    Console.WriteLine("=====================Response Start======================");
                    Console.WriteLine("Status Code:" + response.StatusCode);
                    Console.WriteLine("Response:");
                    Console.Write(PrettyXml(response.PageContent) + "\n");
                    Console.WriteLine("=====================Response End======================");
                }
                else
                {
                    Console.WriteLine("Response is null, Please check with input values given and try again");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occurred while Processing ProcessCardSwipe" + ex.Message.ToString());
            }
        }
        private void ShowProcessDataUI()
        {
            var requestDto = new ProcessDataRequestDto();
            try
            {
                requestDto.CustomerCode = Read_String_Input("Enter Customer Code:", false);
                requestDto.Username = Read_String_Input("Enter Username:", false);
                requestDto.Password = Read_String_Input("Enter PassWord:", false);
                requestDto.CustomerTransactionID = Read_String_Input("Enter CustomerTransactionId:", true);
                requestDto.Data = Read_LongString_Input("Enter Data:", false);
                requestDto.DataFormatType = Read_DataFormatType_Input("Enter DataFormatType:");
                requestDto.EncryptionType = Read_String_Input("Enter EncryptionType :", false);
                requestDto.KSN = Read_String_Input("Enter KSN:", false);
                requestDto.NumberOfPaddedBytes = Read_Intuser_Input("Enter NumberOfPaddedBytes:");
                requestDto.IsEncrypted = Read_Isencrypted_Input("Enter IsEncrypted:");
                requestDto.PaymentMode = Read_PaymentMode_Input("Enter PaymentMode:");
                requestDto.Amount = Read_Intuser_Input("Enter Amount:");
                requestDto.ProcessorName = Read_String_Input("Enter ProcessorName:", false);
                requestDto.TransactionType = Read_TransactionType_Input("Enter transactionType:");
                var svc = _serviceProvider.GetService<IProcessDataClient>();
                var responseDto = svc.ProcessData(requestDto);
                if (responseDto != null)
                {
                    var response = responseDto as ProcessDataResponseDto;
                    Console.WriteLine("=====================Response Start======================");
                    Console.WriteLine("Status Code:" + response.StatusCode);
                    Console.WriteLine("Response:");
                    Console.Write(PrettyXml(response.PageContent) + "\n");
                    Console.WriteLine("=====================Response End======================");
                }
                else
                {
                    Console.WriteLine("Response is null, Please check with input values given and try again");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occurred while Processing ProcessData" + ex.Message.ToString());
            }
        }
        private void ShowProcessKeyPadEntryUI()
        {
            var requestDto = new ProcessKeyPadEntryRequestDto();
            try
            {
                requestDto.CustomerCode = Read_String_Input("Enter Customer Code:", false);
                requestDto.Username = Read_String_Input("Enter Username:", false);
                requestDto.Password = Read_String_Input("Enter PassWord:", false);
                requestDto.DeviceSN = Read_String_Input("Enter DeviceSN:", false);
                requestDto.KSN = Read_String_Input("Enter KSN:", false);
                requestDto.MagnePrint = Read_String_Input("Enter MagnePrint:", false);
                requestDto.MagnePrintStatus = Read_String_Input("Enter MagnePrintStatus:", false);
                requestDto.Track1 = Read_String_Input("Enter Track1:", true);
                requestDto.Track2 = Read_String_Input("Enter Track2:", false);
                requestDto.CustomerTransactionID = Read_String_Input("Enter CustomerTransactionId:", true);
                requestDto.Amount = Read_Decimal_Input("Enter Amount:");
                requestDto.ProcessorName = Read_String_Input("Enter ProcessorName:", false);
                requestDto.TransactionType = Read_TransactionType_Input("Enter transactionType:");
                var svc = _serviceProvider.GetService<IProcessKeyPadEntryClient>();
                var responseDto = svc.ProcessKeyPadEntry(requestDto);
                if (responseDto != null)
                {
                    var response = responseDto as ProcessKeyPadEntryResponseDto;
                    Console.WriteLine("=====================Response Start======================");
                    Console.WriteLine("Status Code:" + response.StatusCode);
                    Console.WriteLine("Response:");
                    Console.Write(PrettyXml(response.PageContent) + "\n");
                    Console.WriteLine("=====================Response End======================");
                }
                else
                {
                    Console.WriteLine("Response is null, Please check with input values given and try again");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error Occurred while Processing ProcessKeyPadEntry" + ex.Message.ToString());
            }
        }
        private void ShowProcessTokenUI()
        {
            var requestDto = new ProcessTokenRequestDto();
            try
            {
                requestDto.AdditionalRequestData_Key = Read_String_Input("Enter AdditionalRequestData Key:", false);
                requestDto.AdditionalRequestData_Value = Read_LongString_Input("Enter AdditionalRequestData Value:", false);
                requestDto.CustomerCode = Read_String_Input("Enter Customer Code:", false);
                requestDto.Username = Read_String_Input("Enter Username:", false);
                requestDto.Password = Read_String_Input("Enter PassWord:", false);
                requestDto.CustomerTransactionID = Read_String_Input("Enter CustomerTransactionId:", true);
                requestDto.Token = Read_String_Input("Enter token:", false);
                requestDto.Amount = Read_Decimal_Input("Enter Amount:");
                requestDto.ProcessorName = Read_String_Input("Enter ProcessorName:", false);
                requestDto.TransactionType = Read_TransactionType_Input("Enter transactionType:");
                var svc = _serviceProvider.GetService<IProcessTokenClient>();
                var responseDto = svc.ProcessToken(requestDto);
                if (responseDto != null)
                {
                    var response = responseDto as ProcessTokenResponseDto;
                    Console.WriteLine("=====================Response Start======================");
                    Console.WriteLine("Status Code:" + response.StatusCode);
                    Console.WriteLine("Response:");
                    Console.Write(PrettyXml(response.PageContent) + "\n");
                    Console.WriteLine("=====================Response End======================");
                }
                else
                {
                    Console.WriteLine("Response is null, Please check with input values given and try again");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error Occurred while Processing ProcessToken" + ex.Message.ToString());
            }
        }
        private void ShowProcessManualEntryUI()
        {
            var requestDto = new ProcessManualEntryRequestDto();
            try
            {
                requestDto.CustomerCode = Read_String_Input("Enter Customer Code:", false);
                requestDto.Username = Read_String_Input("Enter Username:", false);
                requestDto.Password = Read_String_Input("Enter PassWord:", false);
                requestDto.CustomerTransactionID = Read_String_Input("Enter CustomerTransactionId:", true);
                requestDto.AddressLine1 = Read_String_Input("Enter AddressLine1:", true);
                requestDto.AddressLine2 = Read_String_Input("Enter AddressLine2:", true);
                requestDto.City = Read_String_Input("Enter City:", true);
                requestDto.Country = Read_String_Input("Enter Country:", true);
                requestDto.ExpirationDate = Read_ExpirationDateuser_Input("Enter Expiration Date:", "Year(Ex:For Year 2025 Enter 25):", "Month:(Int Between 1-12 Ex: For 3 Enter 03 )");
                requestDto.NameOnCard = Read_String_Input("Enter NameOnCard:", true);
                requestDto.PAN = Read_String_Input("Enter PAN:", false);
                requestDto.State = Read_String_Input("Enter State:", true);
                requestDto.Amount = Read_Decimal_Input("Enter Amount:");
                requestDto.ProcessorName = Read_String_Input("Enter ProcessorName:", false);
                requestDto.TransactionType = Read_TransactionType_Input("Enter TransactionType:");
                var svc = _serviceProvider.GetService<IProcessManualEntryClient>();
                var responseDto = svc.ProcessManualEntry(requestDto);
                if (responseDto != null)
                {
                    var response = responseDto as ProcessManualEntryResponseDto;
                    Console.WriteLine("=====================Response Start======================");
                    Console.WriteLine("Status Code:" + response.StatusCode);
                    Console.WriteLine("Response:");
                    Console.Write(PrettyXml(response.PageContent) + "\n");
                    Console.WriteLine("=====================Response End======================");
                }
                else
                {
                    Console.WriteLine("Response is null, Please check with input values given and try again");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error Occurred while Processing ProcessManualEntry" + ex.Message.ToString());
            }
        }
        private static string Read_LongString_Input(string userMessage, bool isOptional)
        {
            Console.WriteLine(userMessage);
            byte[] inputBuffer = new byte[262144];
            Stream inputStream = Console.OpenStandardInput(262144);
            Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length));
            string strInput = Console.ReadLine();
            if ((!isOptional) && string.IsNullOrWhiteSpace(strInput))
            {
                return Read_LongString_Input(userMessage, isOptional);
            }
            return strInput;
        }
        private static string Read_String_Input(string question, bool isOptional)
        {
            Console.WriteLine(question);
            var ans = Console.ReadLine();
            if ((!isOptional) && string.IsNullOrWhiteSpace(ans))
            {
                return Read_String_Input(question, isOptional);
            }
            return ans;
        }
        private static string Read_ExpirationDate_Yearuser_Input(string question)
        {
            Console.WriteLine(question);
            string answer = Console.ReadLine();
            if ((!string.IsNullOrWhiteSpace(answer)) && (answer.Length == 2) && answer.All(char.IsDigit))
            {
                return answer;
            }
            Console.WriteLine("Invalid Input.");
            return Read_ExpirationDate_Yearuser_Input(question);
        }

        public static bool Between(int num, int lower, int upper, bool inclusive = true)
        {
            return inclusive ? lower <= num && num <= upper : lower < num && num < upper;
        }

        private static string Read_ExpirationDate_Monthuser_Input(string question)
        {
            Console.WriteLine(question);
            var answer = Console.ReadLine();
            if ((!string.IsNullOrWhiteSpace(answer)) && (answer.Length == 2) && answer.All(char.IsDigit) && Between(int.Parse(answer), 1, 12))
            {
                return answer;
            }
            Console.WriteLine("Invalid Input.");
            return Read_ExpirationDate_Monthuser_Input(question);
        }
        private static string Read_ExpirationDateuser_Input(string question, string yearlabel, string monthlabel)
        {
            Console.WriteLine(question);
            string year = Read_ExpirationDate_Yearuser_Input(yearlabel);
            string month = Read_ExpirationDate_Monthuser_Input(monthlabel);
            string answer = year + month;
            return answer;
        }
        private static string Read_TransactionType_Input(string question)
        {
            List<string> transactiontypes = new List<string> { "SALE", "AUTHORIZE", "CAPTURE", "VOID", "REFUND", "FORCE", "REJECT", "TOKEN" };
            Console.WriteLine(question);
            var ans = Console.ReadLine();
            if (transactiontypes.Contains<string>(ans))
                return ans;
            else
            {
                Console.WriteLine("Invalid TransactionType.");
                return Read_TransactionType_Input(question);
            }
        }
        private static string Read_PaymentMode_Input(string question)
        {
            List<string> paymentmodes = new List<string> { "EMV", "MagStripe" };
            Console.WriteLine(question);
            var ans = Console.ReadLine();
            if (paymentmodes.Contains<string>(ans))
                return ans;
            else
            {
                Console.WriteLine("Invalid PaymentMode.");
                return Read_PaymentMode_Input(question);
            }
        }
        private static string Read_DataFormatType_Input(string question)
        {
            List<string> dataformattypes = new List<string> { "TLV" };
            Console.WriteLine(question);
            var ans = Console.ReadLine();
            if (dataformattypes.Contains<string>(ans))
                return ans;
            else
            {
                Console.WriteLine("Invalid DataFormatType.");
                return Read_DataFormatType_Input(question);
            }
        }
        private static Decimal Read_Decimal_Input(string question)
        {
            Console.WriteLine(question);
            var ans = Console.ReadLine();
            try
            {
                var temp = Decimal.Parse(ans);
                return temp;
            }
            catch
            {
                Console.WriteLine("Invalid Input.");
                return Read_Decimal_Input(question);
            }
        }
        private static int Read_Intuser_Input(string question)
        {
            Console.WriteLine(question);
            var ans = Console.ReadLine();
            try
            {
                var temp = int.Parse(ans);
                return temp;
            }
            catch
            {
                Console.WriteLine("Invalid Input.");
                return Read_Intuser_Input(question);
            }
        }
        private static string Read_Isencrypted_Input(string question)
        {
            List<string> isencrypts = new List<string> { "true", "false" };
            Console.WriteLine(question);
            var ans = Console.ReadLine();
            if (isencrypts.Contains<string>(ans))
                return ans;
            else
            {
                Console.WriteLine("Invalid Isencrypted value.");
                return Read_Isencrypted_Input(question);
            }
        }
        private static string ReadLine()
        {
            byte[] inputBuffer = new byte[262144];
            Stream inputStream = Console.OpenStandardInput(262144);
            Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length));
            string strInput = Console.ReadLine();
            return strInput;
        }
        public static bool IsValidXml(string xml)
        {
            try
            {
                XDocument.Parse(xml);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string PrettyXml(string xml)
        {
            if (IsValidXml(xml)) //print xml in beautiful format
            {
                var stringBuilder = new StringBuilder();
                var element = XElement.Parse(xml);
                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                settings.NewLineOnAttributes = true;
                using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    element.Save(xmlWriter);
                }
                return stringBuilder.ToString();
            }
            else
            {
                return xml;
            }
        }
    }
}
