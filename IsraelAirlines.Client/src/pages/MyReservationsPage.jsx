import { Link } from 'react-router-dom';
import { Ticket } from 'lucide-react';
import { getMyReservations } from '../api/reservations';
import { errorMessage } from '../api/errors';
import useApi from '../utils/useApi';
import { formatDateTime } from '../utils/format';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import EmptyState from '../components/EmptyState';

export default function MyReservationsPage() {
  const { status, data, error, reload } = useApi((signal) => getMyReservations({ signal }), []);

  return (
    <div className="container page">
      <h1 className="page-title">
        <Ticket size={24} aria-hidden="true" /> ההזמנות שלי
      </h1>

      {status === 'loading' && !data && <Spinner label="טוען הזמנות…" />}

      {status === 'error' && (
        <Alert variant="error">
          {errorMessage(error)}{' '}
          <button type="button" className="link-btn" onClick={reload}>
            נסו שוב
          </button>
        </Alert>
      )}

      {data && data.length === 0 && (
        <EmptyState
          icon={Ticket}
          title="עדיין אין לך הזמנות"
          text="בחרו טיסה ומושב כדי להתחיל."
          action={
            <Link to="/flights" className="btn btn--primary">
              לרשימת הטיסות
            </Link>
          }
        />
      )}

      {data && data.length > 0 && (
        <div className="card table-card">
          <table className="table">
            <caption className="sr-only">רשימת ההזמנות שלך</caption>
            <thead>
              <tr>
                <th scope="col">מספר הזמנה</th>
                <th scope="col">טיסה</th>
                <th scope="col">מושב</th>
                <th scope="col">נוצרה</th>
              </tr>
            </thead>
            <tbody>
              {data.map((reservation) => (
                <tr key={reservation.id}>
                  <td>
                    <bdi>#{reservation.id}</bdi>
                  </td>
                  <td>
                    <bdi className="flight-number">{reservation.flightNumber}</bdi>
                  </td>
                  <td>
                    <span className="seat-tag">
                      <bdi>{reservation.seatNumber}</bdi>
                    </span>
                  </td>
                  <td>{formatDateTime(reservation.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
