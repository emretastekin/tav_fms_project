<script setup lang="ts">
import { reactive, ref } from 'vue'; // 'ref' eklendi
import api from '../services/api'
import { useRouter } from 'vue-router';

const router = useRouter();

// --- 1. MANUEL UÇUŞ EKLEME DEĞİŞKENLERİ VE METODU ---
const flightForm = reactive({
  flightNumber: '',
  airline: '',
  aircraftType: '',
  origin: '',
  destination: '',
  flightDate: '',
  std: '',
  sta: '',
  flightType: 'Passenger'
});

const saveFlight = async () => {
  try {
    const payload = {
      flightNumber: flightForm.flightNumber,
      airlineCode: flightForm.airline,
      departureStation: flightForm.origin,
      arrivalStation: flightForm.destination,
      scheduleTime: `${flightForm.flightDate}T${flightForm.std}:00`,
      status: 'SCHEDULED'
    };

    await api.post('http://localhost:5204/api/Flight', payload);

    alert("Uçuş başarıyla eklendi ve Kafka'ya gönderildi!");
    router.push('/');

  } catch (error: any) {
    console.error("Kaydetme hatası:", error);
    alert("Uçuş kaydedilirken bir hata oluştu. Konsolu (F12) kontrol et.");
  }
};

// --- 2. TOPLU UÇUŞ YÜKLEME (CSV) DEĞİŞKENLERİ VE METODU ---
const selectedFile = ref<File | null>(null);
const uploadResult = ref<any>(null);

const onFileChange = (event: any) => {
  selectedFile.value = event.target.files[0];
};

const uploadBulkFile = async () => {
  if (!selectedFile.value) return;

  const formData = new FormData();
  formData.append('file', selectedFile.value);

  try {
    // Kendi api servisini kullanıyoruz, token otomatik eklenecektir
    const response = await api.post('http://localhost:5204/api/Flight/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });

    uploadResult.value = response.data;

    // Eğer hiç hata yoksa kullanıcıyı bilgilendir
    if (response.data.errors && response.data.errors.length === 0) {
      alert(`${response.data.message}\nUçuşlar başarıyla yüklendi!`);
    }
  } catch (error: any) {
    console.error("Toplu yükleme hatası:", error);
    uploadResult.value = {
      message: "Dosya yüklenirken sunucu ile iletişim kurulamadı.",
      errors: [error.response?.data || "Bilinmeyen bir hata oluştu."]
    };
  }
};
</script>

<template>
  <div style="padding: 20px; background-color: #f4f6f9; min-height: calc(100vh - 60px); padding-bottom: 80px;">

    <div style="margin-bottom: 20px;">
      <span style="color: #666; font-size: 14px;">Dashboard / Uçuşlar / <b style="color: #004d99;">Yeni</b></span>
      <h2 style="margin-top: 5px; color: #333;">Uçuş Ekle</h2>
    </div>

    <!-- ÜST BÖLÜM: MANUEL KAYIT FORMLARI -->
    <div style="display: flex; gap: 20px; flex-wrap: wrap;">
      <!-- UÇUŞ BİLGİSİ KARTI -->
      <div style="flex: 2; min-width: 300px; background: white; padding: 25px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05);">
        <h3 style="border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; margin-top: 0; color: #004d99;">Uçuş Bilgisi</h3>

        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-top: 20px;">
          <div style="display: flex; flex-direction: column;">
            <label style="font-weight: bold; margin-bottom: 5px;">Uçuş Numarası *</label>
            <input v-model="flightForm.flightNumber" type="text" placeholder="Örn: TK1903" style="padding: 10px; border: 1px solid #ccc; border-radius: 4px;" />
          </div>

          <div style="display: flex; flex-direction: column;">
            <label style="font-weight: bold; margin-bottom: 5px;">Havayolu *</label>
            <select v-model="flightForm.airline" style="padding: 10px; border: 1px solid #ccc; border-radius: 4px;">
              <option value="">Seçiniz...</option>
              <option value="THY">Turkish Airlines (THY)</option>
              <option value="KLM">KLM Royal Dutch Airlines (KLM)</option>
            </select>
          </div>

          <div style="display: flex; flex-direction: column;">
            <label style="font-weight: bold; margin-bottom: 5px;">Kalkış (Origin) *</label>
            <input v-model="flightForm.origin" type="text" maxlength="4" placeholder="Örn: AMS" style="padding: 10px; border: 1px solid #ccc; border-radius: 4px;" />
          </div>

          <div style="display: flex; flex-direction: column;">
            <label style="font-weight: bold; margin-bottom: 5px;">Varış (Destination) *</label>
            <input v-model="flightForm.destination" type="text" maxlength="4" placeholder="Örn: JFK" style="padding: 10px; border: 1px solid #ccc; border-radius: 4px;" />
          </div>

          <div style="display: flex; flex-direction: column; grid-column: span 2;">
            <label style="font-weight: bold; margin-bottom: 5px;">Uçak Tipi *</label>
            <select v-model="flightForm.aircraftType" style="padding: 10px; border: 1px solid #ccc; border-radius: 4px;">
              <option value="">Seçiniz...</option>
              <option value="B738">Boeing 737-800</option>
              <option value="A320">Airbus A320</option>
              <option value="A333">Airbus A330-300</option>
            </select>
          </div>
        </div>
      </div>

      <!-- ZAMAN & TÜR BİLGİSİ KARTI -->
      <div style="flex: 1; min-width: 250px; background: white; padding: 25px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05);">
        <h3 style="border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; margin-top: 0; color: #004d99;">Zaman & Tür Bilgisi</h3>

        <div style="display: flex; flex-direction: column; gap: 15px; margin-top: 20px;">
          <div style="display: flex; flex-direction: column;">
            <label style="font-weight: bold; margin-bottom: 5px;">Uçuş Tarihi *</label>
            <input v-model="flightForm.flightDate" type="date" style="padding: 10px; border: 1px solid #ccc; border-radius: 4px;" />
          </div>

          <div style="display: flex; gap: 10px;">
            <div style="display: flex; flex-direction: column; flex: 1;">
              <label style="font-weight: bold; margin-bottom: 5px;">STD (Kalkış) *</label>
              <input v-model="flightForm.std" type="time" style="padding: 10px; border: 1px solid #ccc; border-radius: 4px;" />
            </div>
            <div style="display: flex; flex-direction: column; flex: 1;">
              <label style="font-weight: bold; margin-bottom: 5px;">STA (Varış) *</label>
              <input v-model="flightForm.sta" type="time" style="padding: 10px; border: 1px solid #ccc; border-radius: 4px;" />
            </div>
          </div>

          <div style="display: flex; flex-direction: column; margin-top: 10px;">
            <label style="font-weight: bold; margin-bottom: 10px;">Uçuş Tipi *</label>
            <div style="display: flex; gap: 15px;">
              <label><input type="radio" v-model="flightForm.flightType" value="Passenger" /> Yolcu</label>
              <label><input type="radio" v-model="flightForm.flightType" value="Cargo" /> Kargo</label>
              <label><input type="radio" v-model="flightForm.flightType" value="Position" /> Pozisyon</label>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ALT BÖLÜM: TOPLU UÇUŞ YÜKLEME KARTI -->
    <div style="margin-top: 20px; background: white; padding: 25px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05);">
      <h3 style="border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; margin-top: 0; color: #004d99;">Dosya (CSV) İle Toplu Kayıt</h3>

      <div style="display: flex; gap: 15px; align-items: center; margin-top: 20px;">
        <input type="file" @change="onFileChange" accept=".csv" style="padding: 8px; border: 1px solid #ccc; border-radius: 4px; flex: 1; max-width: 400px;" />

        <button @click="uploadBulkFile" :disabled="!selectedFile" style="padding: 10px 25px; border: none; background: #17a2b8; color: white; cursor: pointer; border-radius: 4px; font-weight: bold;" :style="{ opacity: !selectedFile ? 0.6 : 1 }">
          Yükle ve Kaydet
        </button>
      </div>

      <!-- API'den dönen sonuç ve hataları gösteren alan -->
      <div v-if="uploadResult" style="margin-top: 20px; padding: 15px; border-radius: 4px; background-color: #f8f9fa; border-left: 4px solid #17a2b8;">
        <p style="font-weight: bold; margin: 0 0 10px 0; color: #333;">{{ uploadResult.message }}</p>

        <!-- Eğer hatalı satırlar varsa kırmızı listele -->
        <ul v-if="uploadResult.errors && uploadResult.errors.length > 0" style="margin: 0; padding-left: 20px; color: #dc3545; font-size: 14px;">
          <li v-for="(error, index) in uploadResult.errors" :key="index" style="margin-bottom: 5px;">
            {{ error }}
          </li>
        </ul>
      </div>
    </div>

    <!-- STICKY FOOTER (Sadece manuel form için Kaydet butonu) -->
    <div style="position: fixed; bottom: 0; left: 0; right: 0; background: white; padding: 15px 30px; border-top: 1px solid #ddd; display: flex; justify-content: flex-end; gap: 15px; box-shadow: 0 -4px 12px rgba(0,0,0,0.05);">
      <router-link to="/" style="padding: 10px 25px; border: 1px solid #ccc; background: white; color: #333; text-decoration: none; border-radius: 4px; font-weight: bold;">İptal</router-link>
      <button @click="saveFlight" style="padding: 10px 25px; border: none; background: #28a745; color: white; cursor: pointer; border-radius: 4px; font-weight: bold;">Manuel Formu Kaydet (Ctrl + S)</button>
    </div>

  </div>
</template>
