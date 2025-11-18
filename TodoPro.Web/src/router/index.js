import { createRouter, createWebHistory } from 'vue-router'
import HelloWorld from '../components/HelloWorld.vue'
import Login from '../components/Login.vue'
import Register from '../components/Register.vue'

const routes = [
  { path: '/', name: 'home', component: HelloWorld },
  { path: '/login', name: 'login', component: Login },
  { path: '/register', name: 'register', component: Register }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
