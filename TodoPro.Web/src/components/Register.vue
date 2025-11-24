<script setup>
import { ref, onMounted } from 'vue' // 引入 onMounted
import { useRouter } from 'vue-router'
import api from '../api'

const account = ref('')
const password = ref('')
const confirmPassword = ref('')
const name = ref('')
// 修正：用於儲存部門列表 (Array)
const departments = ref([]) 
// NEW：用於儲存用戶選擇的部門 ID (Number)
const departmentId = ref(null) 
const supervisor = ref(false)
const message = ref('')
const loading = ref(false)
const router = useRouter()

// *** 關鍵修正：在組件加載時獲取部門列表 ***
onMounted(() => {
    getDepartments();
});

function getDepartments()
{
    // API 存取部門列表不需要 Token，因為註冊頁是公開的
    // 但是，如果您後端 DepartmentsController 要求 [Authorize]，您需要先登入一個帳號才能測試
    api.get('/departments')
    .then(response => {
        // 確保接收到的是 Array
        departments.value = response.data; 
    })
    .catch(error => {
        console.error('Failed to fetch departments:', error);
        message.value = '無法載入部門列表，請檢查伺服器。';
    });
}

// 修正：將 register 改為非同步
async function register() {
    message.value = ''
    if (!account.value || !password.value || !confirmPassword.value) {
        message.value = '請填寫所有必填欄位。'
        return
    }
    if (password.value !== confirmPassword.value) {
        message.value = '密碼與確認密碼不一致'
        return
    }
    // NEW: 可選：檢查是否選擇了部門
    if (!departmentId.value) {
         message.value = '請選擇您的部門。';
         return;
    }

    loading.value = true
    try {
        const res = await api.post('/users/register', {
            account: account.value,
            password: password.value,
            name: name.value,
            // *** 關鍵修正：傳遞 departmentId 而非 department 字串 ***
            departmentId: departmentId.value,
            supervisor: supervisor.value
        })
        window.alert('註冊成功！請登入您的帳號。')
        setTimeout(() => router.push('/login'), 500)
    } catch (err) {
        console.error(err)
        message.value = err?.response?.data || err.message || '註冊失敗。'
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
          
          <!-- *** 修正：部門選擇區塊 *** -->
          <div class="form-group"> 
            <label style="color: #000000; font-weight: bold;">部門</label>
            <!-- 綁定到 departmentId -->
            <select class="input" v-model="departmentId" :disabled="loading || departments.length === 0">
              <option :value="null" disabled>請選擇部門</option>
              <!-- 遍歷 departments 列表 -->
              <option v-for="dept in departments" :key="dept.id" :value="dept.id">
                {{ dept.name }}
              </option>
            </select>
            <p v-if="departments.length === 0 && !loading" style="color: #00000088; font-size: 0.8rem; margin-top: 5px;">
                正在載入部門，或伺服器未提供部門列表。
            </p>
          </div>
          <!-- *** 修正結束 *** -->
          

          <div class="actions" style="margin-top:20px">
            <button class="btn" @click="register" :disabled="loading">
                <span v-if="loading">註冊中...</span>
                <span v-else>註冊</span>
            </button>
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
