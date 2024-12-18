package com.example.quicconnectionwrapper;
import android.net.http.HeaderBlock;
import android.net.http.HttpEngine;
import android.net.http.HttpException;
import android.net.http.UrlRequest;
import android.net.http.UrlResponseInfo;
import android.os.Build;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.RequiresExtension;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.Map;

@RequiresExtension(extension = Build.VERSION_CODES.S, version = 7)
public class URLRequestCallBack implements UrlRequest.Callback {

    public  String requestStatus;
    private final StringBuilder responseBody = new StringBuilder();

    public URLRequestCallBack()
    {
        requestStatus = "VAZIO";
    }


    @Override
    public void onRedirectReceived(@NonNull UrlRequest urlRequest, @NonNull UrlResponseInfo urlResponseInfo, @NonNull String s) throws Exception {
        requestStatus = ("REDIRECT RECEIVED");
        urlRequest.followRedirect();
    }

    @Override
    public void onResponseStarted(@NonNull UrlRequest urlRequest, @NonNull UrlResponseInfo urlResponseInfo) throws Exception {
        requestStatus = ("RESPONSE STARTED");
        urlRequest.read(ByteBuffer.allocateDirect(102400));
    }

    @Override
    public void onReadCompleted(@NonNull UrlRequest urlRequest, @NonNull UrlResponseInfo urlResponseInfo, @NonNull ByteBuffer byteBuffer) throws Exception {
        requestStatus = ("READ COMPLETED");
        //byteBuffer.clear();
        //urlRequest.read(byteBuffer);
        byteBuffer.flip();
        byte[] data = new byte[byteBuffer.remaining()];
        byteBuffer.get(data);
        responseBody.append(new String(data, StandardCharsets.UTF_8));
        byteBuffer.clear();
        urlRequest.read(byteBuffer);
    }

    @Override
    public void onSucceeded(@NonNull UrlRequest urlRequest, @NonNull UrlResponseInfo urlResponseInfo) {

                    requestStatus = ("SUCESSO NA OPERAÇÃO\n" +
                    "STATUS CODE: " + urlResponseInfo.getHttpStatusCode() + "\n" +
                    "PROTOCOL: " + urlResponseInfo.getNegotiatedProtocol() + "\n" +
                                    "RESPONSE BODY: " + requestStatus
                    );
    }

    @Override
    public void onFailed(@NonNull UrlRequest urlRequest, @Nullable UrlResponseInfo urlResponseInfo, @NonNull HttpException e) {
        requestStatus = ("FALHA\n" + e.getMessage());
    }

    @Override
    public void onCanceled(@NonNull UrlRequest urlRequest, @Nullable UrlResponseInfo urlResponseInfo) {
        requestStatus = ("CANCELADO\n" + urlResponseInfo.getHttpStatusText());
    }

    public String getResponse()
    {
        return this.requestStatus;
    }


}
