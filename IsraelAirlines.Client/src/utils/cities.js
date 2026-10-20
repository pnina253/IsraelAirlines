// Geographic coordinates of well-known cities, used only to place server flight data on the map.
// The API stores origin/destination as free text; a city missing here is listed as "no location" instead of guessed.

const CITIES = [
  { name: 'Tel Aviv', he: 'תל אביב', lat: 32.0853, lng: 34.7818 },
  { name: 'Eilat', he: 'אילת', lat: 29.5577, lng: 34.9519 },
  { name: 'Haifa', he: 'חיפה', lat: 32.794, lng: 34.9896 },
  { name: 'Jerusalem', he: 'ירושלים', lat: 31.7683, lng: 35.2137 },
  { name: 'London', he: 'לונדון', lat: 51.5074, lng: -0.1278 },
  { name: 'Paris', he: 'פריז', lat: 48.8566, lng: 2.3522 },
  { name: 'Rome', he: 'רומא', lat: 41.9028, lng: 12.4964 },
  { name: 'Milan', he: 'מילאנו', lat: 45.4642, lng: 9.19 },
  { name: 'Athens', he: 'אתונה', lat: 37.9838, lng: 23.7275 },
  { name: 'Larnaca', he: 'לרנקה', lat: 34.9003, lng: 33.6232 },
  { name: 'Barcelona', he: 'ברצלונה', lat: 41.3874, lng: 2.1686 },
  { name: 'Madrid', he: 'מדריד', lat: 40.4168, lng: -3.7038 },
  { name: 'Lisbon', he: 'ליסבון', lat: 38.7223, lng: -9.1393 },
  { name: 'Berlin', he: 'ברלין', lat: 52.52, lng: 13.405 },
  { name: 'Munich', he: 'מינכן', lat: 48.1351, lng: 11.582 },
  { name: 'Frankfurt', he: 'פרנקפורט', lat: 50.1109, lng: 8.6821 },
  { name: 'Vienna', he: 'וינה', lat: 48.2082, lng: 16.3738 },
  { name: 'Prague', he: 'פראג', lat: 50.0755, lng: 14.4378 },
  { name: 'Budapest', he: 'בודפשט', lat: 47.4979, lng: 19.0402 },
  { name: 'Amsterdam', he: 'אמסטרדם', lat: 52.3676, lng: 4.9041 },
  { name: 'Zurich', he: 'ציריך', lat: 47.3769, lng: 8.5417 },
  { name: 'Istanbul', he: 'איסטנבול', lat: 41.0082, lng: 28.9784 },
  { name: 'Bucharest', he: 'בוקרשט', lat: 44.4268, lng: 26.1025 },
  { name: 'Warsaw', he: 'ורשה', lat: 52.2297, lng: 21.0122 },
  { name: 'Dubai', he: 'דובאי', lat: 25.2048, lng: 55.2708 },
  { name: 'Abu Dhabi', he: 'אבו דאבי', lat: 24.4539, lng: 54.3773 },
  { name: 'New York', he: 'ניו יורק', lat: 40.7128, lng: -74.006 },
  { name: 'Los Angeles', he: 'לוס אנג׳לס', lat: 34.0522, lng: -118.2437 },
  { name: 'Miami', he: 'מיאמי', lat: 25.7617, lng: -80.1918 },
  { name: 'Toronto', he: 'טורונטו', lat: 43.6532, lng: -79.3832 },
  { name: 'Bangkok', he: 'בנגקוק', lat: 13.7563, lng: 100.5018 },
  { name: 'Tokyo', he: 'טוקיו', lat: 35.6762, lng: 139.6503 },
  { name: 'Delhi', he: 'דלהי', lat: 28.6139, lng: 77.209 },
  { name: 'Mumbai', he: 'מומבאי', lat: 19.076, lng: 72.8777 },
  { name: 'Johannesburg', he: 'יוהנסבורג', lat: -26.2041, lng: 28.0473 },
  { name: 'Cairo', he: 'קהיר', lat: 30.0444, lng: 31.2357 },
];

const INDEX = new Map();
for (const city of CITIES) {
  INDEX.set(city.name.toLowerCase(), city);
  INDEX.set(city.he, city);
}

export function findCity(name) {
  return INDEX.get(name.trim().toLowerCase()) ?? INDEX.get(name.trim()) ?? null;
}
