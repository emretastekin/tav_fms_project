package com.emretastekin.tavfmsmobile.viewmodel

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.emretastekin.tavfmsmobile.model.TerminalPoint
import com.emretastekin.tavfmsmobile.network.StrapiApi
import kotlinx.coroutines.launch

class TerminalViewModel : ViewModel() {
    // Arayüzün dinleyeceği mekanlar listesi state'i
    var terminalList: List<TerminalPoint> by mutableStateOf(emptyList())
        private set

    // Olası bağlantı hatalarını tutacağımız state
    var errorMessage: String by mutableStateOf("")
        private set

    init {
        // ViewModel oluştuğunda verileri otomatik çek
        getTerminalPoints()
    }

    private fun getTerminalPoints() {
        viewModelScope.launch {
            try {
                // Strapi'den gelen "data" objesinin içindeki listeyi alıyoruz
                val response = StrapiApi.retrofitService.getTerminalPoints()
                terminalList = response.data
            } catch (e: Exception) {
                errorMessage = e.message ?: "Mekanlar yüklenirken bir hata oluştu."
            }
        }
    }
}