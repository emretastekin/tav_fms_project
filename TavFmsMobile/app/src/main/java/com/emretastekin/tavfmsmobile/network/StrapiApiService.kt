package com.emretastekin.tavfmsmobile.network

import com.emretastekin.tavfmsmobile.model.StrapiResponse
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.GET

// DİKKAT: Strapi 1337 portunda çalışıyor
private const val STRAPI_BASE_URL = "http://10.0.2.2:1337/api/"

private val retrofit = Retrofit.Builder()
    .addConverterFactory(GsonConverterFactory.create())
    .baseUrl(STRAPI_BASE_URL)
    .build()

interface StrapiApiService {
    // Strapi'de oluşturduğumuz tabloya GET isteği atıyoruz
    @GET("terminal-points")
    suspend fun getTerminalPoints(): StrapiResponse
}

object StrapiApi {
    val retrofitService: StrapiApiService by lazy {
        retrofit.create(StrapiApiService::class.java)
    }
}