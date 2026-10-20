// The API returns UTC times ("...Z"); they are shown in the browser's local time zone.

const dateFormat = new Intl.DateTimeFormat('he-IL', {
  weekday: 'short',
  day: 'numeric',
  month: 'short',
  year: 'numeric',
});

const timeFormat = new Intl.DateTimeFormat('he-IL', { hour: '2-digit', minute: '2-digit' });

const dateTimeFormat = new Intl.DateTimeFormat('he-IL', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

const priceFormat = new Intl.NumberFormat('he-IL', {
  style: 'currency',
  currency: 'ILS',
  maximumFractionDigits: 0,
});

export const formatDate = (iso) => dateFormat.format(new Date(iso));
export const formatTime = (iso) => timeFormat.format(new Date(iso));
export const formatDateTime = (iso) => dateTimeFormat.format(new Date(iso));

export function formatDuration(minutes) {
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  if (!hours) return `${rest} דק׳`;
  return rest ? `${hours} ש׳ ${rest} דק׳` : `${hours} ש׳`;
}

// Flights created before prices existed have price 0, which means "not set", not "free".
export function formatPrice(price) {
  return price > 0 ? priceFormat.format(price) : null;
}

// True when arrival falls on a later calendar day than departure (local time).
export function arrivesNextDay(departureIso, arrivalIso) {
  return new Date(departureIso).toDateString() !== new Date(arrivalIso).toDateString();
}
