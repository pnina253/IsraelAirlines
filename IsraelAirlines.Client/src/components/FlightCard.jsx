import { Link } from 'react-router-dom';
import { ChevronLeft, Clock, Plane } from 'lucide-react';
import { arrivesNextDay, formatDate, formatDuration, formatPrice, formatTime } from '../utils/format';

export default function FlightCard({ flight }) {
  const price = formatPrice(flight.price);
  const nextDay = arrivesNextDay(flight.departureTime, flight.arrivalTime);

  return (
    <article className="flight-card" aria-labelledby={`flight-${flight.id}-title`}>
      <div className="flight-card__meta">
        <span className="flight-number" id={`flight-${flight.id}-title`}>
          <bdi>{flight.flightNumber}</bdi>
        </span>
        <span className="flight-card__date">{formatDate(flight.departureTime)}</span>
        <span className="flight-card__aircraft">
          <bdi>{flight.aircraftModel}</bdi>
        </span>
      </div>

      <div className="route">
        <div className="route__point">
          <span className="route__time">{formatTime(flight.departureTime)}</span>
          <span className="route__city">
            <bdi>{flight.origin}</bdi>
          </span>
        </div>

        <div className="route__line" aria-hidden="true">
          <span className="route__duration">
            <Clock size={13} /> {formatDuration(flight.durationMinutes)}
          </span>
          <span className="route__track">
            <Plane size={16} className="route__plane" />
          </span>
        </div>

        <div className="route__point route__point--end">
          <span className="route__time">
            {formatTime(flight.arrivalTime)}
            {nextDay && <sup className="route__nextday" title="נחיתה ביום שלמחרת">+1</sup>}
          </span>
          <span className="route__city">
            <bdi>{flight.destination}</bdi>
          </span>
        </div>
      </div>

      <div className="flight-card__action">
        {price ? (
          <span className="price">
            <span className="price__label">מחיר</span>
            <span className="price__value">{price}</span>
          </span>
        ) : (
          <span className="price price--missing">אין מחיר</span>
        )}
        <Link to={`/flights/${flight.id}`} className="btn btn--primary">
          בחירת מושב <ChevronLeft size={16} aria-hidden="true" />
        </Link>
      </div>
    </article>
  );
}
