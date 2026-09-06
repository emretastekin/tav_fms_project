package com.emretastekin.tavfmsmobile.network

import com.google.android.gms.tasks.Tasks
import com.google.firebase.auth.FirebaseAuth
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val requestBuilder = chain.request().newBuilder()

        // Firebase'den mevcut kullanıcıyı alıyoruz
        val currentUser = FirebaseAuth.getInstance().currentUser

        if (currentUser != null) {
            try {
                // Token'ı arka planda (senkron olarak) çekiyoruz
                val task = currentUser.getIdToken(false)
                val tokenResult = Tasks.await(task)
                val token = tokenResult.token

                // Token başarıyla alındıysa başlığa (Header) ekliyoruz
                if (token != null) {
                    requestBuilder.addHeader("Authorization", "Bearer $token")
                }
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }

        return chain.proceed(requestBuilder.build())
    }
}