package com.emretastekin.tavfmsmobile.ui

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.emretastekin.tavfmsmobile.viewmodel.AuthViewModel

@Composable
fun LoginScreen(viewModel: AuthViewModel) {
    // Ekranın Giriş mi yoksa Kayıt modunda mı olduğunu tutan state
    var isLoginMode by remember { mutableStateOf(true) }

    Column(
        modifier = Modifier.fillMaxSize().padding(16.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        // Başlık moda göre değişiyor
        Text(
            text = if (isLoginMode) "TAV FMS'e Hoş Geldiniz" else "TAV FMS - Yeni Kayıt",
            fontSize = 24.sp,
            fontWeight = FontWeight.Bold,
            color = Color(0xFF003366),
            modifier = Modifier.padding(bottom = 32.dp)
        )

        OutlinedTextField(
            value = viewModel.email,
            onValueChange = { viewModel.email = it },
            label = { Text("E-posta Adresi") },
            modifier = Modifier.fillMaxWidth()
        )

        Spacer(modifier = Modifier.height(8.dp))

        OutlinedTextField(
            value = viewModel.password,
            onValueChange = { viewModel.password = it },
            label = { Text("Şifre") },
            visualTransformation = PasswordVisualTransformation(),
            modifier = Modifier.fillMaxWidth()
        )

        if (viewModel.errorMessage.isNotEmpty()) {
            Text(text = viewModel.errorMessage, color = Color.Red, modifier = Modifier.padding(top = 8.dp))
        }

        Spacer(modifier = Modifier.height(24.dp))

        // Buton ve fonksiyon moda göre değişiyor
        Button(
            onClick = {
                if (isLoginMode) viewModel.login() else viewModel.register()
            },
            modifier = Modifier.fillMaxWidth().height(50.dp)
        ) {
            Text(if (isLoginMode) "Giriş Yap" else "Kayıt Ol", fontSize = 18.sp)
        }

        Spacer(modifier = Modifier.height(8.dp))

        // Modu değiştiren alt buton
        TextButton(onClick = {
            isLoginMode = !isLoginMode
            viewModel.errorMessage = "" // Ekran değişirken eski hataları temizle
        }) {
            Text(if (isLoginMode) "Sistemde hesabınız yok mu? Kayıt Olun" else "Zaten hesabınız var mı? Giriş Yapın")
        }
    }
}