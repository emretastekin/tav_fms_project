package com.emretastekin.tavfmsmobile

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.ui.Modifier
import com.emretastekin.tavfmsmobile.ui.LoginScreen
import com.emretastekin.tavfmsmobile.ui.MainScreen
import com.emretastekin.tavfmsmobile.ui.theme.TavFmsMobileTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            TavFmsMobileTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    // ViewModel'imizi tanımlıyoruz
                    val authViewModel: com.emretastekin.tavfmsmobile.viewmodel.AuthViewModel = androidx.lifecycle.viewmodel.compose.viewModel()

                    // Kullanıcı oturum açmış mı diye kontrol ediyoruz
                    if (authViewModel.isUserLoggedIn) {
                        // Çıkış yapma fonksiyonunu MainScreen'e parametre olarak yolluyoruz
                        MainScreen(onLogOut = { authViewModel.logOut() })
                    } else {
                        LoginScreen(authViewModel)
                    }
                }
            }

        }
    }
}

