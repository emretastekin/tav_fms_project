package com.emretastekin.tavfmsmobile.model

// Strapi'nin en dıştaki JSON yapısını karşılayacak sarmalayıcı sınıf
data class StrapiResponse(
    val data: List<TerminalPoint>
)

// Strapi'den gelecek olan asıl mekan verilerimiz
data class TerminalPoint(
    val id: Int,
    val documentId: String?,
    val Name: String,
    val Type: String,
    val Floor: Int,
    val CoordinateX: Double,
    val CoordinateY: Double,
    val IsActive: Boolean
)
