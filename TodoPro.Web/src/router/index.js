import { createRouter, createWebHistory } from 'vue-router'
import HelloWorld from '../components/HelloWorld.vue'
import Login from '../components/Login.vue'
import Register from '../components/Register.vue'
import Main from '../components/Main.vue'
const routes = [
    { path: '/', name: 'home', component: HelloWorld },
    { path: '/login', name: 'login', component: Login },
    { path: '/register', name: 'register', component: Register },
    { 
        path: '/main', 
        name: 'main', 
        component: Main,
        // *** 新增：明確指出此頁面需要認證 ***
        meta: { requiresAuth: true } 
    },
    { 
        path: '/todo/:id', 
        name: 'todo-detail-layout', 
        component: Main, // 讓 Main.vue 處理佈局
        meta: { requiresAuth: true } 
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})
// router/index.js (修正後的 beforeEach)

router.beforeEach((to, from, next) => {
    const isAuthenticated = localStorage.getItem('authToken');

    // 情況 A: 用戶已登入，但嘗試訪問登入頁
    if (isAuthenticated && (to.path === '/login' || to.path === '/register')) {
        // *** 修正：導航到您的主要應用頁面 /main ***
        next({ path: '/main', replace: true }); 
        return;
    }

    // 情況 B: 頁面需要認證 (例如 /main)，但用戶未登入
    // 注意：需要確保 /main 已經設定了 meta: { requiresAuth: true }
    const requiresAuth = to.matched.some(record => record.meta && record.meta.requiresAuth);
    if (requiresAuth && !isAuthenticated) {
        next('/login');
        return;
    }

    // 情況 C: 其他情況 (例如訪問 /, /login (未登入), /register (未登入))
    next();
});

export default router