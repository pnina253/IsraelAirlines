import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import * as authApi from '../api/auth';
import { setAuthToken, setUnauthorizedHandler } from '../api/http';

// The session lives in memory and in sessionStorage: it survives a page refresh and is gone when the tab closes.
// The server has no cookie-based auth, so this is the narrowest browser storage available.
const STORAGE_KEY = 'israelairlines.session';

const AuthContext = createContext(null);

function readStoredSession() {
  try {
    const session = JSON.parse(sessionStorage.getItem(STORAGE_KEY));
    return session?.token ? session : null;
  } catch {
    return null;
  }
}

function writeStoredSession(session) {
  try {
    if (session) sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    else sessionStorage.removeItem(STORAGE_KEY);
  } catch {
    // Storage can be unavailable (private mode); the in-memory session still works.
  }
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(() => {
    const stored = readStoredSession();
    setAuthToken(stored?.token ?? null);
    return stored;
  });
  const [notice, setNotice] = useState(null);

  const applySession = useCallback((next) => {
    setAuthToken(next?.token ?? null);
    writeStoredSession(next);
    setSession(next);
  }, []);

  const login = useCallback(
    async (email, password) => {
      const response = await authApi.login(email, password);
      applySession({ token: response.token, user: response.user, role: response.role });
      setNotice(null);
      return response;
    },
    [applySession],
  );

  const logout = useCallback(() => applySession(null), [applySession]);

  useEffect(() => {
    setUnauthorizedHandler(() => {
      applySession(null);
      setNotice('פג תוקף ההתחברות. יש להתחבר מחדש.');
    });
    return () => setUnauthorizedHandler(null);
  }, [applySession]);

  const value = useMemo(
    () => ({
      user: session?.user ?? null,
      role: session?.role ?? null,
      isAuthenticated: Boolean(session),
      isAdmin: session?.role === 'Admin',
      notice,
      clearNotice: () => setNotice(null),
      login,
      logout,
    }),
    [session, notice, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used inside AuthProvider');
  return context;
}
