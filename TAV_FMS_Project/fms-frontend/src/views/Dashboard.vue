<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue'; // nextTick eklendi
import api from '../services/api';
import * as echarts from 'echarts'; // ECharts eklendi

// 1. Veri Tipleri
interface ArchivedFlight {
  id: number;
  originalFlightId: string;
  flightNumber: string;
  archivedAt: string;
}

interface ActiveFlight {
  flightNumber: string;
  departureStation: string;
  arrivalStation: string;
  scheduleTime: string;
  status: string;
}

// 2. Hafıza (State) Değişkenleri
const archivedFlights = ref<ArchivedFlight[]>([]);
const activeFlights = ref<ActiveFlight[]>([]);
const loading = ref(true);
const error = ref('');


// Grafiklerin HTML içindeki referanslarını tutacağımız değişkenler
const airlineChartRef = ref(null);
const stationChartRef = ref(null);

// Yeni grafik referansları
const statusChartRef = ref(null);
const trendChartRef = ref(null);


// 3. Verileri Çeken Fonksiyon (Hem açılışta hem de butona basınca çalışır)
// 3. Verileri Çeken Fonksiyon (Gecikmeli ve güvenli yapı)
const fetchData = async () => {
  loading.value = true;
  error.value = '';

  // Yeni kayıt olan kullanıcıda token/yetki senkronizasyonunun oturması için minik bir nefes payı
  await new Promise(resolve => setTimeout(resolve, 300));

  try {
    const flightRes = await api.get('http://localhost:5204/api/Flight');
    activeFlights.value = flightRes.data;

    const archiveRes = await api.get('http://localhost:5221/api/Archive');
    archivedFlights.value = archiveRes.data;
  } catch (err: any) {
    // Eğer ilk istekte 403 gelirse, veritabanı yeni oluşan kullanıcıyı hazırlıyor demektir.
    // 1 saniye sonra tek seferlik sessiz bir deneme daha yapabiliriz:
    if (err.response && err.response.status === 403) {
      setTimeout(async () => {
        try {
          const flightRes = await api.get('http://localhost:5204/api/Flight');
          activeFlights.value = flightRes.data;
          const archiveRes = await api.get('http://localhost:5221/api/Archive');
          archivedFlights.value = archiveRes.data;
          error.value = '';
        } catch (e) {
          error.value = "Veriler çekilemedi. CORS hatası veya kapalı servis olabilir.";
        } finally {
          loading.value = false;
          nextTick(() => { renderCharts(); });
        }
      }, 1000);
      return;
    }

    error.value = "Veriler çekilemedi. CORS hatası veya kapalı servis olabilir.";
    console.error(err);
  } finally {
    if (!error.value) {
      loading.value = false;
      nextTick(() => {
        renderCharts();
      });
    }
  }
};

// Sayfa yüklendiğinde verileri getir
onMounted(() => {
  fetchData();
});

// 4. Statü Güncelleme (LANDED) Butonu Fonksiyonu
const markAsLanded = async (flight: any) => {
  const isConfirmed = confirm(`${flight.flightNumber} numaralı uçuşu LANDED (İndi) statüsüne almak istediğinize emin misiniz?`);
  if (!isConfirmed) return;

  try {
    await api.put(
      `http://localhost:5204/api/Flight/${flight.flightNumber}/status`,
      `"LANDED"`,
      { headers: { 'Content-Type': 'application/json' } }
    );

    alert(`${flight.flightNumber} başarıyla iniş yaptı! Kafka mesajı fırlatıldı.`);
    // Sayfayı yenilemek (F5) yerine sadece verileri tekrar çekiyoruz, çok daha şık!
    fetchData();
  } catch (error) {
    console.error("Statü güncellenirken hata:", error);
    alert("Uçuş güncellenemedi. C# tarafında veya Token yetkisinde sorun olabilir.");
  }
};

// 5. İstatistik Kartları İçin Hesaplamalar (Sadece Aktif Uçuşlar Üzerinden)
const totalFlights = computed(() => activeFlights.value.length);
const scheduledFlights = computed(() => activeFlights.value.filter(f => f.status === 'SCHEDULED').length);
// Yeni hali (Büyük/küçük harf fark etmeksizin yakalaması için toUpperCase ekliyoruz):
const delayedFlights = computed(() =>
  activeFlights.value.filter(f => f.status && f.status.toUpperCase() === 'DELAYED').length
);

// 6. Tarih ve Renk Yardımcıları
const formatDateTime = (dateString: string) => {
  if (!dateString) return '-';
  return new Date(dateString).toLocaleString('tr-TR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
};

const getStatusColor = (status: string) => {
  switch (status) {
    case 'SCHEDULED': return '#17a2b8';
    case 'DELAYED': return '#dc3545';
    case 'LANDED': return '#28a745';
    case 'BOARDING': return '#ffc107';
    default: return '#6c757d';
  }
};

// --- YENİ: CSV Dışa Aktarma Fonksiyonu ---
const exportToCSV = () => {
  if (activeFlights.value.length === 0) {
    alert("Dışa aktarılacak aktif uçuş bulunmuyor.");
    return;
  }

  // 1. Sütun Başlıklarına 'Havayolu' ekliyoruz
  const headers = ['Ucus No', 'Havayolu', 'Kalkis', 'Varis', 'Tarih', 'Durum'];

  // 2. Verileri satırlara çevirme
  const rows = activeFlights.value.map(flight => [
    flight.flightNumber,
    flight.airlineCode, // <-- YENİ EKLENEN SATIR
    flight.departureStation,
    flight.arrivalStation,
    formatDateTime(flight.scheduleTime),
    flight.status
  ]);



  // 3. Başlık ve satırları virgül ile birleştirme
  const csvContent = [
    headers.join(','),
    ...rows.map(row => row.join(','))
  ].join('\n');

  // 4. Excel'de Türkçe karakterlerin bozulmaması için BOM (\uFEFF) ekliyoruz
  const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });

  // 5. Görünmez bir link oluşturup indirmeyi tetikliyoruz
  const link = document.createElement('a');
  const url = URL.createObjectURL(blob);
  link.setAttribute('href', url);
  link.setAttribute('download', 'aktif_ucus_raporu.csv');
  link.style.visibility = 'hidden';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
};



// Grafikleri çizen fonksiyon
// Mevcut renderCharts fonksiyonunu tamamen şu şekilde değiştir:
const renderCharts = () => {
  // --- 1. Havayolu Dağılımı (Pasta Grafik) ---
  if (activeFlights.value.length > 0 && airlineChartRef.value) {
    const airlineCounts = {};
    activeFlights.value.forEach(f => {
      airlineCounts[f.airlineCode] = (airlineCounts[f.airlineCode] || 0) + 1;
    });
    const pieData = Object.keys(airlineCounts).map(key => ({ name: key, value: airlineCounts[key] }));

    echarts.init(airlineChartRef.value).setOption({
      title: { text: 'Havayolu Dağılımı', left: 'center' },
      tooltip: { trigger: 'item' },
      series: [{ type: 'pie', radius: '50%', data: pieData }]
    });
  }

  // --- 2. Kalkış İstasyonu Yoğunluğu (Çubuk Grafik) ---
  if (activeFlights.value.length > 0 && stationChartRef.value) {
    const stationCounts = {};
    activeFlights.value.forEach(f => {
      stationCounts[f.departureStation] = (stationCounts[f.departureStation] || 0) + 1;
    });

    echarts.init(stationChartRef.value).setOption({
      title: { text: 'Kalkış İstasyonu Yoğunluğu', left: 'center' },
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: Object.keys(stationCounts) },
      yAxis: { type: 'value' },
      series: [{ data: Object.values(stationCounts), type: 'bar', itemStyle: { color: '#17a2b8' } }]
    });
  }

  // --- 3. Statü Dağılımı (Halka Grafik - YENİ) ---
  if (statusChartRef.value) {
    // Hem aktif hem de arşivlenmiş (LANDED) uçuşları hesaplıyoruz
    const scheduled = activeFlights.value.filter(f => f.status === 'SCHEDULED').length;
    const delayed = activeFlights.value.filter(f => f.status === 'DELAYED').length;
    const landed = archivedFlights.value.length; // Arşivdekiler zaten LANDED olanlardır

    echarts.init(statusChartRef.value).setOption({
      title: { text: 'Operasyon Durum Dağılımı', left: 'center' },
      tooltip: { trigger: 'item' },
      color: ['#17a2b8', '#dc3545', '#28a745'], // Mavi, Kırmızı, Yeşil
      series: [{
        type: 'pie',
        radius: ['40%', '70%'], // İçini boşaltıp halka yapıyoruz
        data: [
          { value: scheduled, name: 'Planlı' },
          { value: delayed, name: 'Gecikmeli' },
          { value: landed, name: 'İniş Yaptı (Arşiv)' }
        ]
      }]
    });
  }

  // --- 4. Günlük İniş Trendi (Çizgi Grafik - YENİ) ---
  if (archivedFlights.value.length > 0 && trendChartRef.value) {
    // Arşivdeki uçuşları tarihlerine göre (sadece gün) grupluyoruz
    const dateCounts = {};
    archivedFlights.value.forEach(f => {
      const dateStr = new Date(f.archivedAt).toLocaleDateString('tr-TR');
      dateCounts[dateStr] = (dateCounts[dateStr] || 0) + 1;
    });

    echarts.init(trendChartRef.value).setOption({
      title: { text: 'Günlük İniş Yapan Uçuş Trendi', left: 'center' },
      tooltip: { trigger: 'axis' },
      xAxis: { type: 'category', data: Object.keys(dateCounts) },
      yAxis: { type: 'value' },
      series: [{
        data: Object.values(dateCounts),
        type: 'line',
        smooth: true, // Çizgiyi yumuşatır
        areaStyle: {}, // Altını boyar
        itemStyle: { color: '#004d99' }
      }]
    });
  }
};

</script>

<template>
  <main style="padding: 20px; background-color: #f4f6f9; min-height: calc(100vh - 60px); font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">

    <!-- Üst Başlık ve Yenile Butonu -->
    <div style="margin-bottom: 25px; display: flex; justify-content: space-between; align-items: center;">
      <div>
        <span style="color: #666; font-size: 14px;">TAV FMS / <b style="color: #004d99;">Dashboard</b></span>
        <h2 style="margin-top: 5px; color: #333;">Operasyonel Dashboard</h2>
      </div>
      <button @click="fetchData" style="padding: 8px 15px; background: #004d99; color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: bold;">
        🔄 Verileri Yenile
      </button>
    </div>

    <p v-if="loading" style="font-weight: bold; color: #ff9900;">Servislerden veriler bekleniyor...</p>
    <p v-else-if="error" style="color: #dc3545; font-weight: bold; background: #f8d7da; padding: 15px; border-radius: 4px;">{{ error }}</p>

    <div v-else>
      <!-- İSTATİSTİK KARTLARI -->
      <div style="display: flex; gap: 20px; margin-bottom: 30px;">
        <div style="flex: 1; background: white; padding: 20px; border-radius: 8px; border-left: 5px solid #004d99; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
          <h4 style="margin: 0; color: #666; font-size: 14px;">Toplam Uçuş (Aktif)</h4>
          <p style="margin: 10px 0 0 0; font-size: 28px; font-weight: bold; color: #333;">{{ totalFlights }}</p>
        </div>
        <div style="flex: 1; background: white; padding: 20px; border-radius: 8px; border-left: 5px solid #17a2b8; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
          <h4 style="margin: 0; color: #666; font-size: 14px;">Planlı (Scheduled)</h4>
          <p style="margin: 10px 0 0 0; font-size: 28px; font-weight: bold; color: #333;">{{ scheduledFlights }}</p>
        </div>
        <div style="flex: 1; background: white; padding: 20px; border-radius: 8px; border-left: 5px solid #dc3545; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
          <h4 style="margin: 0; color: #666; font-size: 14px;">Gecikmeli (Delayed)</h4>
          <p style="margin: 10px 0 0 0; font-size: 28px; font-weight: bold; color: #dc3545;">{{ delayedFlights }}</p>
        </div>
      </div>


      <!-- ECHARTS GRAFİK ALANLARI -->
      <!-- ECHARTS GRAFİK ALANLARI (2x2 Grid) -->
      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 30px;">

        <!-- 1. Pasta Grafik -->
        <div style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
          <div ref="airlineChartRef" style="width: 100%; height: 300px;"></div>
        </div>

        <!-- 2. Çubuk Grafik -->
        <div style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
          <div ref="stationChartRef" style="width: 100%; height: 300px;"></div>
        </div>

        <!-- 3. Halka Grafik (YENİ) -->
        <div style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
          <div ref="statusChartRef" style="width: 100%; height: 300px;"></div>
        </div>

        <!-- 4. Çizgi Grafik (YENİ) -->
        <div style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
          <div ref="trendChartRef" style="width: 100%; height: 300px;"></div>
        </div>

      </div>


      <!-- AKTİF UÇUŞLAR TABLOSU -->
      <div style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); margin-bottom: 30px;">
        <!-- Başlık ve İndirme Butonu Yanyana -->
        <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px; margin-bottom: 15px;">
          <h3 style="margin: 0; color: #28a745;">Aktif Uçuşlar (Flight Manager)</h3>

          <button @click="exportToCSV" style="background-color: #6c757d; color: white; border: none; padding: 8px 15px; cursor: pointer; font-weight: bold; border-radius: 4px; display: flex; align-items: center; gap: 5px;">
            📥 CSV Olarak İndir
          </button>
        </div>

        <table style="width: 100%; border-collapse: collapse; margin-top: 15px; text-align: left;">
          <thead>
          <tr style="background-color: #f8f9fa; border-bottom: 2px solid #dee2e6;">
            <th style="padding: 12px; font-weight: bold; color: #495057;">Uçuş No</th>
            <th style="padding: 12px; font-weight: bold; color: #495057;">Kalkış</th>
            <th style="padding: 12px; font-weight: bold; color: #495057;">Varış</th>
            <th style="padding: 12px; font-weight: bold; color: #495057;">Tarih</th>
            <th style="padding: 12px; font-weight: bold; color: #495057;">Durum</th>
            <th style="padding: 12px; font-weight: bold; color: #495057;">İşlem</th>
          </tr>
          </thead>
          <tbody>
          <tr v-for="flight in activeFlights" :key="flight.flightNumber" style="border-bottom: 1px solid #e9ecef;">
            <td style="padding: 12px; font-weight: bold;">{{ flight.flightNumber }}</td>
            <td style="padding: 12px;">{{ flight.departureStation }}</td>
            <td style="padding: 12px;">{{ flight.arrivalStation }}</td>
            <td style="padding: 12px;">{{ formatDateTime(flight.scheduleTime) }}</td>
            <td style="padding: 12px;">
                <span :style="{ backgroundColor: getStatusColor(flight.status.toUpperCase()), color: 'white', padding: '5px 10px', borderRadius: '15px', fontSize: '12px', fontWeight: 'bold' }">
                  {{ flight.status }}
                </span>
            </td>
            <td style="padding: 12px;">
              <button
                v-if="flight.status !== 'LANDED'"
                @click="markAsLanded(flight)"
                style="background-color: #ff9900; color: white; border: none; padding: 6px 12px; cursor: pointer; font-weight: bold; border-radius: 4px; font-size: 12px;"
              >
                ✈️ İniş Yap
              </button>
            </td>
          </tr>
          <tr v-if="activeFlights.length === 0">
            <td colspan="6" style="text-align: center; padding: 20px; color: #666;">Aktif uçuş bulunmuyor.</td>
          </tr>
          </tbody>
        </table>
      </div>

      <!-- ARŞİVLENMİŞ UÇUŞLAR TABLOSU -->
      <div style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.05);">
        <h3 style="margin-top: 0; color: #004d99; border-bottom: 2px solid #f0f0f0; padding-bottom: 10px;">Arşivlenmiş Uçuşlar (Archive Service)</h3>

        <table style="width: 100%; border-collapse: collapse; margin-top: 15px; text-align: left;">
          <thead>
          <tr style="background-color: #f8f9fa; border-bottom: 2px solid #dee2e6;">
            <th style="padding: 12px; font-weight: bold; color: #495057;">Arşiv ID</th>
            <th style="padding: 12px; font-weight: bold; color: #495057;">Uçuş Numarası</th>
            <th style="padding: 12px; font-weight: bold; color: #495057;">Orijinal ID</th>
            <th style="padding: 12px; font-weight: bold; color: #495057;">Arşivlenme Zamanı</th>
          </tr>
          </thead>
          <tbody>
          <tr v-for="archive in archivedFlights" :key="archive.id" style="border-bottom: 1px solid #e9ecef;">
            <td style="padding: 12px; color: #666;">#{{ archive.id }}</td>
            <td style="padding: 12px; font-weight: bold; color: #004d99;">{{ archive.flightNumber }}</td>
            <td style="padding: 12px; font-size: 13px; color: #888;">{{ archive.originalFlightId }}</td>
            <td style="padding: 12px;">{{ formatDateTime(archive.archivedAt) }}</td>
          </tr>
          <tr v-if="archivedFlights.length === 0">
            <td colspan="4" style="text-align: center; padding: 20px; color: #666;">Henüz arşivlenmiş uçuş bulunmuyor.</td>
          </tr>
          </tbody>
        </table>
      </div>

    </div>
  </main>
</template>
