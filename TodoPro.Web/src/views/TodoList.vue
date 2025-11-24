<script setup>
import { ref, onMounted,computed } from 'vue';
import api from '../api'; // 假設 api.js 已經配置了 Token 攔截器
import { useRouter } from 'vue-router';
import axios from 'axios';

const router = useRouter();
const todos = ref([]);
const loading = ref(true);
const error = ref(null);
const pendingTodos = computed(() => {
    return todos.value.filter(todo => !todo.isCompleted);
});
function formatDateTime(dateString) {
    if (!dateString) return 'N/A';
    try {
        // *** 核心修正：如果後端沒有提供時區標記 (Z)，則假設它是 UTC 時間，
        // 將空格替換為 'T' 並加上 'Z'，強制 JS 視為 UTC 進行解析。 ***
        let fixedDateString = dateString;
        if (!fixedDateString.endsWith('Z') && !fixedDateString.includes('+')) {
            fixedDateString = fixedDateString.replace(' ', 'T') + 'Z';
        }
        
        const date = new Date(fixedDateString);
        
        // 轉換為本地時間 (GMT+8)，並使用一致的格式
        return date.toLocaleString('zh-TW', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            hour12: false // 使用 24 小時制
        });
    } catch (e) {
        console.error("Date formatting error:", e);
        // 如果轉換失敗，只返回日期部分
        return dateString.substring(0, 10);
    }
}


// --- API 呼叫：獲取所有參與的待辦事項 ---
async function fetchTodos() {
    loading.value = true;
    error.value = null;
    try {
        const response = await api.get('/todos');
        todos.value = response.data;
        console.log('Fetched todos:', todos.value);
    } catch (err) {
        console.error('Failed to fetch todos:', err);
        // 處理 401/403 錯誤，通常是 Token 過期或權限不足
        if (err.response && (err.response.status === 401 || err.response.status === 403)) {
            error.value = '會話已過期或權限不足，請重新登入。';
            // 可選：強制跳轉到登入頁
            // router.push('/login'); 
        } else {
            error.value = '無法載入待辦事項，請檢查後端服務。';
        }
    } finally {
        loading.value = false;
    }
}

// --- 生命周期鉤子 ---
onMounted(() => {
    fetchTodos();
});

// --- 動作：點擊任務卡片 (可選) ---
function viewTodoDetail(todoId) {
    router.push(`/todo/${todoId}`); 
}
</script>

<template>
  <div class="todo-list-view">
    <div v-if="loading" class="status-message loading">
        <div class="spinner"></div>
        <p>正在載入您的待辦事項...</p>
    </div>

    <div v-else-if="error" class="status-message error-message">
        <p>🚨 載入錯誤: {{ error }}</p>
        <button @click="fetchTodos" class="retry-btn">點擊重試</button>
    </div>

    <div v-else-if="todos.length === 0" class="status-message empty">
        <p>🎉 太棒了！您目前沒有任何待辦事項或參與的任務。</p>
    </div>

    <div v-else class="todo-grid">
        <!-- 任務列表 -->
        <div 
            v-for="todo in pendingTodos" 
            :key="todo.id" 
            class="todo-card" 
            :class="{ 'todo-completed': todo.isCompleted }"
            @click="viewTodoDetail(todo.id)"
        >
            <div class="card-header">
                <span class="status-badge" :class="todo.isCompleted ? 'status-done' : 'status-pending'">
                    {{ todo.isCompleted ? '已完成' : '進行中' }}
                </span>
                <span class="created-at">創建於: {{ formatDateTime(todo.createdAt) }}</span>
            </div>

            <h4 class="card-title">{{ todo.title }}</h4>

            <p class="card-description">{{ todo.description || '無描述' }}</p>

            <div class="card-footer">
                <div class="owner-info">
                    所有者: <span>{{ todo.user?.name || todo.user?.account || 'N/A' }}</span>
                    <span v-if="todo.user?.departmentName" class="dept-tag">
                        {{ todo.user.departmentName }}
                    </span>
                </div>
                
                <div @click="console.log((todo.participantCount))" class="group-info">
                    討論成員: <span>{{ todo.participantCount}} 人</span>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<style scoped>
.todo-list-view {
    padding: 10px;
}

.status-message {
    padding: 20px;
    margin: 20px 0;
    border-radius: 8px;
    text-align: center;
    font-size: 1.1rem;
}

.loading {
    background-color: #f0f4f8;
    color: #4a5568;
    display: flex;
    align-items: center;
    justify-content: center;
}
.spinner {
    border: 4px solid rgba(0, 0, 0, 0.1);
    border-left-color: #ff7f11;
    border-radius: 50%;
    width: 24px;
    height: 24px;
    animation: spin 1s linear infinite;
    margin-right: 10px;
}
@keyframes spin {
    to { transform: rotate(360deg); }
}

.error-message {
    background-color: #fee2e2;
    color: #c53030;
    border: 1px solid #fbd3d3;
}
.retry-btn {
    margin-top: 10px;
    padding: 8px 15px;
    background-color: #c53030;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.empty {
    background-color: #e9f7ef;
    color: #38a169;
    border: 1px solid #b2f5d3;
}

/* --- Todo Card Grid Layout --- */
.todo-grid {
    display: grid;
    gap: 20px;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
}

.todo-card {
    background: #ffffff;
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
    transition: transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out;
    cursor: pointer;
    border-left: 5px solid #ff7f11;
}
.todo-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 6px 15px rgba(0, 0, 0, 0.12);
}

.todo-completed {
    border-left: 5px solid #38a169; /* Green for completed */
    opacity: 0.85;
}

/* --- Card Content --- */
.card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 15px;
    font-size: 0.8rem;
    color: #718096;
}

.status-badge {
    padding: 4px 8px;
    border-radius: 15px;
    font-weight: 600;
}
.status-pending {
    background-color: #feebc8;
    color: #d69e2e;
}
.status-done {
    background-color: #b2f5d3;
    color: #38a169;
}

.card-title {
    font-size: 1.4rem;
    font-weight: 700;
    color: #2d3748;
    margin: 0 0 8px 0;
}

.card-description {
    color: #4a5568;
    margin-bottom: 15px;
    font-size: 0.95rem;
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 2; /* 限制兩行 */
    -webkit-box-orient: vertical;
}

.card-footer {
    padding-top: 10px;
    border-top: 1px dashed #edf2f7;
    display: flex;
    justify-content: space-between;
    font-size: 0.9rem;
}
.owner-info span, .group-info span {
    font-weight: 600;
    color: #2b6cb0; /* Blue for key info */
}
.dept-tag {
    margin-left: 8px;
    padding: 2px 6px;
    background-color: #e2e8f0;
    color: #4a5568;
    border-radius: 4px;
    font-size: 0.75rem;
}

</style>