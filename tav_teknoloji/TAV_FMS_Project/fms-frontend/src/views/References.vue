<template>
  <div style="padding: 20px; max-width: 800px; margin: 0 auto;">
    <h2 style="color: #003366; margin-bottom: 20px;">Referans Veri Yönetimi</h2>

    <div style="display: flex; gap: 20px;">

      <!-- Havayolu Ekleme Paneli -->
      <div style="flex: 1; border: 1px solid #ddd; padding: 20px; border-radius: 8px;">
        <h3 style="color: #0055a4; margin-top: 0;">✈️ Havayolu Ekle</h3>
        <div style="display: flex; flex-direction: column; gap: 10px;">
          <input v-model="airlineForm.code" placeholder="Kod (Örn: THY)" style="padding: 8px;" />
          <input v-model="airlineForm.name" placeholder="Adı (Örn: Turkish Airlines)" style="padding: 8px;" />
          <input v-model="airlineForm.country" placeholder="Ülke (Örn: Türkiye)" style="padding: 8px;" />
          <button @click="saveAirline" style="background-color: #28a745; color: white; border: none; padding: 10px; cursor: pointer;">Kaydet</button>
        </div>
      </div>

      <!-- İstasyon Ekleme Paneli -->
      <div style="flex: 1; border: 1px solid #ddd; padding: 20px; border-radius: 8px;">
        <h3 style="color: #0055a4; margin-top: 0;">🏢 İstasyon Ekle</h3>
        <div style="display: flex; flex-direction: column; gap: 10px;">
          <input v-model="stationForm.code" placeholder="Kod (Örn: LTFM)" style="padding: 8px;" />
          <input v-model="stationForm.name" placeholder="Adı (Örn: İstanbul Havalimanı)" style="padding: 8px;" />
          <input v-model="stationForm.city" placeholder="Şehir (Örn: İstanbul)" style="padding: 8px;" />
          <button @click="saveStation" style="background-color: #007bff; color: white; border: none; padding: 10px; cursor: pointer;">Kaydet</button>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive } from 'vue';
import api from '../services/api'

// Form state'leri
const airlineForm = reactive({ code: '', name: '', country: '' });
const stationForm = reactive({ code: '', name: '', city: '' });

// POST İstekleri (Port: 5183 -> Reference Manager)
const saveAirline = async () => {
  try {
    await api.post('http://localhost:5183/api/Airline', airlineForm);
    alert('Havayolu başarıyla eklendi! Kafka tetiklendi.');
    airlineForm.code = ''; airlineForm.name = ''; airlineForm.country = '';
  } catch (error) {
    console.error(error);
    alert('Havayolu eklenirken hata oluştu.');
  }
};

const saveStation = async () => {
  try {
    await api.post('http://localhost:5183/api/Station', stationForm);
    alert('İstasyon başarıyla eklendi! Kafka tetiklendi.');
    stationForm.code = ''; stationForm.name = ''; stationForm.city = '';
  } catch (error) {
    console.error(error);
    alert('İstasyon eklenirken hata oluştu.');
  }
};
</script>
