// Local-calendar date helpers. Dates are handled as local days; "YYYY-MM-DD" strings are local too.

export function toISODate(date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export function parseISODate(value) {
  const [y, m, d] = value.split('-').map(Number);
  return new Date(y, m - 1, d);
}

export function addDays(date, days) {
  const next = new Date(date);
  next.setDate(next.getDate() + days);
  return next;
}

export function addMonths(date, months) {
  return new Date(date.getFullYear(), date.getMonth() + months, 1);
}

export function startOfWeek(date) {
  return addDays(new Date(date.getFullYear(), date.getMonth(), date.getDate()), -date.getDay()); // Sunday
}

export function startOfMonth(date) {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

export function isSameDay(a, b) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

// The server filters by UTC days. A local day range is widened by one day on each side
// so no flight near midnight is missed; callers then keep only flights inside the local range.
export function serverRangeFor(firstDay, lastDay) {
  return { fromDate: toISODate(addDays(firstDay, -1)), toDate: toISODate(addDays(lastDay, 1)) };
}

export const WEEKDAYS = ['א׳', 'ב׳', 'ג׳', 'ד׳', 'ה׳', 'ו׳', 'ש׳'];

export const monthTitle = new Intl.DateTimeFormat('he-IL', { month: 'long', year: 'numeric' });
export const dayTitle = new Intl.DateTimeFormat('he-IL', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
export const shortDay = new Intl.DateTimeFormat('he-IL', { weekday: 'short', day: 'numeric', month: 'numeric' });
