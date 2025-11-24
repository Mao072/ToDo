<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()

function goLogin() {
  router.push('/login')
}

function goRegister() {
  router.push('/register')
}
</script>

<template>
  <div class="home-root">
    <div class="hero">
      <div class="card">
        <h1 class="title">To-Do</h1>

        <div class="actions">
          <button class="btn" @click="goLogin">登入</button>
          <button class="btn" @click="goRegister">註冊</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.home-root {
   position: fixed;
   inset: 0;
   display: flex;
   align-items: center;
   justify-content: center;
   background: linear-gradient(180deg, #f9cfb6 0%, #f8aa66 100%); 
   margin: 0;
}
.hero {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
}
.card {
  max-width: 760px;
  width: calc(100% - 48px);
  background: rgba(255,255,255,0.06); /* subtle translucent card */
  backdrop-filter: blur(6px);
  padding: 48px 36px;
  border-radius: 14px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.25);
  text-align: center;
}
.title {
  font-family: sans-serif ;
  font-size: 3.5rem;
  margin: 0 0 8px 0;
  color: #000000;
  -webkit-text-stroke: 1.6px #000000; /* slightly thinner stroke */
  text-shadow: 0 4px 18px rgba(0,0,0,0.35);
}

.actions {
  display: flex;
  gap: 14px;
  justify-content: center;
  margin-top: 8px;
}
.btn {
  padding: 12px 22px;
  font-size: 1rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all .18s ease;
  min-width: 110px;
  /* unified primary style */
  background: #ffffff;
  color: #ff7f11;
  border: none;
  font-weight: 600;
}
.btn:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(0,0,0,0.18); }

@media (max-width: 640px) {
  .card { padding: 28px 18px; }
  .title { font-size: 2.4rem; }
  .actions { flex-direction: column; gap: 10px; }
  .btn { width: 100%; min-width: auto; }
}
</style>
