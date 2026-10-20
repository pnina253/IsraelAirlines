import { Link } from 'react-router-dom';
import { ChevronLeft, ChevronRight, GanttChart } from 'lucide-react';
import { fetchAllFlights } from '../api/flights';
import { errorMessage } from '../api/errors';
import useApi from '../utils/useApi';
import useQueryState from '../utils/useQueryState';
import { formatTime } from '../utils/format';
import { addDays, dayTitle, parseISODate, serverRangeFor, shortDay, toISODate } from '../utils/dates';
import ViewSwitcher from '../components/ViewSwitcher';
import RouteFilter from '../components/RouteFilter';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import EmptyState from '../components/EmptyState';

const HOUR = 60 * 60 * 1000;
const SPANS = { 1: 'יום', 3: '3 ימים', 7: 'שבוע' };
const TICK_HOURS = { 1: 2, 3: 6, 7: 12 };

// Puts overlapping flights on separate lanes: each flight takes the first lane that is free at its departure.
function assignLanes(flights) {
  const laneEnds = [];
  return flights.map((flight) => {
    const start = new Date(flight.departureTime).getTime();
    let lane = laneEnds.findIndex((end) => end <= start);
    if (lane === -1) lane = laneEnds.length;
    laneEnds[lane] = new Date(flight.arrivalTime).getTime();
    return { flight, lane };
  });
}

export default function TimelinePage() {
  const [query, setQuery, queryKey] = useQueryState({ date: toISODate(new Date()), span: '1', origin: '', destination: '' });
  const span = SPANS[query.span] ? Number(query.span) : 1;
  const start = parseISODate(query.date);
  const end = addDays(start, span);
  const rangeMs = end - start;

  const { status, data, error, reload } = useApi(
    (signal) =>
      fetchAllFlights(
        { origin: query.origin, destination: query.destination, ...serverRangeFor(addDays(start, -1), addDays(end, -1)) },
        { signal },
      ),
    [queryKey],
  );

  // Flights that are in the air at any point inside the window (a flight may start the day before).
  const visible = (data?.items ?? [])
    .filter((f) => new Date(f.departureTime) < end && new Date(f.arrivalTime) > start)
    .sort((a, b) => a.departureTime.localeCompare(b.departureTime));
  const placed = assignLanes(visible);
  const laneCount = Math.max(1, ...placed.map((p) => p.lane + 1));

  const pct = (time) => Math.min(100, Math.max(0, ((new Date(time) - start) / rangeMs) * 100));
  const ticks = Array.from({ length: (span * 24) / TICK_HOURS[span] + 1 }, (_, i) => new Date(start.getTime() + i * TICK_HOURS[span] * HOUR));
  const now = new Date();
  const showNow = now >= start && now < end;

  return (
    <div className="container page">
      <div className="page-head">
        <h1 className="page-title">
          <GanttChart size={24} aria-hidden="true" /> ציר זמן
        </h1>
        <ViewSwitcher />
      </div>

      <RouteFilter origin={query.origin} destination={query.destination} onChange={(values) => setQuery(values)}>
        <label className="field">
          <span className="field__label">מתאריך</span>
          <input className="input" type="date" value={query.date} onChange={(e) => e.target.value && setQuery({ date: e.target.value })} />
        </label>
      </RouteFilter>

      <div className="cal-toolbar">
        <div className="cal-toolbar__nav">
          <button type="button" className="icon-btn" onClick={() => setQuery({ date: toISODate(addDays(start, -span)) })} aria-label="הטווח הקודם">
            <ChevronRight size={18} />
          </button>
          <h2 className="cal-toolbar__title" aria-live="polite">
            {span === 1 ? dayTitle.format(start) : `${shortDay.format(start)} – ${shortDay.format(addDays(end, -1))}`}
          </h2>
          <button type="button" className="icon-btn" onClick={() => setQuery({ date: toISODate(addDays(start, span)) })} aria-label="הטווח הבא">
            <ChevronLeft size={18} />
          </button>
          <button type="button" className="btn btn--ghost btn--sm" onClick={() => setQuery({ date: toISODate(new Date()) })}>
            היום
          </button>
        </div>
        <div className="segmented" role="group" aria-label="טווח">
          {Object.entries(SPANS).map(([value, label]) => (
            <button key={value} type="button" aria-pressed={String(span) === value} onClick={() => setQuery({ span: value })}>
              {label}
            </button>
          ))}
        </div>
        {data && <span className="cal-toolbar__count">{visible.length} טיסות בטווח</span>}
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

      {data && visible.length === 0 ? (
        <EmptyState icon={GanttChart} title="אין טיסות בטווח הזה" text="עברו לתאריך אחר או הרחיבו את הטווח." />
      ) : (
        <div className="timeline-scroll">
          <div className="timeline">
            <div className="timeline__axis" aria-hidden="true">
              {ticks.map((tick, index) => (
                <span key={tick.getTime()} className="timeline__tick" style={{ insetInlineStart: `${(index / (ticks.length - 1)) * 100}%` }}>
                  {tick.getHours() === 0 && span > 1 ? shortDay.format(tick) : formatTime(tick)}
                </span>
              ))}
            </div>

            <div className="timeline__lanes" style={{ height: laneCount * 52 + 12 }}>
              {ticks.map((tick, index) => (
                <span
                  key={tick.getTime()}
                  aria-hidden="true"
                  className={`timeline__gridline ${tick.getHours() === 0 ? 'is-midnight' : ''}`}
                  style={{ insetInlineStart: `${(index / (ticks.length - 1)) * 100}%` }}
                />
              ))}
              {showNow && <span className="timeline__now" aria-hidden="true" style={{ insetInlineStart: `${pct(now)}%` }} />}

              <ol className="timeline__list" aria-label="טיסות לפי זמן">

              {placed.map(({ flight, lane }) => {
                const left = pct(flight.departureTime);
                const width = Math.max(pct(flight.arrivalTime) - left, 0.8);
                return (
                  <li key={flight.id} className="timeline__item" style={{ insetInlineStart: `${left}%`, width: `${width}%`, top: lane * 52 + 6 }}>
                    <Link
                      to={`/flights/${flight.id}`}
                      className="timeline__bar"
                      title={`${flight.flightNumber} · ${flight.origin} ← ${flight.destination} · ${formatTime(flight.departureTime)}–${formatTime(flight.arrivalTime)}`}
                    >
                      <bdi className="timeline__bar-number">{flight.flightNumber}</bdi>
                      <span className="timeline__bar-route">
                        <bdi>{flight.origin}</bdi> ← <bdi>{flight.destination}</bdi>
                      </span>
                      <span className="timeline__bar-time">
                        {formatTime(flight.departureTime)}–{formatTime(flight.arrivalTime)}
                      </span>
                    </Link>
                  </li>
                );
              })}
              </ol>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
