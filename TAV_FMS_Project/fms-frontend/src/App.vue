<template>
  <div style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">

    <!-- Üst Menü (Navbar) -->
    <nav
      v-if="route.path !== '/login' && route.path !== '/register'"
      style="background-color: #004d99; padding: 15px 30px; display: flex; justify-content: space-between; align-items: center;"
    >
      <!-- SOL TARAF: Sayfa Linkleri -->
      <div style="display: flex; gap: 20px; align-items: center;">

        <!-- Dashboard'u herkes görebilir -->
        <router-link to="/" style="color: white; text-decoration: none; font-weight: bold; font-size: 18px;">Dashboard</router-link>

        <!-- Vue Computed Policy ile temizlenmiş Uçuş Ekle butonu -->
        <router-link v-if="canManageFlights" to="/new-flight" style="color: white; text-decoration: none; font-weight: bold; font-size: 18px;">+ Uçuş Ekle</router-link>

        <!-- Vue Computed Policy ile temizlenmiş Referans Yönetimi butonu -->
        <router-link v-if="canManageReferences" to="/references" style="color: #ffcc00; text-decoration: none; font-weight: bold; margin-left: 15px;">
          ⚙️ Referans Yönetimi
        </router-link>

        <!-- Vue Computed Policy ile temizlenmiş Yetki (Rol) Yönetimi butonu -->
        <router-link v-if="canManageRoles" to="/role-management" style="color: #28a745; text-decoration: none; font-weight: bold; margin-left: 15px;">
          🛡️ Yetki Yönetimi
        </router-link>
      </div>

      <!-- SAĞ TARAF: Çıkış Butonu -->
      <div>
        <button @click="logout" style="background-color: #dc3545; color: white; border: none; padding: 8px 16px; border-radius: 4px; font-weight: bold; cursor: pointer;">
          Çıkış Yap
        </button>
      </div>
    </nav>

    <!-- Hangi linke tıklanırsa o sayfanın içeriği buraya yüklenecek -->
    <router-view />
  </div>
</template>


<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { auth } from './firebase.js'
import { signOut, onAuthStateChanged } from 'firebase/auth'
import axios from 'axios' // Axios'u import etmeyi unutma!

const route = useRoute()
const router = useRouter()

// Artık sadece tek bir rolü değil, tüm yetkilerin listesini tutuyoruz
const userPermissions = ref([])

// Buton gösterim kurallarını DİNAMİK İZİNLERE (Permission) bağlıyoruz
const canManageFlights = computed(() => userPermissions.value.includes('Flights.Create'))


// App.vue içindeki butonu gizleyip/gösteren kuralı değiştiriyoruz:
const canManageRoles = computed(() => userPermissions.value.includes('Roles.Manage'))

// Referans yönetimini görmek için havayolu veya istasyon ekleme/düzenleme yetkisi olması yeterli
const canManageReferences = computed(() =>
  userPermissions.value.includes('Airlines.Create') ||
  userPermissions.value.includes('Stations.Create')
)

// Sayfa yüklendiğinde kullanıcının yetkilerini backend'den çekiyoruz
onMounted(() => {
  onAuthStateChanged(auth, async (user) => {
    if (user) {
      try {
        // Firebase'den anlık token'ı alıyoruz
        const token = await user.getIdToken()

        // Backend'e gidip bu kullanıcının yapabileceği her şeyi (İzinleri) çekiyoruz
        const response = await axios.get('http://localhost:5204/api/Auth/my-permissions', {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });

        // Gelen yetki dizisini state'e atıyoruz (Örn: ['Flights.Read', 'Flights.Create'])
        userPermissions.value = response.data;
        console.log("Kullanıcının Sahip Olduğu Yetkiler:", userPermissions.value);

      } catch (error) {
        console.error("Yetkiler çekilirken hata oluştu:", error);
        userPermissions.value = []; // Hata anında güvenliği sağla, menüleri gizle
      }
    } else {
      userPermissions.value = [];
    }
  })
})

// Çıkış yapma fonksiyonu
const logout = async () => {
  try {
    await signOut(auth)
    userPermissions.value = [] // Çıkışta yetkileri sıfırla
    router.push('/login')
  } catch (error) {
    console.error("Çıkış yapılırken bir hata oluştu:", error)
  }
}
</script>


<style>
/* Tıklanan linkin rengini belirginleştirmek için ufak bir stil */
.router-link-active {
  text-decoration: underline !important;
  color: #ffcc00 !important;
}
</style>
