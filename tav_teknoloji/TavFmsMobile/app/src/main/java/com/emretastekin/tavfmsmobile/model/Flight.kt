package com.emretastekin.tavfmsmobile.model

data class Flight(
    val id: Int,
    val flightNumber: String,
    val departureStation: String,
    val arrivalStation: String,
    val scheduleTime: String,
    val status: String
)
