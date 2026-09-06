import { createApp } from 'vue'
import App from './App.vue'
import router from './router' // Router'ı içe aktardık

const app = createApp(App)

app.use(router) // Router'ı Vue uygulamasına bağladık
app.mount('#app')
