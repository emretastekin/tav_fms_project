package com.emretastekin.tavfmsmobile.network

import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object ApiClient {

    // AuthInterceptor'ı (güvenlik görevlisini) Client'a ekliyoruz
    private val client = OkHttpClient.Builder()
        .addInterceptor(AuthInterceptor())
        .build()

    // C# API'si ile konuşacak olan Retrofit nesnesi
    val retrofit: Retrofit by lazy {
        Retrofit.Builder()
            // ÖNEMLİ: Android emülatörü kendi içinde çalıştığı için localhost
            // yerine 10.0.2.2 adresinden bilgisayarının portuna erişir.
            .baseUrl("http://10.0.2.2:5204/api/")
            .client(client)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }
}