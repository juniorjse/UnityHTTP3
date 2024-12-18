package com.example.quicconnectionwrapper;

import android.net.http.UrlRequest;
import android.os.Build;
import android.os.ext.SdkExtensions;

public class QuicGet extends Thread{

    private UrlRequest urlRequest;
    private URLRequestCallBack urlCB;
    private DirectExecutor executor;

    private String output;

    public QuicGet(DirectExecutor executor, UrlRequest urlRequest, URLRequestCallBack urlCB)
    {
        this.executor = executor;
        this.urlCB = urlCB;
        this.urlRequest = urlRequest;
    }

    public void setOutput(String newOutput)
    {
        this.output = newOutput;
    }

    @Override
    public void run() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
            try
            {
                urlRequest.start();
                urlRequest.wait();
                output = urlCB.getResponse();
            }
            catch (Exception ex)
            {
                output = "FALHOU O INICIO DA REQUEST";
            }

        }
    }
}
