<template>
  <div class="login-wrapper">
    <h2>TAV FMS - Yeni Kayıt</h2>

    <form @submit.prevent="register">
      <div class="input-group">
        <label>E-posta Adresi</label>
        <input type="email" v-model="email" required />
      </div>

      <div class="input-group">
        <label>Şifre</label>
        <input type="password" v-model="password" required />
      </div>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>

      <button type="submit" class="primary-btn">Kayıt Ol</button>
    </form>

    <button @click="goToLogin" class="text-btn">
      Zaten hesabınız var mı? Giriş Yapın
    </button>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { auth } from '../firebase.js'
import { createUserWithEmailAndPassword } from 'firebase/auth'
import { useRouter } from 'vue-router'

const email = ref('')
const password = ref('')
const errorMessage = ref('')
const router = useRouter()

const register = async () => {
  try {
    await createUserWithEmailAndPassword(auth, email.value, password.value)
    errorMessage.value = ''
    router.push('/') // Başarılı kayıtta ana sayfaya yönlendir
  } catch (error) {
    errorMessage.value = "Kayıt başarısız: " + error.message
  }
}

const goToLogin = () => {
  router.push('/login') // Login sayfasına geri dön
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
