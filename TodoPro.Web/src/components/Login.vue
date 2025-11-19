<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api, { setAuthToken } from '../api'
import axios from 'axios';

const router = useRouter()
const account = ref('')
const password = ref('')
const message = ref('')
const loading = ref(false)
const error = ref('');
async function handleLogin() {
    error.value = '';
    try {
        // 呼叫後端 API
        const response = await axios.post('/api/users/login', {
            account: account.value,
            password: password.value
        });

        const token = response.data.token;

        if (token) {

          localStorage.setItem('authToken', token);
          window.alert("登入成功！");
          router.replace('/main');
        } else {
            error.value = '登入失敗：後端未返回 Token';
        }

    } catch (err) {
        console.error("Login Error:", err);
        error.value = '登入失敗：帳號或密碼錯誤。';
    }
}
</script>

<template>
  <div class="home-root">
    <div class="hero">
      <div class="card">
        <h1 class="title">Login</h1>

        <div class="form">
          <div class="form-group">
            <label style="color: #000000; font-weight: bold;">帳號</label>
            <input @keyup.enter="handleLogin" class="input" v-model="account" />
          </div>
          <div class="form-group">
            <label style="color: #000000; font-weight: bold;">密碼</label>
            <input @keyup.enter="handleLogin" class="input" type="password" v-model="password" />
          </div>
          <div class="actions" style="margin-top:12px">
            <button class="btn" @keyup.enter="handleLogin" @click="handleLogin" :disabled="loading">登入</button>
          </div>
          <div class="notyet"> 
            <p style="color: #000000; font-weight: bold;">尚未註冊?</p>
            <p class="link" @click="router.push('/register')" style="color: #000000; font-weight: bold; cursor: pointer; " role="button" tabindex="0">註冊</p>
          </div>

          <div v-if="error" class="message">{{ error }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .notyet .link:hover { text-decoration: underline; }
.notyet{
  display: flex;
  justify-content: center;
}
.form{
  /* center the form inside the card and limit width for better layout */
  margin: 20px auto 0;
  width: 100%;
  max-width: 420px;
}
.form-group {
  display: block;
  margin-top: 12px;
}
/* Align labels and inputs to match Register.vue (label above input) */
.form-group label { display: block; text-align: left; margin-bottom: 6px; }
.form .input, .form select { width: 100%; }
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
.input { width:50%; padding:10px 12px; border-radius:8px; border:1px solid rgba(255,255,255,0.12); background: rgba(0,0,0,0.15); color: #fff }
.input:focus { outline:none; box-shadow: 0 6px 18px rgba(0,0,0,0.25); }
@media (max-width: 640px) {
  .card { padding: 28px 18px; }
  .title { font-size: 2.4rem; }
  .actions { flex-direction: column; gap: 10px; }
  .btn { width: 100%; min-width: auto; }
}
</style>