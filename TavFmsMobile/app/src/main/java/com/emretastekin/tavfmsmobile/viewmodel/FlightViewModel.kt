package com.emretastekin.tavfmsmobile.viewmodel

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.emretastekin.tavfmsmobile.model.Flight
import com.emretastekin.tavfmsmobile.network.FmsApi
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

class FlightViewModel : ViewModel() {
    var flightList: List<Flight> by mutableStateOf(emptyList())
        private set

    var errorMessage: String by mutableStateOf("")
        private set

    init {
        // ViewModel ilk oluştuğunda eski düz fonksiyon yerine
        // 403 korumalı ve gecikmeli olan fetchFlights'ı çağırıyoruz!
        fetchFlights()
    }

    // Artık tek bir ana fonksiyonumuz var (İstersen adını getFlights de yapabilirsin)
    fun fetchFlights() {
        viewModelScope.launch {
            errorMessage = ""

            // 1. Yeni kullanıcı girişlerinde token senkronizasyonunun oturması için minik gecikme
            delay(400)

            try {
                val response = FmsApi.retrofitService.getFlights()
                flightList = response
            } catch (e: Exception) {
                // 2. İlk istekte 403 gelirse 1 saniye sonra sessizce tekrar dene
                if (e.message?.contains("403") == true || e.toString().contains("403")) {
                    delay(1000)
                    try {
                        val retryResponse = FmsApi.retrofitService.getFlights()
                        flightList = retryResponse
                        errorMessage = ""
                        return@launch
                    } catch (retryException: Exception) {
                        // İkinci deneme de başarısız olursa hatayı göster
                    }
                }
                errorMessage = e.localizedMessage ?: "Bağlantı Hatası: HTTP 403 Forbidden"
            }
        }
    }
}