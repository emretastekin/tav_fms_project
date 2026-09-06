package com.emretastekin.tavfmsmobile.ui

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material.icons.filled.ExitToApp
import androidx.compose.material.icons.filled.Flight
import androidx.compose.material.icons.filled.Place
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainScreen(onLogOut: () -> Unit = {}) {
    val navController = rememberNavController()
    var selectedItem by remember { mutableIntStateOf(0) }
    val items = listOf("Uçuşlar", "Mekanlar")
    val icons = listOf(Icons.Filled.Flight, Icons.Filled.Place)
    val routes = listOf("flights", "terminals")

    Scaffold(
        // Ekranın en üstüne bir TopAppBar (Üst Çubuk) ekliyoruz
        topBar = {
            TopAppBar(
                title = { Text("TAV FMS", fontWeight = FontWeight.Bold) },
                actions = {
                    IconButton(onClick = onLogOut) {
                        Icon(Icons.Filled.ExitToApp, contentDescription = "Çıkış Yap")
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.surfaceVariant
                )
            )
        },
        bottomBar = {
            NavigationBar(containerColor = MaterialTheme.colorScheme.surfaceVariant) {
                items.forEachIndexed { index, item ->
                    NavigationBarItem(
                        icon = { Icon(icons[index], contentDescription = item) },
                        label = { Text(item) },
                        selected = selectedItem == index,
                        onClick = {
                            selectedItem = index
                            navController.navigate(routes[index]) {
                                popUpTo(navController.graph.startDestinationId) { saveState = true }
                                launchSingleTop = true
                                restoreState = true
                            }
                        }
                    )
                }
            }
        }
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = "flights",
            modifier = Modifier.padding(innerPadding)
        ) {
            composable("flights") { FlightScreen() }
            composable("terminals") { TerminalPointsScreen() }
        }
    }
}