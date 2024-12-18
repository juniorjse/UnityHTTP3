package com.example.quicconnectionwrapper;

import android.net.http.HttpEngine;
import android.os.Build;
import android.os.ext.SdkExtensions;

import java.io.IOException;
import java.net.URL;

public class QuicConnect extends Thread
{
    private static String outputThred;
    private HttpEngine httpEngine;

    public QuicConnect(HttpEngine httpEngine)
    {
        this.httpEngine = httpEngine;
    }

    @Override
    public void run() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
            try {
                httpEngine.openConnection(new URL("http", "www.google.com", 443, "")).connect();
                outputThred = "CONEXÃO BEM SUCEDIDA";
            } catch (IOException e) {
                throw new RuntimeException(e);
            }
        }

    }

    public static String getOutput()
    {
        return outputThred;
    }

}
