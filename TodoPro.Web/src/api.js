import axios from 'axios'

// 從 Vite 環境變量讀取 API 基礎 URL，否則使用 /api
const baseURL = import.meta.env.VITE_API_BASE_URL || '/api' 

const api = axios.create({
  baseURL,
  // 保持 cookies/session 的配置
  withCredentials: true,
})

// *** Token 鍵名統一定義 ***
const TOKEN_KEY = 'authToken'; 

/**
 * 設置或移除 Bearer Token
 * @param {string | null} token - JWT Token
 */
export function setAuthToken(token) {
  if (token) {
    // 在 HTTP 請求頭中設置 Authorization
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`
    try { 
        // 儲存到 localStorage
        localStorage.setItem(TOKEN_KEY, token) 
    } catch (e) {
        console.error("Failed to set token in localStorage", e);
    }
  } else {
    // 移除 Header 和 localStorage 中的 Token
    delete api.defaults.headers.common['Authorization']
    try { 
        localStorage.removeItem(TOKEN_KEY) 
    } catch (e) {
        console.error("Failed to remove token from localStorage", e);
    }
  }
}

// 攔截器：在每次請求中自動設定最新的 Token (這是另一個常見的寫法，我們將使用 setAuthToken 輔助)
api.interceptors.request.use(config => {
    // 雖然 setAuthToken 設置了 default header，但這裡可以作為備用檢查
    const token = localStorage.getItem(TOKEN_KEY);
    if (token && !config.headers.Authorization) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, error => {
    return Promise.reject(error);
});


// *** 關鍵修正：從 storage 初始化 Token ***
try {
  const t = localStorage.getItem(TOKEN_KEY)
  if (t) {
    // 使用修正後的 setAuthToken 函式來初始化 header
    setAuthToken(t); 
  }
} catch (e) {
    console.error("Error initializing token from storage:", e);
}

export default api