// src/utils/jwtHelper.js

/**
 * 解碼 JWT 的 Payload 部分
 * @param {string} token - JWT 字串
 * @returns {object | null} Payload 物件
 */
export function decodeJwt(token) {
  if (!token) return null;
  
  try {
    // JWT 格式為 header.payload.signature
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    // Base64Url 解碼 payload
    const payload = parts[1];
    
    // Base64Url 轉 Base64
    let base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    
    // 填充 = 號使其長度成為 4 的倍數
    while (base64.length % 4) {
        base64 += '=';
    }

    // Node.js: return JSON.parse(Buffer.from(base64, 'base64').toString('utf8'));
    // 瀏覽器環境: 
    return JSON.parse(decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join('')));
    
  } catch (e) {
    console.error("Failed to decode JWT:", e);
    return null;
  }
}