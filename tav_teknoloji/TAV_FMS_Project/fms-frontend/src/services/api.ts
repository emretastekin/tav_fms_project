import axios from 'axios'
import { auth } from '../firebase.js' // Firebase konfigürasyon dosyanın yolu

// Axios için temel ayarları oluşturuyoruz
const api = axios.create({
  // Buraya C# FlightManager servisinin çalıştığı temel URL'i yazmalısın
  // Örnek: baseURL: 'http://localhost:5204/api',
  baseURL: 'http://localhost:5204/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

// INTERCEPTOR: Giden her HTTP isteğini havada yakalar ve araya girer
api.interceptors.request.use(async (config) => {
  // Sistemde giriş yapmış bir kullanıcı var mı kontrol et
  const user = auth.currentUser

  if (user) {
    // Kullanıcının anlık, taze Firebase JWT Token'ını al
    const token = await user.getIdToken()

    // İstek başlığına (Headers) bu token'ı yetki anahtarı olarak ekle
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
}, (error) => {
  return Promise.reject(error)
})

export default api
