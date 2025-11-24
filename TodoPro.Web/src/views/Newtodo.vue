<script setup>
import { ref, onMounted, computed } from 'vue'; 
import axios from 'axios';
import api from '../api' 
import { decodeJwt } from '../jwtHelper'; 

// --- 狀態管理 ---
const title = ref('');
const description = ref('');
const assignedUserIds = ref([]); // 用於 v-model 儲存選中的 User ID 列表
const allUsers = ref([]);      // 儲存從後端獲取的所有使用者列表
const departments = ref([]);   
const loading = ref(false);    
const error = ref('');
const successMessage = ref('');
const currentSupervisorId = ref(null); // 當前登入的管理員 ID (從 JWT 獲取)

// --- 生命周期鉤子 ---
onMounted(() => {
  identifyCurrentUser(); // 1. 識別當前用戶 ID
  fetchAllUsers();       // 2. 載入所有用戶並自動選中自己
  getDepartments();      // 3. 載入部門列表
});

// --- 輔助函式：識別當前登入的用戶 ID (從 JWT) ---
function identifyCurrentUser() {
    const token = localStorage.getItem('authToken');
    if (token) {
        const payload = decodeJwt(token);
        const userId = payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        
        if (userId) {
            currentSupervisorId.value = parseInt(userId); 
        }
    }
}


// --- API 呼叫：獲取所有使用者列表 ---
async function fetchAllUsers() {
  const token = localStorage.getItem('authToken');
  error.value = ''; 
  loading.value = true;
  
  if (!token) {
    loading.value = false;
    error.value = '無法驗證身份，請重新登入。';
    return;
  }

  try {
    const response = await axios.get('/api/todos/users', {
      headers: { Authorization: `Bearer ${token}` }
    });
    
    // 關鍵：將部門資訊展平到頂層，以便計算屬性可以直接存取
    allUsers.value = response.data.map(user => ({
        ...user,
        departmentId: user.departmentId ? user.departmentId : null,
        departmentName: user.department?.name || '未分類'
    }));
    console.log('Fetched users:', allUsers.value);
    // *** 關鍵邏輯：強制將管理員 ID 加入選中列表 ***
    if (currentSupervisorId.value) {
        const id = currentSupervisorId.value;
        if (!assignedUserIds.value.includes(id)) {
             assignedUserIds.value.push(id);
        }
    }

  } catch (err) {
    console.error('Failed to fetch users:', err);
    error.value = '無法載入團隊成員列表，請檢查伺服器連線。';
  } finally {
    loading.value = false;
  }
}

// --- API 呼叫：獲取部門列表 (用於外層循環) ---
async function getDepartments()
{
    // 這裡我們假設 departments API 不需要 Token (已在後端修正)
    try {
        const response = await api.get('/departments');
        departments.value = response.data; 
    } catch(error) {
        console.error('Failed to fetch departments:', error);
    }
}


// *** 計算屬性 - 按部門過濾使用者 (模板過濾器) ***
const filterUsersByDepartment = computed(() => (departmentId) => {
    // 這裡使用展平後的 departmentId 進行比對
    return allUsers.value.filter(user => user.departmentId === departmentId);
});


// --- API 呼叫：創建新的待辦事項 ---
async function createTodo() {
  // 檢查標題
  if (!title.value.trim()) {
    error.value = '待辦事項標題不能為空。';
    return;
  }

  loading.value = true;
  error.value = '';
  successMessage.value = '';
  const token = localStorage.getItem('authToken');

  const payload = {
    title: title.value.trim(),
    description: description.value.trim(),
    assignedUserIds: assignedUserIds.value 
  };

  try {
    if(window.confirm(`您確定要創建此待辦事項並指派給 ${assignedUserIds.value.length} 位成員嗎？`)) {
    } else {
        loading.value = false;
        return; // 使用者取消
    }
    const response = await axios.post('/api/todos', payload, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
    
    successMessage.value = `待辦事項 "${response.data.title}" 創建成功！討論區 ID: ${response.data.discussionGroupId}`;
    
    // 重置表單，並確保管理員仍被選中
    title.value = '';
    description.value = '';
    assignedUserIds.value = [currentSupervisorId.value]; 
  } catch (err) {
    console.error('Create Todo failed:', err);
    error.value = '創建待辦事項失敗，請檢查權限或伺服器。';
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="new-todo-container">
    <h3>新增工作事項</h3>
    
    <p v-if="error" class="message error">{{ error }}</p>
    <p v-if="successMessage" class="message success">{{ successMessage }}</p>

    <form @submit.prevent="createTodo" class="todo-form">
      
      <div class="form-group">
        <label for="title">標題 <span class="required">*</span></label>
        <input 
          type="text" 
          id="title" 
          v-model="title" 
          required 
          :disabled="loading"
          placeholder="例如：設計新的首頁介面"
        >
      </div>

      <div class="form-group">
        <label for="description">描述</label>
        <textarea 
          id="description" 
          v-model="description" 
          :disabled="loading"
          rows="4"
          placeholder="詳細說明工作需求和目標"
        ></textarea>
      </div>

      <!-- *** 指派成員區塊：實現分組與必選 *** -->
      <div class="form-group">
        <label>指派給 ( Assign To ) <span class="required"></span></label>
        
        <div class="user-groups">
          
          <!-- 載入和錯誤狀態顯示 -->
          <p v-if="loading && allUsers.length === 0" class="loading-text">正在載入部門和成員...</p>
          <p v-else-if="error" class="loading-text error-text">{{ error }}</p>
          <p v-else-if="departments.length === 0 && !loading" class="loading-text">沒有可用的部門。</p>
          
          <!-- 正常顯示分組列表 -->
          <div v-else>
            <!-- 遍歷部門列表 (外層循環) -->
            <div 
              v-for="department in departments" 
              :key="department.id" 
              class="department-group" 
            >
              <h4 class="department-title">{{ department.name }}</h4>
              
              <!-- 內層循環：只遍歷屬於當前部門的員工 -->
              <div class="user-select-grid">
                  <label 
                      v-for="user in filterUsersByDepartment(department.id)" 
                      :key="user.id" 
                      class="user-checkbox"
                      :class="{ 'required-member': user.id === currentSupervisorId }"
                  >
                      <input 
                          type="checkbox" 
                          :value="user.id"
                          v-model="assignedUserIds" 
                          :disabled="loading || user.id === currentSupervisorId"
                          :checked="user.id === currentSupervisorId"
                      >
                      {{ user.name || user.account }} 
                      
                      <!-- 標記當前管理員和主管 -->
                      <span v-if="user.id === currentSupervisorId" class="tag tag-self">你</span>
                      <span v-else-if="user.supervisor" class="tag tag-supervisor">主管</span>
                  </label>
                  
                  <!-- 如果該部門沒有成員 -->
                  <p v-if="filterUsersByDepartment(department.id).length === 0 && !loading" class="loading-text" style="text-align: left; width: 100%;">
                      此部門目前沒有成員。
                  </p>
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- *** 指派成員區塊結束 *** -->

      <!-- 提交按鈕 -->
      <button 
        type="submit" 
        class="btn btn-create" 
        :disabled="loading || !title.trim()" 
      >
        <span v-if="loading">創建中...</span>
        <span v-else>創建待辦事項</span>
      </button>
    </form>
  </div>
</template>

<style scoped>
/* 樣式保持不變 */
.new-todo-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
  background-color: #fcfcfc;
  border-radius: 10px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

h3 {
  font-size: 1.8rem;
  color: #333;
  margin-bottom: 25px;
  border-bottom: 2px solid #ff7f11;
  padding-bottom: 10px;
}

.todo-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 600;
  color: #555;
}

.form-group input[type="text"],
.form-group textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

.form-group input:focus,
.form-group textarea:focus {
  border-color: #ff7f11;
  outline: none;
  box-shadow: 0 0 0 3px rgba(255, 127, 17, 0.2);
}

.required {
  color: #ff7f11;
  font-weight: bold;
}

/* --- User Grid --- */
.user-groups {
  padding: 10px;
  border: 1px solid #eee;
  border-radius: 6px;
  background-color: #fff;
  min-height: 50px; 
}

.department-group {
  margin-bottom: 20px;
  padding: 10px 0;
  border-bottom: 1px dashed #eee;
}
.department-group:last-child {
  border-bottom: none;
}

.department-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: #ff7f11;
  margin: 0 0 10px 0;
  padding-left: 5px;
}

.user-select-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.user-checkbox {
  display: inline-flex;
  align-items: center;
  padding: 8px 12px;
  background-color: #f0f0f0;
  border-radius: 50px;
  cursor: pointer;
  transition: background-color 0.2s;
  font-size: 0.9rem;
}

.user-checkbox:hover {
  background-color: #e0e0e0;
}

.user-checkbox input[type="checkbox"] {
  margin-right: 8px;
  accent-color: #ff7f11; /* 改變 checkbox 顏色 */
}

/* 標記管理員為必選 */
.required-member {
  border: 2px solid #ff7f11; 
  background-color: #fff4e6;
  font-weight: 600;
}

.tag {
  margin-left: 5px;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 0.75rem;
  font-weight: 600;
}

.tag-supervisor {
  background-color: #ff7f11;
  color: white;
}
.tag-self {
  background-color: #3f51b5; /* 藍色標記自己 */
  color: white;
}


.loading-text {
  color: #999;
  font-style: italic;
  padding: 10px;
  text-align: center;
}
.error-text {
  color: #d93025;
}

/* --- 按鈕 --- */
.btn-create {
  margin-top: 15px;
  padding: 12px 25px;
  font-size: 1.1rem;
  background: #ff7f11;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn-create:hover:not(:disabled) {
  background: #ff6f00;
}

.btn-create:disabled {
  background: #ccc;
  cursor: not-allowed;
}

/* --- 訊息 --- */
.message {
  padding: 10px;
  margin-bottom: 15px;
  border-radius: 5px;
  font-weight: 600;
}
.error {
  background-color: #fdd;
  color: #d93025;
  border: 1px solid #f99;
}
.success {
  background-color: #e6ffe6;
  color: #3c763d;
  border: 1px solid #c3e6cb;
}
</style>