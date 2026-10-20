import { request } from './http';

// POST /api/Reservation. The user comes from the JWT on the server, not from the body.
// 201 -> reservation, 400 -> invalid seat, 404 -> no such flight, 409 -> seat already taken.
export function createReservation(flightNumber, seatNumber) {
  return request('/api/Reservation', { method: 'POST', body: { flightNumber, seatNumber } });
}

export function getMyReservations({ signal } = {}) {
  return request('/api/Reservation/me', { signal });
}
