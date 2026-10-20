import { request } from './http';

// POST /api/auth/login -> { token, user: { id, fullName, email }, role }
export function login(email, password) {
  return request('/api/auth/login', { method: 'POST', body: { email, password } });
}

// POST /api/auth/register -> { id, fullName, email }. The server always assigns the User role.
export function register(fullName, email, password) {
  return request('/api/auth/register', { method: 'POST', body: { fullName, email, password } });
}
