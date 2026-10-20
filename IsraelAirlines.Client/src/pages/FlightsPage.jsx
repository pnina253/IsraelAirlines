import { useSearchParams } from 'react-router-dom';
import { PlaneTakeoff, SearchX } from 'lucide-react';
import { searchFlights } from '../api/flights';
import { errorMessage, validationMessages } from '../api/errors';
import useApi from '../utils/useApi';
import FlightFilters from '../components/FlightFilters';
import FlightCard from '../components/FlightCard';
import Pagination from '../components/Pagination';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import EmptyState from '../components/EmptyState';
import ViewSwitcher from '../components/ViewSwitcher';

const PAGE_SIZES = [5, 10, 20];

function toPositiveInt(value, fallback) {
  const number = Number.parseInt(value, 10);
  return Number.isInteger(number) && number > 0 ? number : fallback;
}

// The URL holds the query, so the back button and shared links keep the same page and filters.
// Every change sends a new request; the server does the paging and filtering.
export default function FlightsPage() {
  const [params, setParams] = useSearchParams();

  const query = {
    page: toPositiveInt(params.get('page'), 1),
    pageSize: PAGE_SIZES.includes(Number(params.get('pageSize'))) ? Number(params.get('pageSize')) : 10,
    origin: params.get('origin') ?? '',
    destination: params.get('destination') ?? '',
    fromDate: params.get('fromDate') ?? '',
    toDate: params.get('toDate') ?? '',
  };

  const { status, data, error, reload } = useApi((signal) => searchFlights(query, { signal }), [params.toString()]);

  function updateQuery(changes) {
    const next = { ...query, ...changes };
    const cleaned = Object.fromEntries(
      Object.entries(next).filter(([key, value]) => value !== '' && !(key === 'page' && value === 1) && !(key === 'pageSize' && value === 10)),
    );
    setParams(cleaned);
  }

  const filters = { origin: query.origin, destination: query.destination, fromDate: query.fromDate, toDate: query.toDate };
  const loading = status === 'loading';
  const firstItem = data ? (data.page - 1) * data.pageSize + 1 : 0;
  const lastItem = data ? firstItem + data.items.length - 1 : 0;

  return (
    <div className="page page--flush">
      <section className="hero">
        <div className="container">
          <h1 className="hero__title">לאן טסים היום?</h1>
          <p className="hero__subtitle">חפשו טיסה לפי מוצא, יעד ותאריכים, ובחרו את המושב שלכם.</p>
          <FlightFilters value={filters} onSearch={(values) => updateQuery({ ...values, page: 1 })} />
        </div>
      </section>

      <section className="container results" aria-labelledby="results-title" aria-busy={loading}>
        <div className="results__header">
          <h2 id="results-title" className="results__title">
            <PlaneTakeoff size={20} aria-hidden="true" /> טיסות
          </h2>

          {data && data.totalCount > 0 && (
            <p className="results__summary" aria-live="polite">
              מציג {firstItem}–{lastItem} מתוך {data.totalCount} טיסות · עמוד {data.page} מתוך {data.totalPages}
            </p>
          )}

          <ViewSwitcher />

          <label className="results__pagesize">
            <span>בכל עמוד</span>
            <select
              className="input input--sm"
              value={query.pageSize}
              onChange={(event) => updateQuery({ pageSize: Number(event.target.value), page: 1 })}
            >
              {PAGE_SIZES.map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </label>
        </div>

        {status === 'error' && (
          <Alert variant="error" details={validationMessages(error)} correlationId={error.correlationId}>
            {errorMessage(error)}{' '}
            <button type="button" className="link-btn" onClick={reload}>
              נסו שוב
            </button>
          </Alert>
        )}

        {loading && !data && <Spinner label="טוען טיסות…" />}

        {data && data.items.length === 0 && (
          <EmptyState
            icon={SearchX}
            title={data.totalCount > 0 ? 'אין תוצאות בעמוד הזה' : 'לא נמצאו טיסות'}
            text={data.totalCount > 0 ? 'העמוד המבוקש גדול ממספר העמודים הקיים.' : 'נסו לשנות את המוצא, היעד או טווח התאריכים.'}
            action={
              data.totalCount > 0 ? (
                <button type="button" className="btn btn--secondary" onClick={() => updateQuery({ page: 1 })}>
                  חזרה לעמוד הראשון
                </button>
              ) : null
            }
          />
        )}

        {data && data.items.length > 0 && (
          <div className={`flight-list ${loading ? 'is-loading' : ''}`}>
            {loading && <Spinner inline label="מעדכן…" />}
            {data.items.map((flight) => (
              <FlightCard key={flight.id} flight={flight} />
            ))}
          </div>
        )}

        {data && (
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            disabled={loading}
            onChange={(page) => {
              updateQuery({ page });
              window.scrollTo({ top: 0, behavior: 'smooth' });
            }}
          />
        )}
      </section>
    </div>
  );
}
