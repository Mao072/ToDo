import axios from 'axios'

const baseURL = import.meta.env.VITE_API_BASE_URL || '/api'

const api = axios.create({
  baseURL,
  withCredentials: true,
})

// helper to set bearer token for authenticated requests
export function setAuthToken(token) {
  if (token) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`
    try { localStorage.setItem('auth_token', token) } catch {}
  } else {
    delete api.defaults.headers.common['Authorization']
    try { localStorage.removeItem('auth_token') } catch {}
  }
}

// initialize token from storage if present
try {
  const t = localStorage.getItem('auth_token')
  if (t) setAuthToken(t)
} catch {}

export default api
