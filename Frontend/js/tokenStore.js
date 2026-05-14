const KEYS = {
  ACCESS:  'nirbi_access_token',
  REFRESH: 'nirbi_refresh_token',
  USER_ID: 'nirbi_user_id',
  USERNAME:'nirbi_username',
};

function decodeJwt(token) {
  try {
    const payload = token.split('.')[1];
    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

export const tokenStore = {
  save(accessToken, refreshToken, userId, username) {
    localStorage.setItem(KEYS.ACCESS,   accessToken);
    localStorage.setItem(KEYS.REFRESH,  refreshToken);
    if (userId)   localStorage.setItem(KEYS.USER_ID,  String(userId));
    if (username) localStorage.setItem(KEYS.USERNAME, username);
  },

  clear() {
    Object.values(KEYS).forEach(k => localStorage.removeItem(k));
  },

  getAccess()   { return localStorage.getItem(KEYS.ACCESS); },
  getRefresh()  { return localStorage.getItem(KEYS.REFRESH); },
  getUserId()   { return localStorage.getItem(KEYS.USER_ID); },
  getUsername() { return localStorage.getItem(KEYS.USERNAME); },

  isAccessValid() {
    const token = this.getAccess();
    if (!token) return false;
    const payload = decodeJwt(token);
    if (!payload?.exp) return false;
    // refresh 30s before expiry
    return payload.exp * 1000 > Date.now() + 30_000;
  },

  hasSession() {
    return !!this.getRefresh();
  },
};
