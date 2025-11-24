<script setup>
import { useRoute } from 'vue-router';
import { ref, onMounted, onUnmounted, nextTick, computed } from 'vue'; // 新增 computed
import api from '../api';
import { decodeJwt } from '../jwtHelper';

const route = useRoute();
const todoId = ref(null);
const todoDetail = ref(null);
const loading = ref(true);
const error = ref(null);

// --- 聊天狀態 ---
const messages = ref([]);
const newMessageContent = ref('');
const isSending = ref(false);
const currentSupervisorId = ref(null);
let chatPollInterval = null;

// --- [優化] 時間格式化 (移除手動 +8，依賴瀏覽器本地時區) ---
const formatLocalTimeLong = (utcDateString) => {
    if (!utcDateString) return 'N/A';
    try {
        // 確保是標準 ISO 格式 (補 Z)
        let dateStr = utcDateString;
        if (!dateStr.endsWith('Z') && !dateStr.includes('+')) {
            dateStr = dateStr.replace(' ', 'T') + 'Z';
        }
        const date = new Date(dateStr);
        return date.toLocaleString('zh-TW', {
            year: 'numeric', month: '2-digit', day: '2-digit',
            hour: '2-digit', minute: '2-digit', hour12: false
        });
    } catch (e) { return utcDateString; }
};

const formatTime = (dateString) => {
    if (!dateString) return '';
    try {
        let dateStr = dateString;
        if (!dateStr.endsWith('Z') && !dateStr.includes('+')) {
            dateStr = dateStr.replace(' ', 'T') + 'Z';
        }
        return new Date(dateStr).toLocaleTimeString('zh-TW', {
            hour: '2-digit', minute: '2-digit', hour12: false
        });
    } catch (e) { return ''; }
};

// --- [新增] 日期分隔判斷 ---
const formatDateLabel = (dateString) => {
    try {
        const date = new Date(dateString);
        const today = new Date();
        const yesterday = new Date();
        yesterday.setDate(today.getDate() - 1);

        if (date.toDateString() === today.toDateString()) return '今天';
        if (date.toDateString() === yesterday.toDateString()) return '昨天';
        
        return date.toLocaleDateString('zh-TW', { month: 'long', day: 'numeric' });
    } catch (e) { return dateString; }
};

const shouldShowDateSeparator = (currentMsg, index) => {
    if (index === 0) return true; // 第一條訊息必顯示
    
    const prevMsg = messages.value[index - 1];
    const currentDate = new Date(currentMsg.createdAt).toDateString();
    const prevDate = new Date(prevMsg.createdAt).toDateString();
    
    return currentDate !== prevDate;
};

// --- 輔助函式：識別當前登入的用戶 ID ---
function identifyCurrentUser() {
    const token = localStorage.getItem('authToken');
    if (token) {
        const payload = decodeJwt(token);
        const userId = payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        if (userId) currentSupervisorId.value = parseInt(userId); 
    }
}

function scrollToBottom() {
    nextTick(() => {
        const chatBox = document.getElementById('chat-messages');
        if (chatBox) chatBox.scrollTop = chatBox.scrollHeight;
    });
}

// --- API 呼叫邏輯 (保持不變) ---
async function fetchTodoDetail() {
    loading.value = true;
    error.value = null;
    const id = parseInt(route.params.id);
    if (isNaN(id)) {
        error.value = "無效的任務 ID。";
        loading.value = false;
        return;
    }
    try {
        const response = await api.get(`/todos/${id}`); 
        todoDetail.value = response.data;
        todoId.value = id;
        if (todoDetail.value.discussionGroup?.id) {
            await fetchMessages(todoDetail.value.discussionGroup.id);
            startMessagePolling();
        }
    } catch (err) {
        error.value = "無法載入任務詳情。";
        console.error(err);
    } finally {
        loading.value = false;
    }
}

async function fetchMessages(groupId) {
    if (!groupId) return;
    try {
        const response = await api.get(`/groups/${groupId}/messages`);
        const shouldScroll = messages.value.length !== response.data.length;
        messages.value = response.data;
        if (shouldScroll) scrollToBottom();
    } catch (err) {
        console.error("Fetch msg error:", err);
        if (err.response && err.response.status === 404) stopMessagePolling();
    }
}

function startMessagePolling() {
    if (chatPollInterval) clearInterval(chatPollInterval);
    chatPollInterval = setInterval(() => {
        if (todoDetail.value?.discussionGroup?.id && !isSending.value) {
            fetchMessages(todoDetail.value.discussionGroup.id);
        }
    }, 3000);
}

function stopMessagePolling() {
    if (chatPollInterval) clearInterval(chatPollInterval);
}

// --- [優化] 發送訊息 (支援 Shift+Enter 換行) ---
async function handleEnterKey(e) {
    if (e.shiftKey) return; // Shift+Enter 允許換行
    await sendMessage();
}

async function sendMessage() {
    const groupId = todoDetail.value?.discussionGroup?.id;
    if (!groupId || !newMessageContent.value.trim() || isSending.value) return;

    isSending.value = true;
    const content = newMessageContent.value.trim();
    newMessageContent.value = ''; 

    try {
        await api.post(`/groups/${groupId}/messages`, { content: content });
        await fetchMessages(groupId); 
    } catch (err) {
        console.error("Send failed:", err);
        newMessageContent.value = content; // 失敗回填
        alert("發送失敗，請重試");
    } finally {
        isSending.value = false;
        scrollToBottom();
    }
}

onMounted(() => {
    identifyCurrentUser();
    fetchTodoDetail();
});

onUnmounted(() => {
    stopMessagePolling();
});
</script>
<template>
  <div class="todo-detail-view">
    <button @click="$router.push('/main')" class="back-btn">
        &larr; 返回待辦事項列表
    </button>

    <div v-if="loading" class="loading-box">
        正在載入任務 {{ route.params.id }} 的詳細資訊...
    </div>
    
    <div v-else-if="error" class="error-box">{{ error }}</div>
    
    <div v-else-if="todoDetail" class="detail-grid">
        <div class="task-info">
            <h1 class="task-title">{{ todoDetail.title }}</h1>

            <div class="status-header">
                 <span class="status-badge" :class="todoDetail.isCompleted ? 'status-done' : 'status-pending'">
                    {{ todoDetail.isCompleted ? '已完成' : '進行中' }}
                </span>
                <span class="created-at">創建於: {{ formatLocalTimeLong(todoDetail.createdAt) }}</span>
            </div>
            
            <div class="description-box">
                <h3 class="font-semibold">描述:</h3>
                <p class="font-semibold" >{{ todoDetail.description || '無詳細描述' }}</p>
            </div>
            
            <div class="member-info">
                 <h3 class="font-semibold">參與成員 ({{ todoDetail.participantCount || todoDetail.discussionGroup?.members?.length || 1 }} 人):</h3>
                 <div class="member-list">
                    <span v-for="member in todoDetail.discussionGroup?.members" :key="member.id" class="member-tag">
                        {{ member.name || member.account }}
                    </span>
                 </div>
            </div>
        </div>

        <div class="chat-panel">
            <header class="chat-header">
                討論區: {{ todoDetail.discussionGroup?.name || '無法載入群組名稱' }}
            </header>

            <div id="chat-messages" class="chat-messages">
                <div v-if="messages.length === 0 && !loading" class="empty-chat">
                    <p>💬 開始您的討論吧！</p>
                </div>
                
                <div v-for="(msg, index) in messages" :key="msg.id">
                    
                    <div v-if="shouldShowDateSeparator(msg, index)" class="date-separator">
                        <span>{{ formatDateLabel(msg.createdAt) }}</span>
                    </div>

                    <div 
                        class="message-bubble"
                        :class="{ 'message-self': msg.user.id === currentSupervisorId }"
                    >
                        <div class="message-meta">
                            <span class="sender-name">{{ msg.user.name || msg.user.account }}</span>
                            <span class="timestamp">{{ formatTime(msg.createdAt) }}</span>
                        </div>
                        <p class="message-content">{{ msg.content }}</p>
                    </div>
                </div>
            </div>

            <form @submit.prevent="sendMessage" class="chat-input-form">
                <textarea
                    v-model="newMessageContent"
                    :disabled="isSending"
                    placeholder="輸入訊息... (Shift+Enter 換行)"
                    @keydown.enter.prevent.exact="handleEnterKey"
                ></textarea>
                <button type="submit" :disabled="isSending || !newMessageContent.trim()">
                    發送
                </button>
            </form>
        </div>
    </div>
  </div>
</template>
<style scoped>
.todo-detail-view {
    padding: 10px;
}
.back-btn {
    padding: 8px 15px;
    margin-bottom: 20px;
    background: #e2e8f0;
    color: #4a5568;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    transition: background 0.2s;
}
.back-btn:hover { background: #cbd5e0; }

.loading-box, .error-box {
    padding: 20px;
    text-align: center;
    border-radius: 8px;
    background: #fff;
    box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}
.error-box { color: #c53030; background: #fee2e2; }
.font-semibold { color: #000; }

/* --- [修正 1] Grid 佈局保護 --- */
.detail-grid {
    display: grid;
    /* minmax(300px, 1fr) 確保左側至少 300px，防止被右側擠壓 */
    grid-template-columns: minmax(300px, 1fr) 2fr; 
    gap: 20px;
    align-items: start; /* 讓兩邊高度不強制拉伸，視需求可拿掉 */
}

.task-info {
    background: #fff;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    word-break: break-word;
}
.task-title {
    font-size: 2rem;
    font-weight: 700;
    color: #2d3748;
    margin-bottom: 10px;
    word-break: break-word;
    line-height: 1.2;
}
.task-id, .created-at { font-size: 0.9rem; color: #718096; }
.status-header {
    display: flex;
    justify-content: space-between;
    padding-bottom: 15px;
    border-bottom: 1px solid #e2e8f0;
    margin-bottom: 15px;
    flex-wrap: wrap; /* 防止小螢幕標題擠在一起 */
    gap: 10px;
}
.status-badge {
    padding: 4px 8px;
    border-radius: 15px;
    font-weight: 600;
}
.status-pending { background-color: #feebc8; color: #d69e2e; }
.status-done { background-color: #b2f5d3; color: #38a169; }
.description-box {
    margin-bottom: 20px;
    padding: 10px;
    border-left: 3px solid #ff7f11;
    background-color: #f7fafc;
    border-radius: 4px;
}
.member-list {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-top: 10px;
}
.member-tag {
    padding: 4px 10px;
    background: #e9eef6;
    border-radius: 15px;
    font-size: 0.85rem;
    color: #2b6cb0;
}
.member-loading { background: #ccc; color: #666; }

/* --- Chat Panel --- */
.chat-panel {
    display: flex;
    flex-direction: column;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    height: 70vh;
    /* --- [修正 2] 關鍵：防止 Flex/Grid 項目被內容撐大 --- */
    min-width: 0; 
}

.chat-header {
    padding: 15px;
    border-bottom: 1px solid #e2e8f0;
    font-weight: 600;
    color: #ff7f11;
    background: #f7fafc;
    border-top-left-radius: 8px;
    border-top-right-radius: 8px;
}

.chat-messages {
    flex-grow: 1;
    overflow-y: auto;
    padding: 15px;
    background-color: #f0f0f0;
}

.empty-chat {
    text-align: center;
    color: #999;
    padding: 20px;
}

/* --- [新增] 日期分隔線樣式 --- */
.date-separator {
    text-align: center;
    margin: 15px 0;
    position: relative;
}
.date-separator span {
    background-color: #e2e8f0;
    color: #718096;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 600;
}

.message-bubble {
    display: table;
    flex-direction: column;
    max-width: 70%; /* 稍微放寬一點，閱讀體驗較好 */
    margin-bottom: 10px;
}
.message-self {
    margin-left: auto; /* 靠右 */
    align-items: flex-end;
}
.message-meta {
    font-size: 0.75rem;
    color: #718096;
    margin-bottom: 2px;
    display: flex;
    gap: 8px;
    align-items: baseline;
}
.message-self .message-meta {
    color: #4a5568;
    justify-content: flex-end;
}
.sender-name { font-weight: 600; }

.message-content {
    padding: 8px 12px;
    border-radius: 10px;
    font-size: 0.95rem;
    line-height: 1.4;
    overflow-wrap: break-word;
    word-wrap: break-word;
    word-break: break-all; 
}

/* 氣泡顏色 */
.message-bubble:not(.message-self) .message-content {
    color: #2d3748; 
    max-width: 70%;
    background-color: #ffffff;
    border-bottom-left-radius: 2px;
    border: 1px solid #e2e8f0;
}
.message-self .message-content {
    background-color: #ff7f11;
    color: white;
    border-bottom-right-radius: 2px;
}

/* 輸入區 */
.chat-input-form {
    display: flex;
    padding: 10px;
    border-top: 1px solid #e2e8f0;
    background: #fff;
    border-bottom-left-radius: 8px;
    border-bottom-right-radius: 8px;
}
.chat-input-form textarea {
    flex-grow: 1;
    padding: 10px;
    border: 1px solid #e2e8f0;
    border-radius: 6px;
    resize: none;
    margin-right: 10px;
    font-size: 1rem;
    height: 45px; /* 稍微加高一點 */
    font-family: inherit;
}
.chat-input-form button {
    padding: 10px 20px;
    background: #ff7f11;
    color: white;
    border: none;
    border-radius: 6px;
    font-weight: 600;
    cursor: pointer;
    white-space: nowrap; /* 防止按鈕文字換行 */
}
.chat-input-form button:disabled {
    background: #cbd5e0;
    cursor: not-allowed;
}

/* --- [修正 4] 手機版響應式 --- */
@media (max-width: 768px) {
    .detail-grid {
        /* 手機版改為單欄堆疊 */
        grid-template-columns: 1fr; 
        gap: 20px;
    }
    
    /* 讓聊天室在手機版高度稍微小一點，適應螢幕 */
    .chat-panel {
        height: 60vh; 
        min-width: 0; 
    }
    
    .task-title {
        font-size: 1.5rem; /* 手機標題字體縮小 */
    }
}
</style>