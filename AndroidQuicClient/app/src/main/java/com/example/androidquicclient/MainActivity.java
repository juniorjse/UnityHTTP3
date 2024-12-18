package com.example.androidquicclient;

import android.net.http.UrlRequest;
import android.os.Build;
import android.os.Bundle;
import android.os.ext.SdkExtensions;
import android.view.View;
import android.widget.Button;
import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import android.net.http.QuicOptions;
import android.widget.TextView;
import java.io.IOException;
import java.net.URL;
import android.net.http.HttpEngine;
import android.net.http.HttpEngine.Builder;

public class MainActivity extends AppCompatActivity {
    private Button conBtn;
    private Button getBtn;
    private Button discBtn;
    private TextView text;
    private Builder httpBuilder;
    private HttpEngine httpEngine;
    UrlRequest request;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_main);

        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return insets;
        });

        conBtn = findViewById(R.id.BtnConexao);
        getBtn = findViewById(R.id.BtnGet);
        discBtn = findViewById(R.id.BtnDisconnect);
        text = findViewById(R.id.OutputTxt);

        conBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                new Thread(new Runnable() {
                    @Override
                    public void run() {
                        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
                            try {
                            setUpQuicConnection();
                            httpEngine = httpBuilder.build();
                            httpEngine.openConnection(new URL("http", "www.google.com", 443, "")).connect();
                            text.setText("CONEXÃO BEM SUCEDIDA");

                            } catch (IOException e) {
                                text.setText("ERRO NA CONEXÃO " + e.getMessage());
                            }
                            catch (Exception e)
                            {
                                text.setText("ERRO NA CONEXÃO " + e.getMessage());
                            }

                        }

                    }
                }).start();

            }
        });

        getBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                new Thread(new Runnable() {
                    @Override
                    public void run() {
                        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
                            try {

                                DirectExecutor executor = new DirectExecutor();
                                URLRequestCallBack urlRequestCB = new URLRequestCallBack(text);
                                request = httpEngine.newUrlRequestBuilder("https://www.google.com", executor, urlRequestCB).build();
                                request.start();

                            }
                            catch (Exception e)
                            {
                                text.setText("ERRO NA REQUEST " + e.getMessage());
                            }

                        }

                    }
                }).start();

            }
        });


        discBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                new Thread(new Runnable() {
                    @Override
                    public void run() {
                        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7) {
                            try {
                                if (request != null && request.isDone()) {
                                    httpEngine.shutdown();
                                    text.setText("CONEXÃO ENCERRADA");
                                }
                            }
                            catch (Exception e)
                            {
                                text.setText("ERRO AO ENCERRAR CONEXÃO");
                            }

                        }

                    }
                }).start();

            }
        });

    }
    private void setUpQuicConnection()
    {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R && SdkExtensions.getExtensionVersion(Build.VERSION_CODES.S) >= 7)
        {
        this.httpBuilder = new Builder(App.getContext());

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