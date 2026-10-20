import { useEffect, useState } from 'react';
import { Search, X } from 'lucide-react';

// Origin/destination filter for the calendar, timeline and map. Values go to the server as query parameters.
export default function RouteFilter({ origin, destination, onChange, children }) {
  const [form, setForm] = useState({ origin, destination });

  useEffect(() => setForm({ origin, destination }), [origin, destination]);

  function handleSubmit(event) {
    event.preventDefault();
    onChange({ origin: form.origin.trim(), destination: form.destination.trim() });
  }

  return (
    <form className="route-filter" onSubmit={handleSubmit} role="search" aria-label="סינון לפי מסלול">
      <label className="field">
        <span className="field__label">מוצא</span>
        <input
          className="input"
          dir="auto"
          value={form.origin}
          maxLength={100}
          placeholder="כל המוצאים"
          onChange={(e) => setForm((f) => ({ ...f, origin: e.target.value }))}
        />
      </label>
      <label className="field">
        <span className="field__label">יעד</span>
        <input
          className="input"
          dir="auto"
          value={form.destination}
          maxLength={100}
          placeholder="כל היעדים"
          onChange={(e) => setForm((f) => ({ ...f, destination: e.target.value }))}
        />
      </label>
      {children}
      <div className="route-filter__actions">
        <button type="submit" className="btn btn--primary">
          <Search size={16} aria-hidden="true" /> סינון
        </button>
        {(origin || destination) && (
          <button type="button" className="btn btn--ghost" onClick={() => onChange({ origin: '', destination: '' })}>
            <X size={16} aria-hidden="true" /> ניקוי
          </button>
        )}
      </div>
    </form>
  );
}
