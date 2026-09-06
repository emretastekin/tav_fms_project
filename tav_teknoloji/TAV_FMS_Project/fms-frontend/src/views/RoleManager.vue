<template>
  <div style="padding: 30px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
    <h2 style="color: #004d99; margin-bottom: 20px;">Yetki Yönetimi (RBAC)</h2>

    <!-- Rol Seçici -->
    <div style="margin-bottom: 30px;">
      <label style="font-weight: bold; margin-right: 15px;">Düzenlenecek Rolü Seçin:</label>
      <select v-model="selectedRoleId" @change="loadRolePermissions" style="padding: 10px; border-radius: 5px; border: 1px solid #ccc; width: 300px;">
        <option disabled value="">Rol Seçiniz...</option>
        <option v-for="role in roles" :key="role.id" :value="role.id">
          {{ role.name }} ({{ role.description }})
        </option>
      </select>
    </div>

    <!-- Yetki Grupları (Strapi Akordeon Görünümü) -->
    <div v-if="selectedRoleId">
      <div v-for="(permissionsList, groupName) in groupedPermissions" :key="groupName" style="margin-bottom: 20px; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;">

        <!-- Grup Başlığı -->
        <div style="background-color: #f8f9fa; padding: 15px 20px; border-bottom: 1px solid #e0e0e0; display: flex; justify-content: space-between; align-items: center;">
          <h4 style="margin: 0; color: #333; text-transform: capitalize;">{{ groupName }} Modülü</h4>
        </div>

        <!-- Checkbox Listesi -->
        <div style="padding: 20px; display: flex; gap: 30px; flex-wrap: wrap; background-color: white;">
          <label v-for="perm in permissionsList" :key="perm.id" style="display: flex; align-items: center; gap: 8px; cursor: pointer;">
            <!-- Vue v-model doğrudan diziye bağlandığında otomatik ekle/çıkar yapar -->
            <input
              type="checkbox"
              :value="perm.id"
              v-model="selectedPermissionIds"
              style="width: 18px; height: 18px; cursor: pointer;"
            />
            <span style="font-size: 15px; color: #555;">{{ perm.action }}</span>
          </label>
        </div>
      </div>

      <!-- Kaydet Butonu -->
      <div style="margin-top: 30px;">
        <button @click="savePermissions" style="background-color: #28a745; color: white; border: none; padding: 12px 24px; border-radius: 5px; font-weight: bold; font-size: 16px; cursor: pointer;">
          Değişiklikleri Kaydet
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import { auth } from '../firebase.js' // Firebase token için

const roles = ref([])
const allPermissions = ref([])
const selectedRoleId = ref('')
const selectedPermissionIds = ref([]) // Seçilen checkbox'ların ID'lerini tutar

// Sayfa yüklendiğinde rolleri ve tüm yetkileri çek
onMounted(async () => {
  try {
    const user = auth.currentUser;
    const token = await user.getIdToken();
    const config = { headers: { Authorization: `Bearer ${token}` } };

    const [rolesRes, permsRes] = await Promise.all([
      axios.get('http://localhost:5204/api/Auth/roles', config),
      axios.get('http://localhost:5204/api/Auth/permissions', config)
    ]);

    roles.value = rolesRes.data;
    allPermissions.value = permsRes.data;
  } catch (error) {
    console.error("Veriler yüklenemedi", error);
  }
})

// Düz permission listesini Strapi gibi kategorilere ayır (Örn: Flights.Create -> { Flights: [{ action: 'Create', id: '...' }] })
const groupedPermissions = computed(() => {
  const groups = {};

  allPermissions.value.forEach(p => {
    // Code alanını noktadan böl (Örn: "Flights.Read" -> parts[0] = "Flights", parts[1] = "Read")
    const parts = p.code.split('.');
    const groupName = parts[0];
    const actionName = parts[1] || p.code;

    if (!groups[groupName]) {
      groups[groupName] = [];
    }

    groups[groupName].push({
      id: p.id,
      action: actionName.toLowerCase(), // create, read, update, delete
      fullCode: p.code
    });
  });

  return groups;
})

// Seçilen rol değiştiğinde o rolün mevcut yetkilerini veritabanından çek
const loadRolePermissions = async () => {
  if (!selectedRoleId.value) return;

  try {
    const user = auth.currentUser;
    const token = await user.getIdToken();
    const res = await axios.get(`http://localhost:5204/api/Auth/roles/${selectedRoleId.value}/permissions`, {
      headers: { Authorization: `Bearer ${token}` }
    });

    // Gelen ID'leri doğrudan v-model'e bağlı diziye atıyoruz, checkbox'lar otomatik işaretleniyor
    selectedPermissionIds.value = res.data;
  } catch (error) {
    console.error("Rol yetkileri çekilemedi", error);
  }
}

// Kaydet butonuna basıldığında
const savePermissions = async () => {
  try {
    const user = auth.currentUser;
    const token = await user.getIdToken();

    await axios.post(`http://localhost:5204/api/Auth/roles/${selectedRoleId.value}/permissions`,
      selectedPermissionIds.value, // Sadece seçili ID'lerin dizisini yolluyoruz
      { headers: { Authorization: `Bearer ${token}` } }
    );

    alert("Yetkiler başarıyla güncellendi!");
  } catch (error) {
    console.error("Kaydetme hatası", error);
    alert("Kaydederken bir hata oluştu.");
  }
}
</script>
