import { request } from './http';

// { id, model, registrationNumber, isActive, rows, seatLayout, businessRows }
export function getAircraftList({ signal } = {}) {
  return request('/api/Aircraft', { signal });
}

export function getAircraft(id, { signal } = {}) {
  return request(`/api/Aircraft/${id}`, { signal });
}
