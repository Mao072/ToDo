<script setup>
import { ref, onMounted } from 'vue';
import api from '../api'; // 引用您現有的 axios 實例

// --- 資料狀態 ---
const loading = ref(false);
const message = ref({ text: '', type: '' }); // type: 'success' | 'error'
const user = ref({
    // [修正] 必須包含 account 屬性來接收和顯示帳號/Email
    account: '', 
    name: '',
    id: null
});

// --- 密碼表單狀態 (保持不變) ---
const passwordForm = ref({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
});

// --- 輔助：顯示訊息 (保持不變) ---
const showMessage = (text, type = 'success') => {
    message.value = { text, type };
    // 3秒後自動消失
    setTimeout(() => {
        message.value = { text: '', type: '' };
    }, 3000);
};

// --- API: 獲取使用者資料 ---
const fetchUserProfile = async () => {
    loading.value = true;
    try {
        // 🔥 [路徑修正] 使用 C# Controller 定義的 GET api/Users/me
        const response = await api.get('/Users/me'); 
        user.value = response.data;
    } catch (err) {
        console.error(err);
        showMessage('無法載入使用者資料', 'error');
    } finally {
        loading.value = false;
    }
};

// --- API: 更新基本資料 (名稱) ---
const updateProfile = async () => {
    // [修正] 應使用 user.value.name.trim() 檢查，確保不為空
    if (!user.value.name.trim()) {
        showMessage('暱稱不能為空', 'error');
        return;
    }
    
    loading.value = true;
    try {
        // 🔥 [路徑修正] 使用 C# Controller 定義的 PUT api/Users/profile
        await api.put('/Users/profile', {
            name: user.value.name
        });
        showMessage('個人資料更新成功！，重新登入後才會顯示改變');
        
        // 可選：成功後重新載入確保資料同步 (特別是如果名稱被用於 Sidebar 顯示)
        await fetchUserProfile(); 
    } catch (err) {
        console.error(err);
        // 捕捉後端返回的錯誤訊息 (例如：名稱重複)
        const errorMsg = err.response?.data?.message || '更新失敗，請稍後再試。';
        showMessage(errorMsg, 'error');
    } finally {
        loading.value = false;
    }
};

// --- API: 修改密碼 ---
const changePassword = async () => {
    // 前端驗證 (保持不變)
    if (!passwordForm.value.currentPassword || !passwordForm.value.newPassword) {
        showMessage('請填寫所有密碼欄位', 'error');
        return;
    }
    if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
        showMessage('兩次輸入的新密碼不符', 'error');
        return;
    }

    loading.value = true;
    try {
        // 🔥 [路徑修正] 使用 C# Controller 定義的 PUT api/Users/password
        await api.put('/Users/password', {
            oldPassword: passwordForm.value.currentPassword,
            newPassword: passwordForm.value.newPassword
        });
        
        showMessage('密碼修改成功！下次登入請使用新密碼。');
        // 清空表單 (保持不變)
        passwordForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' };
    } catch (err) {
        console.error(err);
        // 捕捉後端返回的錯誤訊息 (例如：舊密碼錯誤)
        const errorMsg = err.response?.data?.message || '密碼修改失敗，請確認舊密碼是否正確。';
        showMessage(errorMsg, 'error');
    } finally {
        loading.value = false;
    }
};

onMounted(() => {
    fetchUserProfile();
});
</script>

<template>
    <div class="profile-container">
        <h2 class="page-title">個人資料設定</h2>

        <div v-if="message.text" class="alert-box" :class="message.type">
            {{ message.text }}
        </div>

        <div class="profile-grid">
            <div class="card info-card">
                <h3 class="card-title">基本資訊</h3>
                <form @submit.prevent="updateProfile">
                    <div class="form-group">
                        <label>名稱</label>
                        <h3 style="color: #2d3748;">{{ user.name }}</h3>
                        <span class="help-text"></span>
                    </div>

                    <div class="form-group">
                        <label>更改名稱</label>
                        <input 
                            type="text" 
                            v-model="user.name" 
                            placeholder="請輸入您的暱稱" 
                            class="input-field"
                        />
                    </div>

                    <div class="form-actions">
                        <button type="submit" class="btn btn-primary" :disabled="loading">
                            {{ loading ? '儲存中...' : '儲存變更' }}
                        </button>
                    </div>
                </form>
            </div>

            <div class="card security-card">
                <h3 class="card-title">更改密碼</h3>
                <form @submit.prevent="changePassword">
                    <div class="form-group">
                        <label>目前密碼</label>
                        <input 
                            type="password" 
                            v-model="passwordForm.currentPassword"
                            placeholder="輸入目前使用的密碼"
                            class="input-field"
                        />
                    </div>

                    <div class="form-group">
                        <label>新密碼</label>
                        <input 
                            type="password" 
                            v-model="passwordForm.newPassword"
                            placeholder="設定新密碼"
                            class="input-field"
                        />
                    </div>

                    <div class="form-group">
                        <label>確認新密碼</label>
                        <input 
                            type="password" 
                            v-model="passwordForm.confirmPassword"
                            placeholder="再次輸入新密碼"
                            class="input-field"
                        />
                    </div>

                    <div class="form-actions">
                        <button type="submit" class="btn btn-outline" :disabled="loading">
                            修改密碼
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<style scoped>
.profile-container {
    max-width: 900px;
    margin: 0 auto;
    /* 這裡不需要 padding，因為父層 content-area 已經有 padding */
}

.page-title {
    font-size: 1.8rem;
    color: #2d3748;
    margin-bottom: 25px;
    font-weight: 700;
}

/* --- 佈局 --- */
.profile-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 30px;
}

@media (max-width: 768px) {
    .profile-grid {
        grid-template-columns: 1fr; /* 手機版變單欄 */
    }
}

/* --- 卡片樣式 --- */
.card {
    background: #fff;
    padding: 25px;
    border-radius: 12px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
    border: 1px solid #e2e8f0;
}

.card-title {
    font-size: 1.25rem;
    color: #4a5568;
    margin-bottom: 20px;
    padding-bottom: 10px;
    border-bottom: 2px solid #f7fafc;
    font-weight: 600;
}

/* --- 表單元件 --- */
.form-group {
    margin-bottom: 20px;
}

.form-group label {
    display: block;
    margin-bottom: 8px;
    color: #718096;
    font-size: 0.9rem;
    font-weight: 500;
}

.input-field, .input-disabled {
    width: 70%;
    padding: 10px 15px;
    border-radius: 8px;
    border: 1px solid #cbd5e0;
    font-size: 1rem;
    transition: border-color 0.2s;
}

.input-field:focus {
    outline: none;
    border-color: #ff7f11;
    box-shadow: 0 0 0 3px rgba(255, 127, 17, 0.1);
}

.input-disabled {
    background-color: #edf2f7;
    color: #718096;
    cursor: not-allowed;
    border-color: #e2e8f0;
}

.help-text {
    display: block;
    margin-top: 5px;
    font-size: 0.8rem;
    color: #a0aec0;
}

/* --- 按鈕 --- */
.form-actions {
    margin-top: 10px;
    text-align: right;
}

.btn {
    padding: 10px 20px;
    border-radius: 6px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
    border: none;
    font-size: 0.95rem;
}

.btn:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.btn-primary {
    background-color: #ff7f11;
    color: white;
}
.btn-primary:hover:not(:disabled) {
    background-color: #e06d0e;
    transform: translateY(-1px);
}

.btn-outline {
    background-color: transparent;
    border: 1px solid #ff7f11;
    color: #ff7f11;
}
.btn-outline:hover:not(:disabled) {
    background-color: #fff5eb;
}

/* --- 訊息提示框 --- */
.alert-box {
    padding: 12px 20px;
    border-radius: 8px;
    margin-bottom: 20px;
    font-weight: 500;
}
.alert-box.success {
    background-color: #c6f6d5;
    color: #2f855a;
    border: 1px solid #9ae6b4;
}
.alert-box.error {
    background-color: #fed7d7;
    color: #c53030;
    border: 1px solid #feb2b2;
}
</style>