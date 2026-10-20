import { request } from './http';

// POST /api/User/login -> { token, user: { id, fullName, email }, role }
export function login(email, password) {
  return request('/api/User/login', { method: 'POST', body: { email, password } });
}

// POST /api/User/register -> { id, fullName, email }. The server always assigns the User role.
export function register(fullName, email, password) {
  return request('/api/User/register', { method: 'POST', body: { fullName, email, password } });
}
