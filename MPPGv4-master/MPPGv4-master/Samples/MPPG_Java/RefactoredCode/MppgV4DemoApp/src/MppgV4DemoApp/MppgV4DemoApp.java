package MppgV4DemoApp;

import MppgV4WebServices.UIFactory.IMppgV4UIFactory;
import MppgV4WebServices.UIFactory.MppgV4UI;
import MppgV4WebServices.UIFactory.MppgV4UIFactory;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.URLDecoder;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Properties;
import java.util.Scanner;

public class MppgV4DemoApp {

    public static void main(String[] args) throws FileNotFoundException, IOException {

        String configFilePath = getConfigFilePath();
        File configFile = new File(configFilePath);
        String mppgv4webserviceurl;
        String certificateFileName;
        String certificatePassword;
        String certificateFilePath;
        try ( FileReader reader = new FileReader(configFile)) {
            Properties props = new Properties();
            props.load(reader);
            mppgv4webserviceurl = props.getProperty("mppgv4webserviceurl");
            certificateFileName = props.getProperty("certificate_filename");
//            if (runningFromJar()) {
//                System.out.println("running from jar");
//                File f = new File(System.getProperty("java.class.path"));
//                File dir = f.getAbsoluteFile().getParentFile();
//                String currentDirectory = dir.toString();
//                Path path = Paths.get(currentDirectory, certificateFileName);
//                certificateFilePath = path.toString();
//            } else {
//                System.out.println("not running from jar");
//                certificateFilePath = MppgV4DemoApp.class.getResource(certificateFileName).getPath();
//                certificateFilePath = URLDecoder.decode(certificateFilePath, "UTF-8");
//            }
//            certificateFilePath = MppgV4DemoApp.class.getResource(certificateFileName).getPath();
 certificateFilePath = getCertificateFilePath(certificateFileName);
            //At present Mppg operations does not require Certificate filename and password
            //In future if require certificate, below code is compatible for such scenarios. 
            //if not both certificateFilePath,certificatePassword will be passed empty
            if ("".equals(certificateFileName.trim())) {
                certificateFilePath = "";
                certificatePassword = "";

            } else {
                System.out.println("Certificate File Path Is");
                System.out.println(certificateFilePath);
                certificatePassword = props.getProperty("certificate_password");
            }
        }
        outer:
        for (;;) {
            IMppgV4UIFactory mggpv4UIFactory = new MppgV4UIFactory();
            Scanner input = new Scanner(System.in);
            int operation;
            System.out.println("========================== Select Operation Choice=====================================");
            System.out.println(
                    "Please select options "
                    + "1: ProcessCardSwipe "
                    + "2: ProcessData "
                    + "3: ProcessManualEntry "
                    + "4: ProcessKeyPadEntry "
                    + "5: ProcessToken "
            );
            operation = input.nextInt();
            switch (operation) {
                case 1:
                    mggpv4UIFactory.ShowUI(MppgV4UI.PROCESSCARDSWIPE, mppgv4webserviceurl, certificateFilePath, certificatePassword);
                    break;
                case 2:
                    mggpv4UIFactory.ShowUI(MppgV4UI.PROCESSDATA, mppgv4webserviceurl, certificateFilePath, certificatePassword);
                    break;
                case 3:
                    mggpv4UIFactory.ShowUI(MppgV4UI.PROCESSMANUALENTRY, mppgv4webserviceurl, certificateFilePath, certificatePassword);
                    break;
                case 4:
                    mggpv4UIFactory.ShowUI(MppgV4UI.PROCESSKEYPADENTRY, mppgv4webserviceurl, certificateFilePath, certificatePassword);
                    break;
                case 5:
                    mggpv4UIFactory.ShowUI(MppgV4UI.PROCESSTOKEN, mppgv4webserviceurl, certificateFilePath, certificatePassword);
                    break;
                //case 6:
                //  mggpv4UIFactory.ShowUI(MppgV4UI.PROCESSREFERENCEID,mppgv4webserviceurl, certificateFilePath, certificatePassword);
                //  break;
            }
        }
    }

    public static void repeatProcess() throws Exception {
        Scanner input = new Scanner(System.in);
        System.out.println("would u like to to continue(yes/no)");
        String nextop = input.next();
        if (nextop.toUpperCase().equals("Y")) {
            // Selectoperation();
        }
    }

    private static String getCertificateFilePath(String certificateFileName) throws UnsupportedEncodingException {
        String certificateFilePath;
        if (runningFromJar()) {
            File f = new File(System.getProperty("java.class.path"));
            File dir = f.getAbsoluteFile().getParentFile();
            String currentDirectory = dir.toString();
            Path path = Paths.get(currentDirectory, certificateFileName);
            certificateFilePath = path.toString();
        } else {
            certificateFilePath = MppgV4DemoApp.class.getResource(certificateFileName).getPath();
            certificateFilePath = URLDecoder.decode(certificateFilePath, "UTF-8");
        }
        return certificateFilePath;
    }

    private static String getConfigFilePath() throws UnsupportedEncodingException {
        String configFilePath;
        if (runningFromJar()) {
            File f = new File(System.getProperty("java.class.path"));
            File dir = f.getAbsoluteFile().getParentFile();
            String currentDirectory = dir.toString();
            Path path = Paths.get(currentDirectory, "config.properties");
            configFilePath = path.toString();
        } else {
            configFilePath = MppgV4DemoApp.class.getResource("config.properties").getPath();
            configFilePath = URLDecoder.decode(configFilePath, "UTF-8");
        }
        return configFilePath;
    }

    public static String getJarName() {
        return new File(MppgV4DemoApp.class.getProtectionDomain()
                .getCodeSource()
                .getLocation()
                .getPath())
                .getName();
    }

    public static boolean runningFromJar() {
        return getJarName().contains(".jar");
    }

}
