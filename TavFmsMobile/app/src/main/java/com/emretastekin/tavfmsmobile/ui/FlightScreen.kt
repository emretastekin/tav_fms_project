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
import com.emretastekin.tavfmsmobile.model.Flight
import com.emretastekin.tavfmsmobile.viewmodel.FlightViewModel

@Composable
fun FlightScreen(viewModel: FlightViewModel = androidx.lifecycle.viewmodel.compose.viewModel()) {
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text(
            text = "Aktif Uçuşlar",
            fontSize = 24.sp,
            fontWeight = FontWeight.Bold,
            color = Color(0xFF003366),
            modifier = Modifier.padding(bottom = 16.dp, top = 32.dp)
        )

        // Hata varsa ekranda göster
        if (viewModel.errorMessage.isNotEmpty()) {
            Text(text = "Bağlantı Hatası: ${viewModel.errorMessage}", color = Color.Red)
        } else {
            // Uçuşları performanslı bir şekilde kaydırılabilir liste (LazyColumn) ile göster
            LazyColumn(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                items(viewModel.flightList) { flight ->
                    FlightCard(flight)
                }
            }
        }
    }
}

@Composable
fun FlightCard(flight: Flight) {
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
                Text(text = flight.flightNumber, fontWeight = FontWeight.Bold, fontSize = 20.sp)

                // Statüye göre renk belirleme (LANDED ise yeşil, değilse turuncu)
                Text(
                    text = flight.status,
                    color = if (flight.status == "LANDED") Color(0xFF28a745) else Color(0xFFff9900),
                    fontWeight = FontWeight.Bold
                )
            }
            Spacer(modifier = Modifier.height(8.dp))
            Text(text = "Rota: ${flight.departureStation} ➔ ${flight.arrivalStation}", fontSize = 16.sp)
            Text(text = "Zaman: ${flight.scheduleTime}", color = Color.Gray, fontSize = 14.sp)
        }
    }
}