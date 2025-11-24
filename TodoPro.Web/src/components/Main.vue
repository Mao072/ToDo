<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router'; // 引入 useRoute
import { decodeJwt } from '../jwtHelper';
import axios from 'axios'; 

// 引入所有動態視圖
import TodoList from '../views/TodoList.vue';        
import Profile from '../views/Profile.vue';     
import DoneList from '../views/DoneList.vue';         
import NewTodo from '../views/NewTodo.vue';   
import TodoDetail from '../views/TodoDetail.vue'; // 引入 TodoDetail

const router = useRouter();
const route = useRoute(); // 實例化 route 物件
const activeMenu = ref('Todos'); 
const sidebarCollapsed = ref(false); 
const isSupervisor = ref(false);
const userName = ref('訪客');

onMounted(() => {
    checkSupervisorStatus();
});

// 檢查 Supervisor 權限和用戶資訊
function checkSupervisorStatus() {
    const token = localStorage.getItem('authToken'); 
    
    if (!token) { return; }
    
    const payload = decodeJwt(token);
    if (payload) {
        console.log('Decoded JWT Payload:', payload);
        isSupervisor.value = payload.supervisor === 'True' || payload.supervisor === true;
        userName.value = payload.name || '用戶'; 
    }

    axios.get('http://localhost:5000/api/users/me', {
        headers: { Authorization: `Bearer ${token}` }
    })
    .then(response => {
        userName.value = response.data.name || response.data.account; 
    })
    .catch(error => {
        console.error("Failed to fetch user details:", error);
        if (error.response && (error.response.status === 401 || error.response.status === 403)) {
             signOut(false); 
        }
    });
}


// 登出函式 (強制重新載入)
function signOut(confirmRequired = true) {
  if (!confirmRequired || window.confirm("是否要登出?")) {
    localStorage.removeItem('authToken'); 
    router.push('/login')
        .then(() => { window.location.reload(); })
        .catch(() => { window.location.reload(); });
  }
}

function selectMenu(menu) {
  activeMenu.value = menu;
  // NEW: 導航到 /main 避免停留在 /todo/:id 路由上，確保 sidebar 選項可以切換
  if (route.name === 'todo-detail-layout') { 
      router.push('/main');
  }
}

// *** 關鍵修正：判斷當前是否處於 Detail View ***
const isDetailRoute = computed(() => {
    // 檢查路由名稱是否為我們在 router/index.js 中設定的名稱
    return route.name === 'todo-detail-layout' && route.params.id; 
});


// 動態組件切換 (僅在非 Detail 模式下使用)
const currentView = computed(() => {
    // 如果是詳情頁路由，currentView 實際上不會被用到，但仍需返回一個組件
    switch (activeMenu.value) {
        case 'NewTodo':
            return NewTodo;
        case 'Todos':
            return TodoList;
        case 'Profile':
            return Profile; 
        case 'Done':
            return DoneList;
        default:
            return TodoList;
    }
});
</script>

<template>
<div class="main-layout">
    <aside class="sidebar" :class="{ collapsed: sidebarCollapsed }">
        <div class="logo">Todo 專案</div>
        
        <nav class="nav-menu">
            <!-- 菜單項：當處於詳情頁時，活動狀態應在 Todos 上 -->
            <button 
                v-if="isSupervisor" 
                class="menu-item" 
                :class="{ active: activeMenu === 'NewTodo' && !isDetailRoute }"
                @click="selectMenu('NewTodo')"
            >
                ➕ 新增工作
            </button>
            
            <button 
                class="menu-item" 
                :class="{ active: activeMenu === 'Todos' || isDetailRoute }" 
                @click="selectMenu('Todos')"
            >
                📝 待辦事項
            </button>
            
            <button 
                class="menu-item" 
                :class="{ active: activeMenu === 'Profile' && !isDetailRoute }"
                @click="selectMenu('Profile')"
            >
                ⚙️ 個人資料
            </button>
            
            <button 
                class="menu-item" 
                :class="{ active: activeMenu === 'Done' && !isDetailRoute }"
                @click="selectMenu('Done')"
            >
                ✅ 已完成事項 
            </button>
        </nav>
        
        <div class="user-info-area">
            <p class="user-greeting">你好! <span>{{ userName }}</span></p>
        </div>
        
        <div class="user-actions">
            <button class="btn btn-register" @click="signOut()">登出</button>
        </div>
        
    </aside>

    <main class="content-area">
        <header class="content-header">
            <h2>
                <!-- 根據是否是詳情頁顯示不同標題 -->
                <span v-if="isDetailRoute">任務詳情 #{{ route.params.id }}</span>
                <span v-else>
                    {{ activeMenu === 'Profile' ? '個人資料' : 
                       activeMenu === 'NewTodo' ? '新增工作' :
                       activeMenu === 'Users' ? '團隊成員' :
                       activeMenu === 'Done' ? '已完成事項' :
                       '待辦事項列表'
                    }} 
                </span>
            </h2>
        </header>
        
        <!-- *** 關鍵修正：條件渲染 TodoDetail 或 側邊欄內容 *** -->
        <div class="view-wrapper">
            <!-- 如果是詳情路由，直接渲染 TodoDetail -->
            <TodoDetail v-if="isDetailRoute" /> 
            <!-- 否則，渲染當前選中的組件 -->
            <component :is="currentView" v-else />
        </div>
        
    </main>
</div>
</template>

<style scoped>
/* Style 保持不變 */
.main-layout {
  position: fixed;
  inset: 0;
  display: flex;
  background: linear-gradient(180deg, #f9cfb6 0%, #f8aa66 100%); 
}

/* --- 側邊欄 (Sidebar) --- */
.sidebar {
  width: 240px;
  background: rgba(255, 255, 255, 0.95);
  box-shadow: 4px 0 10px rgba(0, 0, 0, 0.1);
  padding: 20px 0;
  display: flex;
  flex-direction: column;
  transition: width 0.3s ease;
  z-index: 10;
  flex-shrink: 0;
}
.sidebar.collapsed {
  width: 60px; /* 收起時的寬度 */
}
.sidebar.collapsed .logo,
.sidebar.collapsed .user-greeting,
.sidebar.collapsed .user-info-area,
.sidebar.collapsed .btn-register {
  display: none;
}
.sidebar.collapsed .menu-item {
  justify-content: center;
  padding: 12px 0;
}

.logo {
  padding: 0 20px 20px;
  font-size: 1.5rem;
  font-weight: bold;
  color: #ff7f11;
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
  margin-bottom: 15px;
}

.nav-menu {
  flex-grow: 1;
}

.menu-item {
  display: flex;
  align-items: center;
  width: 100%;
  padding: 12px 20px;
  font-size: 1rem;
  text-align: left;
  background: transparent;
  border: none;
  cursor: pointer;
  color: #333;
  transition: background 0.2s ease, color 0.2s ease;
  white-space: nowrap; 
}
.menu-item:hover {
  background: rgba(255, 127, 17, 0.1);
  color: #ff7f11;
}
.menu-item.active {
  background: #ff7f11;
  color: #ffffff;
  font-weight: bold;
}

.user-info-area {
    padding: 0 20px 10px;
    border-top: 1px solid rgba(0, 0, 0, 0.05);
}
.user-greeting {
    font-size: 1rem;
    color: #333;
    font-weight: 500;
}
.user-greeting span {
    font-weight: bold;
    color: #ff7f11; 
}

.user-actions {
  padding: 0 20px 20px;
  display: flex;
  flex-direction: column;
}

.btn-register {
  background: #ff7f11;
  color: #ffffff;
  border: 1px solid #ff7f11;
}


/* --- 主內容區 (Main Content) --- */
.content-area {
  flex-grow: 1;
  padding: 30px;
  overflow-y: auto;
  min-width: 0;
}

.content-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  border-bottom: 2px solid rgba(0, 0, 0, 0.1);
  padding-bottom: 10px;
}
.content-header h2 {
  font-size: 2rem;
  color: #000000;
}
.toggle-btn {
  padding: 8px 15px;
  background: #ffffff;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  box-shadow: 0 2px 5px rgba(0,0,0,0.1);
}

.view-wrapper {
    /* 確保動態組件能正確佔用空間 */
    padding-top: 10px; 
}

/* --- 基礎按鈕樣式 (繼承自 Register/Login) --- */
.btn {
  padding: 12px 22px;
  font-size: 1rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all .18s ease;
  min-width: 110px;
  border: none;
  font-weight: 600;
}
.btn:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(0,0,0,0.18); }


/* --- RWD 調整 --- */
@media (max-width: 768px) {
  .sidebar {
    position: fixed;
    height: 100%;
    transform: translateX(0);
  }
  .sidebar.collapsed {
    transform: translateX(-180px); /* 隱藏大部分 */
    width: 60px;
  }
  .content-area {
    margin-left: 60px; /* 為了讓內容不會被收起的 sidebar 遮擋 */
    padding: 20px;
  }
}
</style>