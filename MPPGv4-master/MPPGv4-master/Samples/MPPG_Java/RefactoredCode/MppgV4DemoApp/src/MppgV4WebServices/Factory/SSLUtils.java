package MppgV4WebServices.Factory;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.security.KeyManagementException;
import java.security.KeyStore;
import java.security.KeyStoreException;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;
import java.security.UnrecoverableKeyException;
import java.security.cert.CertificateException;
import javax.net.ssl.KeyManagerFactory;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLSocketFactory;

public class SSLUtils {

    public static SSLSocketFactory getSSLSocketFactory(File pKeyFile, String pKeyPassword) throws KeyStoreException, FileNotFoundException, IOException, NoSuchAlgorithmException, CertificateException, UnrecoverableKeyException, KeyManagementException {
        //KeyManagerFactory keyManagerFactory = KeyManagerFactory.getInstance("SunX509");
        KeyManagerFactory keyManagerFactory = KeyManagerFactory.getInstance(KeyManagerFactory.getDefaultAlgorithm());
        KeyStore keyStore = KeyStore.getInstance("PKCS12");

        try (InputStream keyInput = new FileInputStream(pKeyFile)) {
            keyStore.load(keyInput, pKeyPassword.toCharArray());
        }
        keyManagerFactory.init(keyStore, pKeyPassword.toCharArray());
        SSLContext context = SSLContext.getInstance("TLS");
        context.init(keyManagerFactory.getKeyManagers(), null, new SecureRandom());
        return context.getSocketFactory();
    }
}
