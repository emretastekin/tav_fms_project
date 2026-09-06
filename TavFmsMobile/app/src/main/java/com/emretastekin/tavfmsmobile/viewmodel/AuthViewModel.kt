package com.emretastekin.tavfmsmobile.viewmodel

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.google.firebase.auth.FirebaseAuth
import kotlinx.coroutines.launch
// Not: Projendeki Retrofit / FmsApiService sınıfını buraya import etmelisin
// import com.emretastekin.tavfmsmobile.network.FmsApiService

import kotlinx.coroutines.delay // Bunu import etmeyi unutma

class AuthViewModel : ViewModel() {
    private val auth: FirebaseAuth = FirebaseAuth.getInstance()

    var email by mutableStateOf("")
    var password by mutableStateOf("")
    var errorMessage by mutableStateOf("")

    var isUserLoggedIn by mutableStateOf(auth.currentUser != null)
        private set

    // Backend'e senkronizasyon isteği atan fonksiyon (Aynı zamanda 403 hatasını kökten çözer)
    private fun syncUserWithBackend() {
        val user = auth.currentUser
        user?.getIdToken(true)?.addOnCompleteListener { task ->
            if (task.isSuccessful) {
                val token = task.result?.token
                if (token != null) {
                    // Retrofit servisi ile backend'e istek atıyoruz.
                    // AuthInterceptor header'a "Bearer <token>" bilgisini otomatik ekleyeceği için
                    // bu fonksiyon tetiklendiği an backend veritabanına kullanıcıyı kaydedecektir!
                    viewModelScope.launch {
                        try {
                            // Projendeki FmsApiService üzerinden myPermissions çağrısını yapıyoruz
                            com.emretastekin.tavfmsmobile.network.FmsApi.retrofitService.getMyPermissions()
                        } catch (e: Exception) {
                            // Ağ hatası vb. durumlar için loglanabilir
                            e.printStackTrace()
                        }
                    }
                }
            }
        }
    }
    fun login() {
        if (email.isNotEmpty() && password.isNotEmpty()) {
            auth.signInWithEmailAndPassword(email, password)
                .addOnCompleteListener { task ->
                    if (task.isSuccessful) {
                        isUserLoggedIn = true
                        errorMessage = ""

                        // GİRİŞ BAŞARILI: Veritabanı ile senkronize et
                        syncUserWithBackend()
                    } else {
                        errorMessage = task.exception?.message ?: "Giriş başarısız oldu."
                    }
                }
        } else {
            errorMessage = "Lütfen e-posta ve şifrenizi girin."
        }
    }

    fun register() {
        if (email.isNotEmpty() && password.isNotEmpty()) {
            auth.createUserWithEmailAndPassword(email, password)
                .addOnCompleteListener { task ->
                    if (task.isSuccessful) {
                        errorMessage = ""

                        // KRİTİK DÜZELTME: Veritabanının kullanıcıyı ve rolü kaydetmesine
                        // yarım saniye (500ms) süre tanıyoruz, ardından giriş yapıp ana ekrana atıyoruz:
                        viewModelScope.launch {
                            delay(500) // Backend'in kaydı tamamlaması için minik bir nefes alma süresi
                            syncUserWithBackend()
                            isUserLoggedIn = true // Ekranı şimdi ana sayfaya geçiriyoruz
                        }
                    } else {
                        errorMessage = task.exception?.message ?: "Kayıt işlemi başarısız."
                    }
                }
        } else {
            errorMessage = "Lütfen e-posta ve şifrenizi girin."
        }
    }

    fun logOut() {
        auth.signOut()
        isUserLoggedIn = false
        email = ""
        password = ""
        errorMessage = ""
    }
}