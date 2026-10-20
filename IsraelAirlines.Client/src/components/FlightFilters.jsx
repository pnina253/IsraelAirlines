import { useEffect, useState } from 'react';
import { ArrowLeftRight, CalendarDays, MapPin, Search, X } from 'lucide-react';

const EMPTY = { origin: '', destination: '', fromDate: '', toDate: '' };

// Filters are sent to the server; nothing is filtered in the browser.
export default function FlightFilters({ value, onSearch }) {
  const [form, setForm] = useState({ ...EMPTY, ...value });
  const [error, setError] = useState(null);

  useEffect(() => {
    setForm({ ...EMPTY, ...value });
  }, [value.origin, value.destination, value.fromDate, value.toDate]);

  const update = (field) => (event) => setForm((current) => ({ ...current, [field]: event.target.value }));

  function handleSubmit(event) {
    event.preventDefault();
    if (form.fromDate && form.toDate && form.fromDate > form.toDate) {
      setError('תאריך ההתחלה חייב להיות לפני תאריך הסיום.');
      return;
    }
    setError(null);
    onSearch({
      origin: form.origin.trim(),
      destination: form.destination.trim(),
      fromDate: form.fromDate,
      toDate: form.toDate,
    });
  }

  function handleClear() {
    setForm(EMPTY);
    setError(null);
    onSearch(EMPTY);
  }

  function swap() {
    setForm((current) => ({ ...current, origin: current.destination, destination: current.origin }));
  }

  const hasFilters = Object.values(value).some(Boolean);

  return (
    <form className="filters" onSubmit={handleSubmit} role="search" aria-label="חיפוש טיסות">
      <div className="filters__route">
        <label className="field">
          <span className="field__label">
            <MapPin size={14} aria-hidden="true" /> מוצא
          </span>
          <input
            className="input"
            value={form.origin}
            onChange={update('origin')}
            placeholder="לדוגמה: Tel Aviv"
            maxLength={100}
            dir="auto"
          />
        </label>

        <button type="button" className="icon-btn filters__swap" onClick={swap} aria-label="החלפת מוצא ויעד">
          <ArrowLeftRight size={18} />
        </button>

        <label className="field">
          <span className="field__label">
            <MapPin size={14} aria-hidden="true" /> יעד
          </span>
          <input
            className="input"
            value={form.destination}
            onChange={update('destination')}
            placeholder="לדוגמה: London"
            maxLength={100}
            dir="auto"
          />
        </label>
      </div>

      <div className="filters__dates">
        <label className="field">
          <span className="field__label">
            <CalendarDays size={14} aria-hidden="true" /> מתאריך
          </span>
          <input className="input" type="date" value={form.fromDate} onChange={update('fromDate')} />
        </label>
        <label className="field">
          <span className="field__label">
            <CalendarDays size={14} aria-hidden="true" /> עד תאריך
          </span>
          <input className="input" type="date" value={form.toDate} onChange={update('toDate')} min={form.fromDate || undefined} />
        </label>
      </div>

      <div className="filters__actions">
        <button type="submit" className="btn btn--accent">
          <Search size={16} aria-hidden="true" /> חיפוש
        </button>
        {hasFilters && (
          <button type="button" className="btn btn--ghost" onClick={handleClear}>
            <X size={16} aria-hidden="true" /> ניקוי
          </button>
        )}
      </div>

      {error && (
        <p className="field__error filters__error" role="alert">
          {error}
        </p>
      )}
    </form>
  );
}
