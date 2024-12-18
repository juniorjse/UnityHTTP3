package com.example.quicconnectionwrapper;

import android.content.Context;
import android.net.http.HttpEngine;
import android.net.http.QuicOptions;
import android.net.http.UrlRequest;
import android.os.Build;
import android.os.ext.SdkExtensions;
import java.io.IOException;
import java.net.URL;

public class QuicInstructions
{
    private static HttpEngine.Builder httpBuilder;
    private HttpEngine httpEngine;
    UrlRequest request;
    public String QuicAndroidConnect() throws InterruptedException {
        String out = "";
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
            try {
                setUpQuicConnection();
                httpEngine = httpBuilder.build();
                httpEngine.openConnection(new URL("http", "www.google.com", 443, "")).connect();
                out = ("CONEXÃO BEM SUCEDIDA");

            } catch (IOException e) {
                out = ("ERRO NA CONEXÃO " + e.getStackTrace());
            }
            catch (Exception e)
            {
                out = ("ERRO NA CONEXÃO " + e.getStackTrace());
            }

        }
        else{
            out = "device invalido";
        }

        return out;
    }
    public String QuicAndroidGet() throws InterruptedException {
        String output = "ERRO NO GET";

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
            DirectExecutor executor = new DirectExecutor();
            URLRequestCallBack urlRequestCB = new URLRequestCallBack();
            request = httpEngine.newUrlRequestBuilder("https://www.google.com", executor, urlRequestCB).build();
            QuicGet quicGet = new QuicGet(executor,request, urlRequestCB);
            quicGet.start();
            quicGet.wait();

            output = quicGet.getName();
        }

        return output;
    }
    private void setUpQuicConnection(App app)
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7)
        {

            this.httpBuilder = new HttpEngine.Builder(App.getContext());

            httpBuilder.setEnableQuic(true);
            httpBuilder.setEnableHttp2(false);
            httpBuilder.setEnableBrotli(false);

            QuicOptions.Builder quicBuilder = new QuicOptions.Builder();

            quicBuilder.addAllowedQuicHost("www.google.com");
            QuicOptions quicOptions = quicBuilder.build();
            httpBuilder.setQuicOptions(quicOptions);
            httpBuilder.addQuicHint("www.google.com", 443, 443);

            this.httpEngine = httpBuilder.build();
        }
    }



}
