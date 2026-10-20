import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { CircleMarker, MapContainer, Polyline, TileLayer, Tooltip } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { ChevronLeft, MapPinOff, Map as MapIcon, X } from 'lucide-react';
import { fetchAllFlights } from '../api/flights';
import { errorMessage } from '../api/errors';
import useApi from '../utils/useApi';
import useQueryState from '../utils/useQueryState';
import { findCity } from '../utils/cities';
import { formatDate, formatPrice, formatTime } from '../utils/format';
import ViewSwitcher from '../components/ViewSwitcher';
import RouteFilter from '../components/RouteFilter';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';

const COLORS = { primary: '#0B3D91', secondary: '#4FB3E8', accent: '#F5A524', muted: '#9fb3cc' };

// A gentle arc between two points, so routes from the same city do not overlap as straight lines.
function arc(from, to, segments = 32) {
  const [lat1, lng1] = from;
  const [lat2, lng2] = to;
  const bend = 0.18;
  const ctrl = [(lat1 + lat2) / 2 + (lng2 - lng1) * bend, (lng1 + lng2) / 2 - (lat2 - lat1) * bend];
  return Array.from({ length: segments + 1 }, (_, i) => {
    const t = i / segments;
    const u = 1 - t;
    return [u * u * lat1 + 2 * u * t * ctrl[0] + t * t * lat2, u * u * lng1 + 2 * u * t * ctrl[1] + t * t * lng2];
  });
}

// Moves a label away from close neighbours so names and prices do not cover each other.
function labelDirection(city, cities) {
  const near = cities.filter(
    (other) => other !== city && Math.hypot(other.geo.lat - city.geo.lat, other.geo.lng - city.geo.lng) < 8,
  );
  const north = near.some((other) => other.geo.lat > city.geo.lat);
  const south = near.some((other) => other.geo.lat < city.geo.lat);
  if (north && south) return { direction: 'right', offset: [10, 0] };
  if (north) return { direction: 'bottom', offset: [0, 8] };
  return { direction: 'top', offset: [0, -8] };
}

function minPrice(flights) {
  const prices = flights.map((f) => f.price).filter((p) => p > 0);
  return prices.length ? Math.min(...prices) : null;
}

// Builds cities and routes from the flights the server returned. Cities without known coordinates are reported, not guessed.
function buildMap(flights) {
  const cities = new Map();
  const routes = new Map();
  const unknown = new Set();

  const cityEntry = (name) => {
    const geo = findCity(name);
    if (!geo) {
      unknown.add(name);
      return null;
    }
    const key = geo.name;
    if (!cities.has(key)) cities.set(key, { key, label: name, geo, arrivals: [], departures: [] });
    return cities.get(key);
  };

  for (const flight of flights) {
    const from = cityEntry(flight.origin);
    const to = cityEntry(flight.destination);
    if (!from || !to) continue;
    from.departures.push(flight);
    to.arrivals.push(flight);
    const key = `${from.key}|${to.key}`;
    if (!routes.has(key)) routes.set(key, { key, from, to, flights: [] });
    routes.get(key).flights.push(flight);
  }

  return { cities: [...cities.values()], routes: [...routes.values()], unknown: [...unknown] };
}

export default function MapPage() {
  const [query, setQuery, queryKey] = useQueryState({ origin: '', destination: '', fromDate: '', toDate: '' });
  const [selected, setSelected] = useState(null);

  const { status, data, error, reload } = useApi(
    (signal) => fetchAllFlights({ origin: query.origin, destination: query.destination, fromDate: query.fromDate, toDate: query.toDate }, { signal }),
    [queryKey],
  );

  const map = useMemo(() => buildMap(data?.items ?? []), [data]);
  const selectedCity = map.cities.find((c) => c.key === selected) ?? null;
  const bounds = map.cities.length ? map.cities.map((c) => [c.geo.lat, c.geo.lng]) : null;
  const boundsKey = map.cities.map((c) => c.key).sort().join(',');

  const panelFlights = selectedCity
    ? [...selectedCity.arrivals].sort((a, b) => a.departureTime.localeCompare(b.departureTime))
    : [];
  const panelDepartures = selectedCity ? selectedCity.departures.length : 0;

  return (
    <div className="container page">
      <div className="page-head">
        <h1 className="page-title">
          <MapIcon size={24} aria-hidden="true" /> מפת טיסות
        </h1>
        <ViewSwitcher />
      </div>

      <RouteFilter origin={query.origin} destination={query.destination} onChange={(values) => { setSelected(null); setQuery(values); }}>
        <label className="field">
          <span className="field__label">מתאריך</span>
          <input className="input" type="date" value={query.fromDate} onChange={(e) => setQuery({ fromDate: e.target.value })} />
        </label>
        <label className="field">
          <span className="field__label">עד תאריך</span>
          <input className="input" type="date" value={query.toDate} min={query.fromDate || undefined} onChange={(e) => setQuery({ toDate: e.target.value })} />
        </label>
      </RouteFilter>

      {status === 'error' && (
        <Alert variant="error">
          {errorMessage(error)}{' '}
          <button type="button" className="link-btn" onClick={reload}>
            נסו שוב
          </button>
        </Alert>
      )}
      {map.unknown.length > 0 && (
        <Alert variant="info" title="יעדים ללא מיקום במפה">
          לערים הבאות אין קואורדינטות בטבלת הערים של הלקוח, ולכן הן לא מוצגות במפה: <bdi>{map.unknown.join(', ')}</bdi>. הטיסות עצמן זמינות ברשימה.
        </Alert>
      )}

      <div className="map-layout">
        <div className="map-frame card" dir="ltr">
          {status === 'loading' && <div className="map-frame__loading"><Spinner label="טוען טיסות…" /></div>}
          <MapContainer
            key={boundsKey || 'empty'}
            className="map"
            center={[40, 20]}
            zoom={3}
            minZoom={2}
            bounds={bounds && bounds.length > 1 ? bounds : undefined}
            boundsOptions={{ padding: [48, 48], maxZoom: 6 }}
            worldCopyJump
            scrollWheelZoom
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
              url="https://tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            {map.routes.map((route) => {
              const active = !selectedCity || route.from.key === selected || route.to.key === selected;
              return (
                <Polyline
                  key={route.key}
                  positions={arc([route.from.geo.lat, route.from.geo.lng], [route.to.geo.lat, route.to.geo.lng])}
                  pathOptions={{
                    color: active ? COLORS.secondary : COLORS.muted,
                    weight: active ? 2.5 : 1.5,
                    opacity: active ? 0.9 : 0.4,
                    dashArray: '6 6',
                  }}
                />
              );
            })}

            {map.cities.map((city) => {
              const isDestination = city.arrivals.length > 0;
              const isSelected = city.key === selected;
              const price = formatPrice(minPrice(city.arrivals));
              return (
                <CircleMarker
                  key={city.key}
                  center={[city.geo.lat, city.geo.lng]}
                  radius={isSelected ? 11 : isDestination ? 8 : 7}
                  pathOptions={{
                    color: '#ffffff',
                    weight: 2,
                    fillColor: isSelected ? COLORS.accent : isDestination ? COLORS.primary : COLORS.secondary,
                    fillOpacity: 1,
                  }}
                  eventHandlers={{ click: () => setSelected(isSelected ? null : city.key) }}
                  bubblingMouseEvents={false}
                >
                  <Tooltip permanent {...labelDirection(city, map.cities)} className={`city-label ${isSelected ? 'is-selected' : ''}`}>
                    <span dir="auto">{city.label}</span>
                    {isDestination && price && <span className="city-label__price" dir="rtl">מ־{price}</span>}
                  </Tooltip>
                </CircleMarker>
              );
            })}
          </MapContainer>
        </div>

        <aside className="card map-panel" aria-live="polite">
          {selectedCity ? (
            <>
              <div className="map-panel__head">
                <h2 className="section-title">
                  טיסות אל <bdi>{selectedCity.label}</bdi>
                </h2>
                <button type="button" className="icon-btn" onClick={() => setSelected(null)} aria-label="סגירה">
                  <X size={16} />
                </button>
              </div>
              {panelDepartures > 0 && <p className="map-panel__note">ויוצאות ממנה {panelDepartures} טיסות.</p>}
              {panelFlights.length === 0 ? (
                <p className="booking-panel__empty">אין טיסות אל העיר הזו בסינון הנוכחי.</p>
              ) : (
                <ul className="map-flights">
                  {panelFlights.map((flight) => (
                    <li key={flight.id}>
                      <Link to={`/flights/${flight.id}`} className="map-flight">
                        <span className="map-flight__main">
                          <bdi className="flight-number">{flight.flightNumber}</bdi>
                          <span>
                            מ־<bdi>{flight.origin}</bdi>
                          </span>
                        </span>
                        <span className="map-flight__when">
                          {formatDate(flight.departureTime)} · {formatTime(flight.departureTime)}
                        </span>
                        <span className="map-flight__price">{formatPrice(flight.price) ?? 'אין מחיר'}</span>
                        <ChevronLeft size={16} aria-hidden="true" className="map-flight__go" />
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </>
          ) : (
            <div className="map-panel__intro">
              <h2 className="section-title">בחרו יעד במפה</h2>
              <p>לחיצה על נקודה מציגה את הטיסות אליה, עם המחירים מהשרת.</p>
              {data && (
                <p className="map-panel__note">
                  {data.items.length} טיסות · {map.cities.length} ערים · {map.routes.length} מסלולים
                </p>
              )}
              {map.cities.length > 0 && (
                <ul className="map-destinations">
                  {map.cities
                    .filter((c) => c.arrivals.length)
                    .map((city) => (
                      <li key={city.key}>
                        <button type="button" className="map-destination" onClick={() => setSelected(city.key)}>
                          <bdi>{city.label}</bdi>
                          <span>
                            {city.arrivals.length} טיסות
                            {formatPrice(minPrice(city.arrivals)) && ` · מ־${formatPrice(minPrice(city.arrivals))}`}
                          </span>
                        </button>
                      </li>
                    ))}
                </ul>
              )}
              {data && data.items.length === 0 && (
                <p className="booking-panel__empty">
                  <MapPinOff size={16} aria-hidden="true" /> אין טיסות בסינון הנוכחי.
                </p>
              )}
            </div>
          )}
        </aside>
      </div>
    </div>
  );
}
