import { Link } from 'react-router-dom';
import { PlaneTakeoff, Warehouse } from 'lucide-react';
import { getAircraftList } from '../api/aircraft';
import { errorMessage } from '../api/errors';
import useApi from '../utils/useApi';
import AircraftDetails from '../components/AircraftDetails';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import EmptyState from '../components/EmptyState';

export default function AircraftPage() {
  const { status, data, error, reload } = useApi((signal) => getAircraftList({ signal }), []);

  return (
    <div className="container page">
      <div className="page-head">
        <h1 className="page-title">
          <Warehouse size={24} aria-hidden="true" /> צי המטוסים
        </h1>
        <Link to="/admin/flights/new" className="btn btn--accent">
          <PlaneTakeoff size={16} aria-hidden="true" /> יצירת טיסה
        </Link>
      </div>

      {status === 'loading' && !data && <Spinner label="טוען מטוסים…" />}
      {status === 'error' && (
        <Alert variant="error">
          {errorMessage(error)}{' '}
          <button type="button" className="link-btn" onClick={reload}>
            נסו שוב
          </button>
        </Alert>
      )}
      {data && data.length === 0 && <EmptyState icon={Warehouse} title="אין מטוסים במערכת" />}
      {data && data.length > 0 && (
        <div className="aircraft-grid">
          {data.map((aircraft) => (
            <article key={aircraft.id} className="card aircraft-card">
              <AircraftDetails aircraft={aircraft} />
            </article>
          ))}
        </div>
      )}
    </div>
  );
}
