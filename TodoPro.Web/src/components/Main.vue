  <script setup>
import { ref, computed,onMounted } from 'vue';
import { useRouter } from 'vue-router';

import TodoList from '../views/TodoList.vue';        
import NotificationView from '../views/NotificationView.vue'; 
import UsersList from '../views/UsersList.vue';       
import DoneList from '../views/DoneList.vue';         
import NewTodo from '../views/NewTodo.vue';       
import { decodeJwt } from '../jwtHelper';
const router = useRouter();
const activeMenu = ref('Todos'); 
const sidebarCollapsed = ref(false); 
const isSupervisor = ref(false);
onMounted(() => {
    checkSupervisorStatus();
});

function checkSupervisorStatus() {
    
    const token = localStorage.getItem('authToken'); 
    console.log(token)
    if (token) {
        const payload = decodeJwt(token);
        if (payload) {
            isSupervisor.value = payload.supervisor === 'True' || payload.supervisor === true;
        }
    }
}
function signOut() {
  if(window.confirm("是否要登出?"))router.push('/login');
  localStorage.removeItem('authToken'); 
}

function selectMenu(menu) {
  activeMenu.value = menu;
  console.log(`Navigating to: ${menu}`);
}

// *** NEW: 計算屬性，用於確定要渲染哪個組件 ***
const currentView = computed(() => {
  switch (activeMenu.value) {
    case 'NewTodo':
      return NewTodo;
    case 'Todos':
      return TodoList;
    case 'Groups':
      return NotificationView; // 這裡將 'Groups' 映射為 NotificationView
    case 'Users':
      return UsersList;
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
      <div class="logo">TodoPro 專案</div>
      
      <nav class="nav-menu">
        <button 
          v-if="isSupervisor" 
          class="menu-item" 
          :class="{ active: activeMenu === 'NewTodo' }"
          @click="selectMenu('NewTodo')"
        >
          ➕ 新增工作
        </button>
        <button 
          class="menu-item" 
          :class="{ active: activeMenu === 'Todos' }"
          @click="selectMenu('Todos')"
        >
          📝 待辦事項
        </button>
        <button 
          class="menu-item" 
          :class="{ active: activeMenu === 'Groups' }"
          @click="selectMenu('Groups')"
        >
          👥 通知 </button>
        <button 
          class="menu-item" 
          :class="{ active: activeMenu === 'Users' }"
          @click="selectMenu('Users')"
        >
          👨‍💼 團隊成員 
        </button>
        <button 
          class="menu-item" 
          :class="{ active: activeMenu === 'Done' }"
          @click="selectMenu('Done')"
        >
          ✅ 已完成事項 
        </button>
      </nav>
      
      <div class="user-actions">
        <p>你好! </p>
        <button class="btn btn-register" @click="signOut">登出</button>
      </div>
    </aside>

      <main class="content-area">
        <header class="content-header">
            <h2>{{ activeMenu === 'Groups' ? '通知中心' : activeMenu }} </h2>

        </header>
        

        
      </main>
    </div>
  </template>

  <style scoped>
  /* --- 全局佈局 --- */
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
  }
  .sidebar.collapsed {
    width: 60px; /* 收起時的寬度 */
  }
  .sidebar.collapsed .logo,
  .sidebar.collapsed .menu-title,
  .sidebar.collapsed .btn-register,
  .sidebar.collapsed .btn-login {
    display: none;
  }
  .sidebar.collapsed .menu-item {
    justify-content: center;
    padding: 12px 0;
  }
  .sidebar.collapsed .menu-item::before {
    content: none;
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

  .menu-title {
    font-size: 0.9rem;
    color: #666;
    padding: 8px 20px;
    margin: 10px 0 5px 0;
    font-weight: normal;
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
    white-space: nowrap; /* 防止文字換行 */
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

  .user-actions {
    padding: 20px;
    border-top: 1px solid rgba(0, 0, 0, 0.05);
    display: flex;
    flex-direction: column;
    gap: 10px;
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

  /* --- Todo 列表樣式 --- */
  .todo-list-container {
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 10px 0;
  }

  .todo-card-item {
    background: rgba(255, 255, 255, 0.85);
    padding: 15px 20px;
    border-radius: 8px;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.08);
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  .todo-title {
    font-size: 1.1rem;
    font-weight: 500;
    color: #333;
  }
  .todo-status {
    padding: 4px 10px;
    border-radius: 50px;
    font-size: 0.85rem;
    font-weight: bold;
    background: #28a745; /* Success Green */
    color: white;
  }
  .todo-status.pending {
    background: #ff7f11; /* Pending Orange */
  }

  .data-placeholder {
    text-align: center;
    color: #666;
    margin-top: 30px;
  }

  /* --- 繼承您的按鈕基礎樣式 --- */
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