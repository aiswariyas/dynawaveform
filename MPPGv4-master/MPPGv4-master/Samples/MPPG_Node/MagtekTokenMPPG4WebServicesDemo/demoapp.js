const fs = require("fs");
const readln = require("readline");
const isNumber = require("is-number");
const formattoxml = require("xml-formatter");
const ProcessCardSwipe = require("./ProcessCardSwipe");
const ProcessData = require("./ProcessData");
const ProcessManualEntry = require("./ProcessManualEntry");
const ProcessKeyPadEntry = require("./ProcessKeyPadEntry");
const ProcessToken = require("./ProcessToken");


let cl = readln.createInterface(process.stdin, process.stdout);

//helper functions to read user inputs
const Answer = function (q) {
    return new Promise((resolve) => {
        cl.question(q, answer => {
            resolve(answer);
        });
    });
};

async function read_struser_input(question, IsOptional = true) {
    let ans = await Answer(question);
    if ((!IsOptional) && (ans.trim() === "")) {
        return read_struser_input(question, IsOptional);
    }
    return ans;
}

async function read_floatuser_input(question) {
    let ans = await Answer(question);

    if ((ans.trim === "") || (!isNumber(ans))) {
        return read_floatuser_input(question);
    } else {
        return ans;
    }
}


async function read_transactiontype_input(question) {
    const transactiontypes = ["SALE", "AUTHORIZE", "CAPTURE", "VOID", "REFUND", "FORCE", "REJECT", "TOKEN"];
    let ans = await Answer(question);
    if (transactiontypes.includes(ans)) {
        return ans;
    } else {
        console.log("Invalid TransactionType.");
        return read_transactiontype_input(question);
    }
}

function between(n, a, b) {
    return ((n >= a) && (n <= b));
}

async function read_yearuser_input(question) {
    let ans = await read_struser_input(question, false);
    if ((isNumber(ans)) && (parseInt(ans)) && (ans.length === 2)) {
        return parseInt(ans);
    } else {
        console.log("Invalid Input.");
        return read_yearuser_input(question);
    }
}


async function read_monthuser_input(question) {
    let ans = await read_struser_input(question, false);
    if ((isNumber(ans)) && (parseInt(ans)) && (between(parseInt(ans), 1, 12))) {
        return parseInt(ans);
    } else {
        console.log("Invalid Input.");
        return read_monthuser_input(question);
    }
}


async function read_expirationdate_input(question, yearlabel, monthlabel) {
    console.log(question);
    let year = await read_yearuser_input(yearlabel);
    let month = await read_monthuser_input(monthlabel);

    let formattedDay = ("0" + year).slice(-2);
    let formattedMonth = ("0" + month).slice(-2);
    return formattedDay.toString() + formattedMonth.toString();
}


//process card swipe ui
async function process_card_swipe_ui() {

    const configSettings = fs.readFileSync("config.json");
    const config = JSON.parse(configSettings);
    const tokenmppg4servicesurl = config.tokenmppg4servicesurl;
    //currently mppg operations don't require certificate 
    //In future if certificate file needs to be supported. Uncomment the below lines.
    //const path = require("path");
    //let certificateFileName = config.certificateFileName;
    //let certificatePassword = config.certificatePassword;
    //let currentDir = path.resolve(__dirname, "./");
    //let certificateFilePath = path.join(currentDir, certificateFileName);

    let certificateFilePath = null;
    let certificatePassword = null;

    console.log("=====================Request building start======================");
    let customercode = await read_struser_input("Enter Customercode:", false);
    let username = await read_struser_input("Enter Username:", false);
    let password = await read_struser_input("Enter Password:", false);
    let devicesn = await read_struser_input("Enter DeviceSN:", true);
    let ksn = await read_struser_input("Enter KSN:", false);
    let magneprint = await read_struser_input("Enter MagnePrint:", false);
    let magneprintstatus = await read_struser_input("Enter MagnePrintStatus:", false);
    let track1 = await read_struser_input("Enter track1:", true);
    let track2 = await read_struser_input("Enter track2:", false);
    let track3 = await read_struser_input("Enter track3:", true);
    let customertransactionId = await read_struser_input("Enter customertransactionId:", true);
    let amount = await read_floatuser_input("Enter amount:");
    let processorname = await read_struser_input("Enter processorname:", false);
    let transactionType = await read_transactiontype_input("Enter transactionType:", false);
    console.log("=====================Request building End======================");

    let obj = new ProcessCardSwipe(customercode, username, password, devicesn, ksn,
        magneprint, magneprintstatus, track1, track2, track3, customertransactionId,
        amount, processorname, transactionType);
    try {
        let response = await obj.CallWebService(tokenmppg4servicesurl, certificateFilePath, certificatePassword);
        console.log("Response StatusCode Is:-");
        console.log(response.statuscode);
        console.log("Response Is:-");
        console.log(formattoxml(response.data));
    }
    catch (ex) {
        console.log("Exception:-");
        console.log(ex.message);
    }
}

//process manual entry ui
async function process_manual_entry_ui() {
    const configSettings = fs.readFileSync("config.json");
    const config = JSON.parse(configSettings);
    const tokenmppg4servicesurl = config.tokenmppg4servicesurl;
    //currently mppg operations don't require certificate 
    //In future if certificate file needs to be supported. Uncomment the below lines.
    //const path = require("path");
    //let certificateFileName = config.certificateFileName;
    //let certificatePassword = config.certificatePassword;
    //let currentDir = path.resolve(__dirname, "./");
    //let certificateFilePath = path.join(currentDir, certificateFileName);

    let certificateFilePath = null;
    let certificatePassword = null;

    console.log("=====================Request building start======================");
    let customercode = await read_struser_input("Enter Customercode:", false);
    let username = await read_struser_input("Enter Username:", false);
    let password = await read_struser_input("Enter Password:", false);
    let customertransactionId = await read_struser_input("Enter customertransactionId:", true);
    let addressline1 = await read_struser_input("Enter Addressline1:", true);
    let addressline2 = await read_struser_input("Enter Addressline2:", true);
    let city = await read_struser_input("Enter City:", true);
    let country = await read_struser_input("Enter Country:", true);
    let expirationdate = await read_expirationdate_input("Enter Expirationdate:", "Year(Ex:For Year 2025 Enter 25):", "Month(Ex:Int Between 1-12):");
    let nameoncard = await read_struser_input("Enter Nameoncard:", true);
    let pan = await read_struser_input("Enter Pan:", false);
    let state = await read_struser_input("Enter State:", true);
    let amount = await read_floatuser_input("Enter amount:");
    let processorname = await read_struser_input("Enter processorname:", false);
    let transactionType = await read_transactiontype_input("Enter transactionType:");
    console.log("=====================Request building End======================");

    let obj = new ProcessManualEntry(customercode, username, password,
        customertransactionId, addressline1, addressline2, city, country,
        expirationdate, nameoncard, pan, state, amount, processorname, transactionType);
    try {
        let response = await obj.CallWebService(tokenmppg4servicesurl, certificateFilePath, certificatePassword);
        console.log("Response StatusCode Is:-");
        console.log(response.statuscode);
        console.log("Response Is:-");
        console.log(formattoxml(response.data));
    }
    catch (ex) {
        console.log("Exception:-");
        console.log(ex.message);
    }
}

//process data ui
async function process_data_ui() {
    const configSettings = fs.readFileSync("config.json");
    const config = JSON.parse(configSettings);
    const tokenmppg4servicesurl = config.tokenmppg4servicesurl;
    //currently mppg operations don't require certificate 
    //In future if certificate file needs to be supported. Uncomment the below lines.
    //const path = require("path");
    //let certificateFileName = config.certificateFileName;
    //let certificatePassword = config.certificatePassword;
    //let currentDir = path.resolve(__dirname, "./");
    //let certificateFilePath = path.join(currentDir, certificateFileName);

    let certificateFilePath = null;
    let certificatePassword = null;
    console.log("=====================Request building start======================");
    let customercode = await read_struser_input("Enter Customercode:", false);
    let username = await read_struser_input("Enter Username:", false);
    let password = await read_struser_input("Enter Password:", false);
    let customerTransactionId = await read_struser_input("Enter customertransactionId:", true);
    let data = await read_struser_input("Enter Data:", false);
    let dataformattype = await read_struser_input("Enter DataFormatType:", false);
    let encryptiontype = await read_struser_input("Enter EncryptionType:", false);
    let ksn = await read_struser_input("Enter KSN:", false);
    let numberofpaddedbytes = await read_struser_input("Enter NumberOfPaddedBytes:", false);
    let isencrypted = await read_struser_input("Enter isEncrypted:", false);
    let paymentmode = await read_struser_input("Enter PaymentMode:", false);
    let amount = await read_floatuser_input("Enter Amount:", false);
    let processorname = await read_struser_input("Enter ProcessorName:", false);
    let transactionType = await read_transactiontype_input("Enter TransactionType:", false);
    let obj = new ProcessData(customercode, username, password, customerTransactionId, data, dataformattype, encryptiontype, ksn, numberofpaddedbytes, isencrypted, paymentmode, amount, processorname, transactionType);
    try {
        let response = await obj.CallWebService(tokenmppg4servicesurl, certificateFilePath, certificatePassword);
        console.log("Response StatusCode Is:-");
        console.log(response.statuscode);
        console.log("Response Is:-");
        console.log(formattoxml(response.data));
    }
    catch (ex) {
        console.log("Exception:-");
        console.log(ex.message);
    }
}

//process_token_ui
async function process_token_ui() {
    const configSettings = fs.readFileSync("config.json");
    const config = JSON.parse(configSettings);
    const tokenmppg4servicesurl = config.tokenmppg4servicesurl;
    //currently mppg operations don't require certificate 
    //In future if certificate file needs to be supported. Uncomment the below lines.
    //const path = require("path");
    //let certificateFileName = config.certificateFileName;
    //let certificatePassword = config.certificatePassword;
    //let currentDir = path.resolve(__dirname, "./");
    //let certificateFilePath = path.join(currentDir, certificateFileName);

    let certificateFilePath = null;
    let certificatePassword = null;
    console.log("=====================Request building start======================");
    let customercode = await read_struser_input("Enter Customercode:", false);
    let username = await read_struser_input("Enter Username:", false);
    let password = await read_struser_input("Enter Password:", false);
    let customertransactionId = await read_struser_input("Enter customertransactionId:", true);
    let token = await read_struser_input("Enter token:", false);
    let amount = await read_floatuser_input("Enter Amount:", false);
    let processorname = await read_struser_input("Enter ProcessorName:", false);
    let transactionType = await read_transactiontype_input("Enter TransactionType:", false);
    let additionalRequestData_key = await read_struser_input("Enter additionalRequestData_key:", false);
    let additionalRequestData_value = await read_struser_input("Enter additionalRequestData_value:", false);
    let obj = new ProcessToken(customercode, username, password, customertransactionId, additionalRequestData_key,
        additionalRequestData_value, token, amount, processorname, transactionType);
    try {
        let response = await obj.CallWebService(tokenmppg4servicesurl, certificateFilePath, certificatePassword);
        console.log("Response StatusCode Is:-");
        console.log(response.statuscode);
        console.log("Response Is:-");
        console.log(formattoxml(response.data));
    }
    catch (ex) {
        console.log("Exception:-");
        console.log(ex.message);
    }
}

//processkeypad entry ui
async function process_keypad_entry_ui() {
    const configSettings = fs.readFileSync("config.json");
    const config = JSON.parse(configSettings);
    const tokenmppg4servicesurl = config.tokenmppg4servicesurl;
    //currently mppg operations don't require certificate 
    //In future if certificate file needs to be supported. Uncomment the below lines.
    //const path = require("path");
    //let certificateFileName = config.certificateFileName;
    //let certificatePassword = config.certificatePassword;
    //let currentDir = path.resolve(__dirname, "./");
    //let certificateFilePath = path.join(currentDir, certificateFileName);

    let certificateFilePath = null;
    let certificatePassword = null;
    console.log("=====================Request building start======================");
    let customercode = await read_struser_input("Enter Customercode ", false);
    let username = await read_struser_input("Enter Username:", false);
    let password = await read_struser_input("Enter Password:", false);
    let devicesn = await read_struser_input("Enter devicesn:", true);
    let ksn = await read_struser_input("Enter ksn:", false);
    let magneprint = await read_struser_input("Enter magneprint:", false);
    let magneprintstatus = await read_struser_input("Enter magneprintstatus:", false);
    let track1 = await read_struser_input("Enter track1:", true);
    let track2 = await read_struser_input("Enter track2:", false);
    let track3 = await read_struser_input("Enter track3:", true);
    let customertransactionId = await read_struser_input("Enter customertransactionId:", true);
    let amount = await read_floatuser_input("Enter Amount:", false);
    let processorname = await read_struser_input("Enter processorname:", false);
    let transactionType = await read_transactiontype_input("Enter TransactionType:", false);
    let obj = new ProcessKeyPadEntry(customercode, username, password, devicesn, ksn, magneprint, magneprintstatus, track1, track2, track3, customertransactionId, amount, processorname, transactionType);
    try {
        let response = await obj.CallWebService(tokenmppg4servicesurl, certificateFilePath, certificatePassword);
        console.log("Response StatusCode Is:-");
        console.log(response.statuscode);
        console.log("Response Is:-");
        console.log(formattoxml(response.data));
    }
    catch (ex) {
        console.log("Exception:-");
        console.log(ex.message);
    }
}


//main function
(async () => {
    for (; ;) {
        console.log("Please Select Service Option Number (1.ProcessCardSwipe,2.ProcessManualEntry,3.ProcessData,4.ProcessToken,5.ProcessKeyPadEntry)");
        let opt = await read_struser_input("Enter Option:", true);
        if (isNumber(opt)) {
            let option = parseInt(opt);
            switch (option) {
                case 1:
                    await process_card_swipe_ui();
                    break;
                case 2:
                    await process_manual_entry_ui();
                    break;
                case 3:
                    await process_data_ui();
                    break;
                case 4:
                    await process_token_ui();
                    break;
                case 5:
                    await process_keypad_entry_ui();
                    break;
                default:
                    console.log("Invalid Option");
                    cl.close();
                    process.exit();
            }
        }
        else {
            console.log("Enter Numbers Only");
        }
    }
})();