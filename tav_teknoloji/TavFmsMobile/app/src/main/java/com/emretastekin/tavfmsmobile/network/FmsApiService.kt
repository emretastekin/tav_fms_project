package com.emretastekin.tavfmsmobile.network

import com.emretastekin.tavfmsmobile.model.Flight
import retrofit2.http.GET

interface FmsApiService {
    @GET("Flight")
    suspend fun getFlights(): List<Flight>

    // İŞTE BURAYA BU FONKSİYONU EKLİYORUZ:
    @GET("Auth/my-permissions")
    suspend fun getMyPermissions(): List<String>
}

object FmsApi {
    val retrofitService: FmsApiService by lazy {
        ApiClient.retrofit.create(FmsApiService::class.java)
    }
}