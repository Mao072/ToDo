<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '../api'

const account = ref('')
const password = ref('')
const confirmPassword = ref('')
const name = ref('')
const department = ref('')
const supervisor = ref(false)
const message = ref('')
const loading = ref(false)
const router = useRouter()

async function register() {
  message.value = ''
  if (!account.value || !password.value || !confirmPassword.value) {
    message.value = '請填寫所有欄位（包含確認密碼）'
    return
  }
  if (password.value !== confirmPassword.value) {
    message.value = '密碼與確認密碼不一致'
    return
  }
  loading.value = true
  try {
    const res = await api.post('/users/register', {
      account: account.value,
      password: password.value,
      name: name.value,
      department: department.value,
      supervisor: supervisor.value
    })
    //message.value = `Registered: ${res.data.account} (id=${res.data.id})`
    // optionally redirect to login
    window.alert('註冊成功，請使用新帳號登入')
    setTimeout(() => router.push('/login'), 700)
  } catch (err) {
    console.error(err)
    message.value = err?.response?.data || err.message || String(err)
  }
  finally { loading.value = false }
}
</script>

<template>
  <div class="home-root">
    <div class="hero">
      <div class="card">
        <h1 class="title">註冊</h1>

        <div class="form">
          <div class="form-group">
            <label style="color: #000000; font-weight: bold;">帳號</label>
            <input class="input" v-model="account" />
          </div>
          <div class="form-group">
            <label style="color: #000000; font-weight: bold;">密碼</label>
            <input class="input" type="password" v-model="password" />
          </div>
           <div class="form-group">
            <label style="color: #000000; font-weight: bold;">確認密碼</label>
            <input class="input" type="password" v-model="confirmPassword" />
          </div>
                    <div class="form-group">
            <label style="color: #000000; font-weight: bold;">名稱</label>
            <input class="input" v-model="name" />
          </div>
            <div class="actions" style="margin-top:12px">
            <label style="color: #000000; font-weight: bold;">部門</label>
            <select class="input" v-model="department" :disabled="loading">
              <option value="">請選擇部門</option>
              <option value="Engineering">Engineering</option>
              <option value="Design">Design</option>
              <option value="HR">HR</option>
            </select>
          </div>
          <div class="actions" style="margin-top:12px">
            <button class="btn" @click="register" :disabled="loading">註冊</button>
          </div>
          <div class="notyet"> 
            <p style="color: #000000; font-weight: bold;">已經註冊?</p>
            <p class="link" @click="router.push('/login')" style="color: #000000; font-weight: bold; cursor: pointer; " role="button" tabindex="0">登入</p>
          </div>

          <div v-if="message" class="message">{{ message }}</div>
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
/* Align labels and inputs in this form */
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
