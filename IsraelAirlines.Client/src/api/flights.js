import { request } from './http';

// GET /api/flights -> { items, page, pageSize, totalCount, totalPages }. Paging and filtering run on the server.
export function searchFlights({ page, pageSize, origin, destination, fromDate, toDate }, { signal } = {}) {
  return request('/api/flights', {
    query: { page, pageSize, origin, destination, fromDate, toDate },
    signal,
  });
}

export function getFlight(id, { signal } = {}) {
  return request(`/api/flights/${id}`, { signal });
}

// Requires login. Returns seat numbers only, e.g. ["2D", "14C"].
export function getReservedSeats(flightId, { signal } = {}) {
  return request(`/api/flights/${flightId}/reserved-seats`, { signal });
}

// Admin only.
export function createFlight(flight) {
  return request('/api/flights', { method: 'POST', body: flight });
}

// Loads every flight matching the filters by walking the server's pages (100 per request).
// Used by the map, calendar and timeline, which need a whole date range at once.
export async function fetchAllFlights(filters, { signal, maxPages = 10 } = {}) {
  const first = await searchFlights({ ...filters, page: 1, pageSize: 100 }, { signal });
  const items = [...first.items];
  const lastPage = Math.min(first.totalPages, maxPages);

  for (let page = 2; page <= lastPage; page++) {
    const next = await searchFlights({ ...filters, page, pageSize: 100 }, { signal });
    items.push(...next.items);
  }

  return { items, totalCount: first.totalCount, truncated: first.totalPages > maxPages };
}
