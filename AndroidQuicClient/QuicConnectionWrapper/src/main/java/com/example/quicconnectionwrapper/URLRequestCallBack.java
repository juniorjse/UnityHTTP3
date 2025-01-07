package com.example.quicconnectionwrapper;
import android.net.http.HttpException;
import android.net.http.UrlRequest;
import android.net.http.UrlResponseInfo;
import android.os.Build;
import android.util.Log;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.RequiresExtension;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;

@RequiresExtension(extension = Build.VERSION_CODES.S, version = 7)
public class URLRequestCallBack implements UrlRequest.Callback {

    public  static String requestStatus;

    public static boolean isFinish;
    private final StringBuilder responseBody = new StringBuilder();

    public URLRequestCallBack()
    {
        requestStatus = "VAZIO";
        isFinish = false;
    }


    @Override
    public void onRedirectReceived(@NonNull UrlRequest urlRequest, @NonNull UrlResponseInfo urlResponseInfo, @NonNull String s) throws Exception {
        requestStatus = ("REDIRECT RECEIVED");
        Log.i("URL REQUEST CALLBACK", "REDIRECT RECEIVED");
        urlRequest.followRedirect();
    }

    @Override
    public void onResponseStarted(@NonNull UrlRequest urlRequest, @NonNull UrlResponseInfo urlResponseInfo) throws Exception {
        requestStatus = ("RESPONSE STARTED");
        urlRequest.read(ByteBuffer.allocateDirect(102400));
        Log.i("URL REQUEST CALLBACK", "RESPONSE STARTED");
    }

    @Override
    public void onReadCompleted(@NonNull UrlRequest urlRequest, @NonNull UrlResponseInfo urlResponseInfo, @NonNull ByteBuffer byteBuffer) throws Exception {
        requestStatus = ("READ COMPLETED");
        Log.i("URL REQUEST CALLBACK", "READ COMPLETED");
        byteBuffer.flip();
        byte[] data = new byte[byteBuffer.remaining()];
        byteBuffer.get(data);
        responseBody.append(new String(data, StandardCharsets.UTF_8));
        byteBuffer.clear();
        urlRequest.read(byteBuffer);
    }

    @Override
    public void onSucceeded(@NonNull UrlRequest urlRequest, @NonNull UrlResponseInfo urlResponseInfo) {
                    Log.i("URL REQUEST CALLBACK", "REQUEST SUCESS - GETTING RESPONSE");
                    requestStatus = ("SUCESSO NA OPERAÇÃO\n" +
                    "STATUS CODE: " + urlResponseInfo.getHttpStatusCode() + "\n" +
                    "PROTOCOL: " + urlResponseInfo.getNegotiatedProtocol() + "\n" +
                                    "RESPONSE BODY: " + responseBody
                    );
                    isFinish = true;
    }

    @Override
    public void onFailed(@NonNull UrlRequest urlRequest, @Nullable UrlResponseInfo urlResponseInfo, @NonNull HttpException e) {
        requestStatus = ("FALHA\n             " + e.getMessage());
        Log.e("URL REQUEST CALLBACK", "REQUEST FAIL");
    }

    @Override
    public void onCanceled(@NonNull UrlRequest urlRequest, @Nullable UrlResponseInfo urlResponseInfo) {
        requestStatus = ("CANCELADO\n                          " + urlResponseInfo.getHttpStatusText());
        Log.e("URL REQUEST CALLBACK", "REQUEST CANCEL");
    }

    public String getResponse()
    {
        return this.requestStatus;
    }
    public boolean getIsfinish()
    {
        return isFinish;
    }


}
