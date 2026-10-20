import { ChevronLeft, ChevronRight } from 'lucide-react';

// Page numbers to show: first, last, and a window around the current page.
function pageWindow(page, totalPages) {
  const pages = new Set([1, totalPages, page - 1, page, page + 1]);
  const sorted = [...pages].filter((p) => p >= 1 && p <= totalPages).sort((a, b) => a - b);

  const result = [];
  sorted.forEach((p, index) => {
    if (index > 0 && p - sorted[index - 1] > 1) result.push(`gap-${p}`);
    result.push(p);
  });
  return result;
}

export default function Pagination({ page, totalPages, onChange, disabled }) {
  if (totalPages <= 1) return null;

  return (
    <nav className="pagination" aria-label="מעבר בין עמודים">
      <button
        type="button"
        className="pagination__btn"
        onClick={() => onChange(page - 1)}
        disabled={disabled || page <= 1}
      >
        <ChevronRight size={16} aria-hidden="true" /> הקודם
      </button>

      <ol className="pagination__pages">
        {pageWindow(page, totalPages).map((item) =>
          typeof item === 'string' ? (
            <li key={item} className="pagination__gap" aria-hidden="true">
              …
            </li>
          ) : (
            <li key={item}>
              <button
                type="button"
                className="pagination__page"
                onClick={() => onChange(item)}
                disabled={disabled}
                aria-current={item === page ? 'page' : undefined}
                aria-label={`עמוד ${item}`}
              >
                {item}
              </button>
            </li>
          ),
        )}
      </ol>

      <button
        type="button"
        className="pagination__btn"
        onClick={() => onChange(page + 1)}
        disabled={disabled || page >= totalPages}
      >
        הבא <ChevronLeft size={16} aria-hidden="true" />
      </button>
    </nav>
  );
}
