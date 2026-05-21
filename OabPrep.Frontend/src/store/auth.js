/**
 * OAB Prep — AuthStore
 * Gerencia tokens JWT e estado do usuário autenticado.
 * Usa localStorage (em produção React Native: SecureStore — FE-01).
 */

const KEYS = {
  ACCESS:  'oab_access_token',
  REFRESH: 'oab_refresh_token',
  USER:    'oab_user',
};

const AuthStore = {
  getAccessToken:  () => localStorage.getItem(KEYS.ACCESS),
  getRefreshToken: () => localStorage.getItem(KEYS.REFRESH),

  getUser() {
    try { return JSON.parse(localStorage.getItem(KEYS.USER)); }
    catch { return null; }
  },

  setTokens(accessToken, refreshToken) {
    localStorage.setItem(KEYS.ACCESS, accessToken);
    if (refreshToken) localStorage.setItem(KEYS.REFRESH, refreshToken);
  },

  setUser(user) {
    localStorage.setItem(KEYS.USER, JSON.stringify(user));
  },

  isAuthenticated() {
    return !!this.getAccessToken();
  },

  isAdmin() {
    return this.getUser()?.role === 'Admin';
  },

  clear() {
    Object.values(KEYS).forEach(k => localStorage.removeItem(k));
  },
};

export default AuthStore;
