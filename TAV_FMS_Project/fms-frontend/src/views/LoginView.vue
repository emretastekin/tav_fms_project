<template>
  <div class="login-wrapper">
    <h2>TAV FMS Web Paneli</h2>

    <!-- DİKKAT: <form @submit.prevent="login"> yerine normal bir <div> kullanıyoruz -->
    <div>
      <div class="input-group">
        <label>E-posta Adresi</label>
        <!-- Form kullanmadığımız için 'required' özellikleri tarayıcı tarafından zorlanmaz,
             gerekirse login fonksiyonun içinde boş mu diye kontrol edebilirsin -->
        <input type="email" v-model="email" />
      </div>

      <div class="input-group">
        <label>Şifre</label>
        <input type="password" v-model="password" />
      </div>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>


      <!-- DİKKAT: type="submit" SİLİNDİ, @click="login" EKLENDİ -->
      <div>
        <button @click="login" class="primary-btn">Giriş Yap</button>
      </div>
    </div>

    <button @click="goToRegister" class="text-btn">
      Sistemde hesabınız yok mu? Kayıt Olun
    </button>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { auth } from '../firebase'
import { signInWithEmailAndPassword } from 'firebase/auth'
import { useRouter } from 'vue-router'

const email = ref('')
const password = ref('')
const errorMessage = ref('')
const router = useRouter()

const login = async () => {
  try {
    await signInWithEmailAndPassword(auth, email.value, password.value)
    errorMessage.value = ''
    router.push('/') // Başarılı girişte ana sayfaya atar
  } catch (error) {
    errorMessage.value = "Giriş başarısız: E-posta veya şifre hatalı."
  }
}

const goToRegister = () => {
  router.push('/register') // Kayıt olmak isteyenleri yeni sayfaya yönlendirir
}
</script>

<style scoped>
.login-wrapper { max-width: 400px; margin: 100px auto; padding: 2rem; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); font-family: sans-serif; }
.input-group { margin-bottom: 1rem; }
input { width: 100%; padding: 8px; margin-top: 4px; }
.primary-btn { width: 100%; padding: 10px; background-color: #003366; color: white; border: none; cursor: pointer; }
.text-btn { background: none; border: none; color: #003366; text-decoration: underline; cursor: pointer; margin-top: 1rem; width: 100%; }
.error-text { color: red; font-size: 0.9rem; }
</style>
