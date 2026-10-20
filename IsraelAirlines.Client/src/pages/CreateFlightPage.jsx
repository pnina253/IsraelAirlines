import { useState } from 'react';
import { Link } from 'react-router-dom';
import { PlaneTakeoff, Warehouse } from 'lucide-react';
import { createFlight } from '../api/flights';
import { getAircraftList } from '../api/aircraft';
import { errorMessage, validationMessages } from '../api/errors';
import useApi from '../utils/useApi';
import { formatDateTime, formatDuration, formatPrice } from '../utils/format';
import AircraftDetails from '../components/AircraftDetails';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import Field from '../components/Field';

const EMPTY = { flightNumber: '', origin: '', destination: '', departure: '', durationMinutes: '', price: '', registrationNumber: '' };

// Mirrors FlightRequest validation and FlightService rules; the server checks them again.
function validate(form) {
  const errors = {};
  if (!form.flightNumber.trim()) errors.flightNumber = 'יש להזין מספר טיסה.';
  if (!form.origin.trim()) errors.origin = 'יש להזין מוצא.';
  if (!form.destination.trim()) errors.destination = 'יש להזין יעד.';
  if (form.origin.trim() && form.origin.trim().toLowerCase() === form.destination.trim().toLowerCase()) {
    errors.destination = 'היעד חייב להיות שונה מהמוצא.';
  }
  if (!form.departure) errors.departure = 'יש לבחור מועד המראה.';
  else if (new Date(form.departure) <= new Date()) errors.departure = 'מועד ההמראה חייב להיות בעתיד.';
  const duration = Number(form.durationMinutes);
  if (!Number.isInteger(duration) || duration < 1 || duration > 3000) errors.durationMinutes = 'משך בדקות, בין 1 ל־3000.';
  const price = Number(form.price);
  if (!form.price || Number.isNaN(price) || price < 0.01 || price > 100000) errors.price = 'מחיר בין 0.01 ל־100,000 ₪.';
  if (!form.registrationNumber) errors.registrationNumber = 'יש לבחור מטוס.';
  return errors;
}

export default function CreateFlightPage() {
  const aircraft = useApi((signal) => getAircraftList({ signal }), []);
  const [form, setForm] = useState(EMPTY);
  const [fieldErrors, setFieldErrors] = useState({});
  const [result, setResult] = useState({ status: 'idle' });

  const update = (field) => (event) => {
    setForm((current) => ({ ...current, [field]: event.target.value }));
    setFieldErrors((current) => ({ ...current, [field]: undefined }));
  };
  const selected = aircraft.data?.find((a) => a.registrationNumber === form.registrationNumber) ?? null;

  async function handleSubmit(event) {
    event.preventDefault();
    const errors = validate(form);
    setFieldErrors(errors);
    if (Object.keys(errors).length) return;

    setResult({ status: 'submitting' });
    try {
      const flight = await createFlight({
        flightNumber: form.flightNumber.trim(),
        origin: form.origin.trim(),
        destination: form.destination.trim(),
        departureTime: new Date(form.departure).toISOString(), // local time from the picker, sent as UTC
        durationMinutes: Number(form.durationMinutes),
        price: Number(form.price),
        registrationNumber: form.registrationNumber,
      });
      setResult({ status: 'success', flight });
      setForm(EMPTY);
      setFieldErrors({});
    } catch (error) {
      setResult({ status: 'error', error });
    }
  }

  const submitting = result.status === 'submitting';

  return (
    <div className="container page">
      <div className="page-head">
        <h1 className="page-title">
          <PlaneTakeoff size={24} aria-hidden="true" /> יצירת טיסה
        </h1>
        <Link to="/admin/aircraft" className="btn btn--ghost">
          <Warehouse size={16} aria-hidden="true" /> צי המטוסים
        </Link>
      </div>

      <div className="admin-layout">
        <form className="card form form-card" onSubmit={handleSubmit} noValidate>
          <div className="form-grid">
            <Field label="מספר טיסה" error={fieldErrors.flightNumber}>
              <input className="input" dir="ltr" value={form.flightNumber} onChange={update('flightNumber')} maxLength={20} placeholder="IA303" />
            </Field>
            <Field label="מטוס" error={fieldErrors.registrationNumber}>
              {aircraft.status === 'loading' && !aircraft.data ? (
                <Spinner inline label="טוען מטוסים…" />
              ) : (
                <select className="input" value={form.registrationNumber} onChange={update('registrationNumber')}>
                  <option value="">בחירת מטוס…</option>
                  {aircraft.data?.map((a) => (
                    <option key={a.id} value={a.registrationNumber} disabled={!a.isActive}>
                      {a.model} · {a.registrationNumber}
                      {a.isActive ? '' : ' (לא פעיל)'}
                    </option>
                  ))}
                </select>
              )}
            </Field>
            <Field label="מוצא" error={fieldErrors.origin}>
              <input className="input" dir="auto" value={form.origin} onChange={update('origin')} maxLength={100} placeholder="Tel Aviv" />
            </Field>
            <Field label="יעד" error={fieldErrors.destination}>
              <input className="input" dir="auto" value={form.destination} onChange={update('destination')} maxLength={100} placeholder="Rome" />
            </Field>
            <Field label="מועד המראה (שעון מקומי)" error={fieldErrors.departure}>
              <input className="input" type="datetime-local" value={form.departure} onChange={update('departure')} />
            </Field>
            <Field
              label="משך הטיסה (דקות)"
              error={fieldErrors.durationMinutes}
              hint={Number(form.durationMinutes) > 0 ? formatDuration(Number(form.durationMinutes)) : null}
            >
              <input className="input" type="number" min={1} max={3000} value={form.durationMinutes} onChange={update('durationMinutes')} />
            </Field>
            <Field label="מחיר כרטיס (₪)" error={fieldErrors.price}>
              <input className="input" type="number" min={0.01} max={100000} step="0.01" value={form.price} onChange={update('price')} />
            </Field>
          </div>

          {aircraft.status === 'error' && <Alert variant="error">{errorMessage(aircraft.error)}</Alert>}

          {result.status === 'error' && (
            <Alert variant="error" details={validationMessages(result.error)}>
              {errorMessage(result.error, {
                409: 'כבר קיימת טיסה עם המספר הזה.',
                404: 'המטוס שנבחר לא נמצא.',
                400: result.error.problem?.detail ? `השרת דחה את הנתונים: ${result.error.problem.detail}` : undefined,
              })}
            </Alert>
          )}

          {result.status === 'success' && (
            <Alert variant="success" title="הטיסה נוצרה בשרת">
              <p>
                <bdi>{result.flight.flightNumber}</bdi> · <bdi>{result.flight.origin}</bdi> ← <bdi>{result.flight.destination}</bdi> ·{' '}
                {formatDateTime(result.flight.departureTime)} · {formatPrice(result.flight.price)}
              </p>
              <Link to={`/flights/${result.flight.id}`} className="link-btn">
                לדף הטיסה
              </Link>
            </Alert>
          )}

          <button type="submit" className="btn btn--accent btn--lg" disabled={submitting}>
            <PlaneTakeoff size={18} aria-hidden="true" /> {submitting ? 'שולח לשרת…' : 'יצירת טיסה'}
          </button>
        </form>

        <aside className="card admin-aside" aria-label="פרטי המטוס שנבחר">
          <h2 className="section-title">המטוס שנבחר</h2>
          {selected ? <AircraftDetails aircraft={selected} /> : <p className="booking-panel__empty">בחרו מטוס כדי לראות את פרטיו.</p>}
        </aside>
      </div>
    </div>
  );
}
