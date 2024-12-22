package com.example.quicconnectionwrapper;

import android.content.Context;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.net.http.HttpEngine;
import android.net.http.QuicOptions;
import android.net.http.UrlRequest;
import android.os.Build;
import android.os.ext.SdkExtensions;
import android.util.Log;
import java.net.URL;
import java.util.Arrays;

public class QuicInstructions
{
    private static HttpEngine.Builder httpBuilder;
    private static HttpEngine httpEngine;
    private static URLRequestCallBack urlRequestCB;
    private UrlRequest request;
    public String QuicAndroidConnect(Context context) throws InterruptedException {
        String out = "";
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
            try {
                setUpQuicConnection(context);
                httpEngine.openConnection(new URL("http", "www.google.com", 443, "")).connect();
                out = ("CONEXÃO BEM SUCEDIDA");
                Log.i("Conexao Quic", "CONEXAO BEM SUCEDIDA");

            }
            catch (Exception e)
            {
                out = ("ERRO NA CONEXÃO " + e.getStackTrace());
                Log.e("Conexao Quic", "ERRO NA CONEXAO");
            }

        }
        else{
            out = "Dispositivo Invalido";
            Log.e("Conexao Quic", "DISPOSITIVO NAO SUPORTA CONEXAO");
        }

        return out;
    }
    public String QuicAndroidGet() throws InterruptedException
    {
        String output = "ERRO NO GET";

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7
                && httpEngine != null) {
            try
            {
                DirectExecutor executor = new DirectExecutor();
                this.urlRequestCB = new URLRequestCallBack();
                this.request = httpEngine.newUrlRequestBuilder("https://www.google.com", executor, urlRequestCB).build();
                Log.i("Request", "Inicio do Request");
                request.start();
                while (!request.isDone() || urlRequestCB.getResponse().length() < 15) {}
                output = this.urlRequestCB.getResponse();
            }
            catch (Exception ex)
            {
                output = Arrays.toString(ex.getStackTrace());
            }
        }

        return output;
    }

    public void QuicAndroidDisconnect()
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
            this.httpEngine.shutdown();
        }
    }
    public void setUpQuicConnection(Context context)
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7)
        {
            try {
                Log.i("Quic Settigns", "Criando HttpBuilder");
                this.httpBuilder = new HttpEngine.Builder(context);
                this.httpBuilder.setEnableQuic(true);
                this.httpBuilder.setEnableHttp2(true);
                this.httpBuilder.setEnableBrotli(true);

                Log.i("Quic Settigns", "Criando QuicOptions");
                QuicOptions.Builder quicBuilder = new QuicOptions.Builder();

                quicBuilder.addAllowedQuicHost("www.google.com");
                QuicOptions quicOptions = quicBuilder.build();
                this.httpBuilder.setQuicOptions(quicOptions);
                this.httpBuilder.addQuicHint("www.google.com", 443, 443);

                if (isNetworkAvailable(context))
                {
                    this.httpEngine = httpBuilder.build();
                    Log.i("Quic Settigns", "HTTPENGINE BUILDADO COM SUCESSO");
                }
                else {
                    Log.e("Quic Settigns","REDE INDISPONIVEL");
                }

            }
            catch (Exception ex)
            {
                Log.e("Quic Settigns","UMA EXCECAO ACONTECEU");
            }

        }
    }

    public boolean isNetworkAvailable(Context context) {
        ConnectivityManager cm = (ConnectivityManager) context.getSystemService(Context.CONNECTIVITY_SERVICE);
        NetworkInfo networkInfo = cm.getActiveNetworkInfo();
        return networkInfo != null && networkInfo.isConnected();
    }



}
