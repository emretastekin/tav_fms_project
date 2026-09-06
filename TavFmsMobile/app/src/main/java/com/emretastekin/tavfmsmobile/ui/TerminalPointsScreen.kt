package com.emretastekin.tavfmsmobile.ui

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.emretastekin.tavfmsmobile.model.TerminalPoint
import com.emretastekin.tavfmsmobile.viewmodel.TerminalViewModel

@Composable
fun TerminalPointsScreen(viewModel: TerminalViewModel = androidx.lifecycle.viewmodel.compose.viewModel()) {
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text(
            text = "Terminal Noktaları",
            fontSize = 24.sp,
            fontWeight = FontWeight.Bold,
            color = Color(0xFF003366),
            modifier = Modifier.padding(bottom = 16.dp, top = 32.dp)
        )

        // Hata varsa ekranda göster
        if (viewModel.errorMessage.isNotEmpty()) {
            Text(text = "Bağlantı Hatası: ${viewModel.errorMessage}", color = Color.Red)
        } else {
            // Mekanları performanslı bir şekilde liste halinde göster
            LazyColumn(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                items(viewModel.terminalList) { point ->
                    TerminalCard(point)
                }
            }
        }
    }
}

@Composable
fun TerminalCard(point: TerminalPoint) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp),
        colors = CardDefaults.cardColors(containerColor = Color.White)
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                Text(text = point.Name, fontWeight = FontWeight.Bold, fontSize = 20.sp)

                // Aktiflik durumuna göre renk belirleme
                Text(
                    text = if (point.IsActive) "Aktif" else "Pasif",
                    color = if (point.IsActive) Color(0xFF28a745) else Color.Red,
                    fontWeight = FontWeight.Bold
                )
            }
            Spacer(modifier = Modifier.height(8.dp))
            Text(text = "Tür: ${point.Type}", fontSize = 16.sp)
            Text(text = "Kat: ${point.Floor}  |  Koordinat: ${point.CoordinateX}, ${point.CoordinateY}", color = Color.Gray, fontSize = 14.sp)
        }
    }
}