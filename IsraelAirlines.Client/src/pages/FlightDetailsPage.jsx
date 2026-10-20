import { useCallback, useEffect, useRef, useState } from 'react';
import { Link, useLocation, useParams } from 'react-router-dom';
import { ArrowRight, Armchair, Clock, Lock, Plane, RefreshCw, Ticket, Users } from 'lucide-react';
import { getFlight, getReservedSeats } from '../api/flights';
import { getAircraft } from '../api/aircraft';
import { createReservation } from '../api/reservations';
import { errorMessage, validationMessages } from '../api/errors';
import { useAuth } from '../auth/AuthContext';
import useApi from '../utils/useApi';
import { arrivesNextDay, formatDate, formatDateTime, formatDuration, formatPrice, formatTime } from '../utils/format';
import { CABIN_LABELS, POSITION_LABELS, describeSeat } from '../utils/seats';
import SeatMap, { SeatLegend } from '../components/SeatMap';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import EmptyState from '../components/EmptyState';

const timeOnly = new Intl.DateTimeFormat('he-IL', { hour: '2-digit', minute: '2-digit', second: '2-digit' });

export default function FlightDetailsPage() {
  const { id } = useParams();
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  const details = useApi(async (signal) => {
    const flight = await getFlight(id, { signal });
    const aircraft = await getAircraft(flight.aircraftId, { signal });
    return { flight, aircraft };
  }, [id]);

  // Reserved seats always come from the server; after any booking attempt they are fetched again.
  const [seats, setSeats] = useState({ status: 'idle', list: [], error: null, updatedAt: null });
  const seatsRequest = useRef(0);

  const loadSeats = useCallback(async () => {
    const requestId = ++seatsRequest.current;
    setSeats((current) => ({ ...current, status: 'loading', error: null }));
    try {
      const list = await getReservedSeats(id);
      if (requestId === seatsRequest.current) {
        setSeats({ status: 'success', list, error: null, updatedAt: new Date() });
      }
    } catch (error) {
      if (requestId === seatsRequest.current) {
        setSeats((current) => ({ ...current, status: 'error', error }));
      }
    }
  }, [id]);

  useEffect(() => {
    if (isAuthenticated) loadSeats();
    else setSeats({ status: 'idle', list: [], error: null, updatedAt: null });
  }, [isAuthenticated, loadSeats]);

  const [selectedSeat, setSelectedSeat] = useState(null);
  const [conflictSeat, setConflictSeat] = useState(null);
  const [booking, setBooking] = useState({ status: 'idle' });

  function selectSeat(seat) {
    setSelectedSeat(seat);
    if (booking.status !== 'submitting' && booking.status !== 'success') setBooking({ status: 'idle' });
  }

  async function handleBook() {
    const seat = selectedSeat;
    setBooking({ status: 'submitting', seat });

    try {
      // Success is shown only after the server answers 201 with the created reservation.
      const reservation = await createReservation(details.data.flight.flightNumber, seat);
      setBooking({ status: 'success', reservation });
      setSelectedSeat(null);
      setConflictSeat(null);
    } catch (error) {
      if (error.status === 409) {
        // The real 409 from the server: someone else holds this seat.
        setBooking({ status: 'conflict', seat, error });
        setConflictSeat(seat);
        setSelectedSeat(null);
      } else {
        setBooking({ status: 'error', seat, error });
      }
    }

    loadSeats();
  }

  if (details.status === 'loading' && !details.data) return <Spinner label="טוען פרטי טיסה…" />;

  if (details.status === 'error') {
    return (
      <div className="container page">
        <EmptyState
          icon={Plane}
          title={details.error.status === 404 ? 'הטיסה לא נמצאה' : 'לא ניתן לטעון את הטיסה'}
          text={details.error.status === 404 ? 'ייתכן שהקישור שגוי או שהטיסה הוסרה.' : errorMessage(details.error)}
          action={
            <Link to="/flights" className="btn btn--secondary">
              חזרה לרשימת הטיסות
            </Link>
          }
        />
      </div>
    );
  }

  const { flight, aircraft } = details.data;
  const price = formatPrice(flight.price);
  const totalSeats = aircraft.rows * aircraft.seatLayout.replace(/ /g, '').length;
  const selectedInfo = selectedSeat ? describeSeat(aircraft, selectedSeat) : null;
  const submitting = booking.status === 'submitting';

  return (
    <div className="container page">
      <Link to="/flights" className="back-link">
        <ArrowRight size={16} aria-hidden="true" /> חזרה לרשימת הטיסות
      </Link>

      <section className="flight-header card" aria-labelledby="flight-title">
        <div className="flight-header__top">
          <h1 id="flight-title" className="flight-header__title">
            טיסה <bdi className="flight-number flight-number--lg">{flight.flightNumber}</bdi>
          </h1>
          {price ? (
            <span className="price price--lg">
              <span className="price__label">מחיר כרטיס</span>
              <span className="price__value">{price}</span>
            </span>
          ) : (
            <span className="price price--missing">אין מחיר</span>
          )}
        </div>

        <div className="route route--lg">
          <div className="route__point">
            <span className="route__time">{formatTime(flight.departureTime)}</span>
            <span className="route__city">
              <bdi>{flight.origin}</bdi>
            </span>
            <span className="route__date">{formatDate(flight.departureTime)}</span>
          </div>
          <div className="route__line" aria-hidden="true">
            <span className="route__duration">
              <Clock size={13} /> {formatDuration(flight.durationMinutes)}
            </span>
            <span className="route__track">
              <Plane size={18} className="route__plane" />
            </span>
          </div>
          <div className="route__point route__point--end">
            <span className="route__time">
              {formatTime(flight.arrivalTime)}
              {arrivesNextDay(flight.departureTime, flight.arrivalTime) && <sup className="route__nextday">+1</sup>}
            </span>
            <span className="route__city">
              <bdi>{flight.destination}</bdi>
            </span>
            <span className="route__date">{formatDate(flight.arrivalTime)}</span>
          </div>
        </div>

        <dl className="facts">
          <div>
            <dt>מטוס</dt>
            <dd>
              <bdi>{flight.aircraftModel}</bdi>
            </dd>
          </div>
          <div>
            <dt>מספר רישום</dt>
            <dd>
              <bdi>{flight.registrationNumber}</bdi>
            </dd>
          </div>
          <div>
            <dt>תצורת מושבים</dt>
            <dd>
              {aircraft.rows} שורות · <bdi>{aircraft.seatLayout}</bdi>
            </dd>
          </div>
          {seats.status !== 'idle' && (
            <div>
              <dt>מושבים פנויים</dt>
              <dd>
                {totalSeats - seats.list.length} מתוך {totalSeats}
              </dd>
            </div>
          )}
        </dl>
      </section>

      {!isAuthenticated ? (
        <EmptyState
          icon={Lock}
          title="התחברו כדי לבחור מושב"
          text="מפת המושבים והזמנה זמינות למשתמשים מחוברים בלבד."
          action={
            <Link to="/login" state={{ from: location }} className="btn btn--primary">
              התחברות
            </Link>
          }
        />
      ) : (
        <div className="booking-layout">
          <section className="card seat-card" aria-labelledby="seat-title">
            <div className="seat-card__header">
              <h2 id="seat-title" className="section-title">
                <Armchair size={20} aria-hidden="true" /> בחירת מושב
              </h2>
              <button
                type="button"
                className="btn btn--ghost btn--sm"
                onClick={loadSeats}
                disabled={seats.status === 'loading'}
              >
                <RefreshCw size={15} aria-hidden="true" className={seats.status === 'loading' ? 'spin' : ''} />
                רענון זמינות
              </button>
            </div>
            <p className="seat-card__updated" aria-live="polite">
              {seats.updatedAt ? `הזמינות עודכנה מהשרת ב־${timeOnly.format(seats.updatedAt)}` : 'טוען זמינות מהשרת…'}
            </p>

            <SeatLegend hasBusiness={aircraft.businessRows > 0} />

            {seats.status === 'error' && (
              <Alert variant="error">
                {errorMessage(seats.error)}{' '}
                <button type="button" className="link-btn" onClick={loadSeats}>
                  נסו שוב
                </button>
              </Alert>
            )}

            {seats.updatedAt ? (
              <div className="seatmap-scroll">
                <SeatMap
                  aircraft={aircraft}
                  reservedSeats={seats.list}
                  selectedSeat={selectedSeat}
                  conflictSeat={conflictSeat}
                  onSelect={selectSeat}
                  disabled={submitting}
                />
              </div>
            ) : (
              seats.status !== 'error' && <Spinner label="טוען מפת מושבים…" />
            )}
          </section>

          <aside className="card booking-panel" aria-labelledby="booking-title">
            <h2 id="booking-title" className="section-title">
              <Ticket size={20} aria-hidden="true" /> סיכום הזמנה
            </h2>

            {selectedInfo ? (
              <div className="selected-seat">
                <span className="selected-seat__number">
                  <bdi>{selectedSeat}</bdi>
                </span>
                <ul className="selected-seat__info">
                  <li>{CABIN_LABELS[selectedInfo.cabin]}</li>
                  <li>מושב {POSITION_LABELS[selectedInfo.position]}</li>
                  <li>שורה {selectedInfo.row}</li>
                </ul>
              </div>
            ) : (
              <p className="booking-panel__empty">בחרו מושב פנוי במפה.</p>
            )}

            <dl className="booking-panel__summary">
              <div>
                <dt>טיסה</dt>
                <dd>
                  <bdi>{flight.flightNumber}</bdi>
                </dd>
              </div>
              <div>
                <dt>מסלול</dt>
                <dd>
                  <bdi>{flight.origin}</bdi> ← <bdi>{flight.destination}</bdi>
                </dd>
              </div>
              <div>
                <dt>המראה</dt>
                <dd>{formatDateTime(flight.departureTime)}</dd>
              </div>
              <div>
                <dt>מחיר</dt>
                <dd>{price ?? 'אין מחיר'}</dd>
              </div>
            </dl>

            <button
              type="button"
              className="btn btn--accent btn--block btn--lg"
              onClick={handleBook}
              disabled={!selectedSeat || submitting}
            >
              {submitting ? 'שולח לשרת…' : selectedSeat ? `הזמנת מושב ${selectedSeat}` : 'הזמנת מושב'}
            </button>

            <BookingResult booking={booking} />

            {import.meta.env.DEV && (
              <p className="demo-tip">
                <Users size={14} aria-hidden="true" />
                לבדיקת התנגשות: פתחו חלון גלישה בסתר, התחברו כמשתמש אחר ונסו להזמין את אותו מושב.
              </p>
            )}
          </aside>
        </div>
      )}
    </div>
  );
}

function BookingResult({ booking }) {
  if (booking.status === 'success') {
    const { reservation } = booking;
    return (
      <Alert variant="success" title="ההזמנה אושרה על ידי השרת">
        <p>
          מושב <strong><bdi>{reservation.seatNumber}</bdi></strong> בטיסה <bdi>{reservation.flightNumber}</bdi> שמור עבורך.
        </p>
        <p className="alert__meta">
          מספר הזמנה <bdi>#{reservation.id}</bdi> · נוצרה {formatDateTime(reservation.createdAt)}
        </p>
        <Link to="/my-reservations" className="link-btn">
          להזמנות שלי
        </Link>
      </Alert>
    );
  }

  if (booking.status === 'conflict') {
    return (
      <Alert variant="warning" title={`המושב ${booking.seat} כבר נתפס`}>
        <p>בחרו מושב אחר או נסו טיסה אחרת. מפת המושבים עודכנה מהשרת.</p>
        <p className="alert__meta">תשובת השרת: 409 Conflict</p>
      </Alert>
    );
  }

  if (booking.status === 'error') {
    const { error } = booking;
    return (
      <Alert
        variant="error"
        details={validationMessages(error)}
        correlationId={error.status >= 500 ? error.correlationId : null}
      >
        {errorMessage(error, {
          400: 'המושב שנבחר אינו קיים במטוס הזה.',
          404: 'הטיסה כבר לא קיימת במערכת.',
          401: 'פג תוקף ההתחברות. יש להתחבר מחדש.',
        })}
      </Alert>
    );
  }

  return null;
}
