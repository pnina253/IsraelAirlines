import { Link } from 'react-router-dom';
import { CalendarDays, ChevronLeft, ChevronRight } from 'lucide-react';
import { fetchAllFlights } from '../api/flights';
import { errorMessage } from '../api/errors';
import useApi from '../utils/useApi';
import useQueryState from '../utils/useQueryState';
import { formatPrice, formatTime } from '../utils/format';
import {
  WEEKDAYS,
  addDays,
  addMonths,
  dayTitle,
  isSameDay,
  monthTitle,
  parseISODate,
  serverRangeFor,
  shortDay,
  startOfMonth,
  startOfWeek,
  toISODate,
} from '../utils/dates';
import ViewSwitcher from '../components/ViewSwitcher';
import RouteFilter from '../components/RouteFilter';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';

const MAX_IN_CELL = 3;

function visibleDays(view, anchor) {
  const first = view === 'week' ? startOfWeek(anchor) : startOfWeek(startOfMonth(anchor));
  const count = view === 'week' ? 7 : 42;
  return Array.from({ length: count }, (_, i) => addDays(first, i));
}

export default function CalendarPage() {
  const today = new Date();
  const [query, setQuery, queryKey] = useQueryState({ view: 'month', date: toISODate(today), origin: '', destination: '' });
  const anchor = parseISODate(query.date);
  const days = visibleDays(query.view, anchor);
  const firstDay = days[0];
  const lastDay = days[days.length - 1];

  const { status, data, error, reload } = useApi(
    (signal) =>
      fetchAllFlights({ origin: query.origin, destination: query.destination, ...serverRangeFor(firstDay, lastDay) }, { signal }),
    [queryKey],
  );

  // Group by local departure day.
  const byDay = new Map();
  for (const flight of data?.items ?? []) {
    const key = toISODate(new Date(flight.departureTime));
    if (!byDay.has(key)) byDay.set(key, []);
    byDay.get(key).push(flight);
  }

  function move(step) {
    const next = query.view === 'week' ? addDays(anchor, 7 * step) : addMonths(anchor, step);
    setQuery({ date: toISODate(next) });
  }

  const title =
    query.view === 'week' ? `${shortDay.format(firstDay)} – ${shortDay.format(lastDay)}` : monthTitle.format(anchor);
  const inRange = (data?.items ?? []).filter((f) => {
    const d = new Date(f.departureTime);
    return d >= firstDay && d < addDays(lastDay, 1);
  }).length;

  return (
    <div className="container page">
      <div className="page-head">
        <h1 className="page-title">
          <CalendarDays size={24} aria-hidden="true" /> לוח טיסות
        </h1>
        <ViewSwitcher />
      </div>

      <RouteFilter origin={query.origin} destination={query.destination} onChange={(values) => setQuery(values)} />

      <div className="cal-toolbar">
        <div className="cal-toolbar__nav">
          <button type="button" className="icon-btn" onClick={() => move(-1)} aria-label="התקופה הקודמת">
            <ChevronRight size={18} />
          </button>
          <h2 className="cal-toolbar__title" aria-live="polite">
            {title}
          </h2>
          <button type="button" className="icon-btn" onClick={() => move(1)} aria-label="התקופה הבאה">
            <ChevronLeft size={18} />
          </button>
          <button type="button" className="btn btn--ghost btn--sm" onClick={() => setQuery({ date: toISODate(today) })}>
            היום
          </button>
        </div>
        <div className="segmented" role="group" aria-label="סוג תצוגה">
          {[
            ['month', 'חודש'],
            ['week', 'שבוע'],
          ].map(([value, label]) => (
            <button key={value} type="button" aria-pressed={query.view === value} onClick={() => setQuery({ view: value })}>
              {label}
            </button>
          ))}
        </div>
        {data && <span className="cal-toolbar__count">{inRange} טיסות בתקופה</span>}
        {status === 'loading' && <Spinner inline label="טוען…" />}
      </div>

      {status === 'error' && (
        <Alert variant="error">
          {errorMessage(error)}{' '}
          <button type="button" className="link-btn" onClick={reload}>
            נסו שוב
          </button>
        </Alert>
      )}
      {data?.truncated && <Alert variant="warning">יש יותר מ־1,000 טיסות בתקופה; מוצגות הראשונות בלבד. צמצמו את הסינון.</Alert>}

      <div className={`calendar calendar--${query.view}`}>
        <div className="calendar__weekdays" aria-hidden="true">
          {WEEKDAYS.map((day) => (
            <span key={day} className="calendar__weekday">
              {day}
            </span>
          ))}
        </div>
        <div className="calendar__grid">
          {days.map((day) => {
            const key = toISODate(day);
            const flights = (byDay.get(key) ?? []).sort((a, b) => a.departureTime.localeCompare(b.departureTime));
            const outside = query.view === 'month' && day.getMonth() !== anchor.getMonth();
            const limit = query.view === 'month' ? MAX_IN_CELL : flights.length;

            return (
              <div
                key={key}
                className={['calendar__day', outside && 'is-outside', isSameDay(day, today) && 'is-today'].filter(Boolean).join(' ')}
              >
                <span className="sr-only">{`${dayTitle.format(day)}, ${flights.length} טיסות`}</span>
                <span className="calendar__date" aria-hidden="true">{query.view === 'week' ? shortDay.format(day) : day.getDate()}</span>
                <ul className="calendar__flights">
                  {flights.slice(0, limit).map((flight) => (
                    <li key={flight.id}>
                      <Link to={`/flights/${flight.id}`} className="cal-chip" title={`${flight.flightNumber} ${flight.origin} ← ${flight.destination}`}>
                        <span className="cal-chip__time">{formatTime(flight.departureTime)}</span>
                        <bdi className="cal-chip__route">{flight.destination}</bdi>
                        {query.view === 'week' && (
                          <span className="cal-chip__more">
                            <bdi>{flight.flightNumber}</bdi> · מ־<bdi>{flight.origin}</bdi>
                            {formatPrice(flight.price) && ` · ${formatPrice(flight.price)}`}
                          </span>
                        )}
                      </Link>
                    </li>
                  ))}
                </ul>
                {flights.length > limit && (
                  <button type="button" className="link-btn calendar__more" onClick={() => setQuery({ view: 'week', date: key })}>
                    +{flights.length - limit} נוספות
                  </button>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
