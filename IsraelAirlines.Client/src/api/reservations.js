import { request } from './http';

// POST /api/reservations. The user comes from the JWT on the server, not from the body.
// 201 -> reservation, 400 -> invalid seat, 404 -> no such flight, 409 -> seat already taken.
export function createReservation(flightNumber, seatNumber) {
  return request('/api/reservations', { method: 'POST', body: { flightNumber, seatNumber } });
}

export function getMyReservations({ signal } = {}) {
  return request('/api/reservations/me', { signal });
}

// Moves the reservation to another seat. 409 if the new seat is taken.
export function changeSeat(reservationId, seatNumber) {
  return request(`/api/reservations/${reservationId}`, { method: 'PUT', body: { seatNumber } });
}

// Cancels the reservation and releases the seat. 204 on success.
export function cancelReservation(reservationId) {
  return request(`/api/reservations/${reservationId}`, { method: 'DELETE' });
}
