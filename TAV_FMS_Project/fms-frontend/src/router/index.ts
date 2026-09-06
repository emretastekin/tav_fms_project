import { createRouter, createWebHistory } from 'vue-router'
import axios from 'axios' // Axios'u import ediyoruz
import RoleManager from '../views/RoleManager.vue'
import Dashboard from '../views/Dashboard.vue'
import LoginView from '../views/LoginView.vue'
import NewFlight from '../views/NewFlight.vue'
import References from '../views/References.vue'
import RegisterView from '../views/RegisterView.vue'

import { auth } from '../firebase.js'
import { onAuthStateChanged } from 'firebase/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/',
      name: 'dashboard',
      component: Dashboard,
      meta: { requiresAuth: true }
    },
    {
      path: '/new-flight',
      name: 'newFlight',
      component: NewFlight,
      meta: {
        requiresAuth: true,
        requiredPermission: 'Flights.Create'
      }
    },
    {
      path: '/references',
      name: 'references',
      component: References,
      meta: {
        requiresAuth: true,
        requiredPermission: ['Airlines.Create', 'Stations.Create'] // Dizi desteği
      }
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView
    },
    {
      path: '/role-management',
      name: 'RoleManagement',
      component: RoleManager,
      meta: { requiresAuth: true, requiredPermission: 'Roles.Manage' }
    }
  ]
})

const getCurrentUser = () => {
  return new Promise((resolve, reject) => {
    const unsubscribe = onAuthStateChanged(auth, user => {
      unsubscribe()
      resolve(user)
    }, reject)
  })
}

// --- KUSURSUZ İZİN KONTROTLU ROUTER GUARD ---
router.beforeEach(async (to, from, next) => {
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const user = await getCurrentUser()

  if (requiresAuth && !user) {
    next('/login')
  } else if ((to.path === '/login' || to.path === '/register') && user) {
    next('/')
  } else if (requiresAuth && user) {
    const requiredPermission = to.meta.requiredPermission;

    // Eğer sayfa özel bir izin gerektiriyorsa
    if (requiredPermission) {
      try {
        // 1. Firebase Token al
        const token = await user.getIdToken();

        // 2. Doğrudan Backend'den kullanıcının güncel izinlerini çek
        const response = await axios.get('http://localhost:5204/api/Auth/my-permissions', {
          headers: { Authorization: `Bearer ${token}` }
        });

        const userPermissions = response.data || []; // Backend'den gelen dizi (örn: ['Airlines.Create', ...])

        // 3. İzinlerin dizi mi string mi olduğunu kontrol et
        const permissionsArray = Array.isArray(requiredPermission) ? requiredPermission : [requiredPermission];

        // 4. Kullanıcının listede istenen izinlerden en az birine sahip olup olmadığını denetle
        const hasPermission = permissionsArray.some(permission => userPermissions.includes(permission));

        console.log(`[Router Guard] Sayfa: ${to.path} | Gerekli:`, permissionsArray, `Sahip Olunanlar:`, userPermissions, `Sonuç:`, hasPermission);

        if (!hasPermission) {
          console.warn(`Yetkisiz Erişim! Bu sayfa için gerekli izinlere sahip değilsiniz.`);
          next('/'); // Anasayfaya at
          return;
        }
      } catch (error) {
        console.error("Yetki kontrolü sırasında hata oluştu:", error);
        next('/');
        return;
      }
    }
    next();
  } else {
    next()
  }
})

export default router
