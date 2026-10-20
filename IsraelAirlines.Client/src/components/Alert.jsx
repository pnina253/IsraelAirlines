import { AlertTriangle, CheckCircle2, Info, XCircle } from 'lucide-react';

const ICONS = { success: CheckCircle2, error: XCircle, warning: AlertTriangle, info: Info };

// Errors and conflicts are announced immediately (role="alert"); other messages politely.
export default function Alert({ variant = 'info', title, children, details = [], correlationId, onClose }) {
  const Icon = ICONS[variant];
  const urgent = variant === 'error' || variant === 'warning';

  return (
    <div className={`alert alert--${variant}`} role={urgent ? 'alert' : 'status'}>
      <Icon className="alert__icon" size={20} aria-hidden="true" />
      <div className="alert__body">
        {title && <p className="alert__title">{title}</p>}
        {children && <div className="alert__text">{children}</div>}
        {details.length > 0 && (
          <ul className="alert__details" dir="auto">
            {details.map((detail) => (
              <li key={detail}>{detail}</li>
            ))}
          </ul>
        )}
        {correlationId && (
          <p className="alert__meta">
            מזהה לבירור: <bdi>{correlationId}</bdi>
          </p>
        )}
      </div>
      {onClose && (
        <button type="button" className="alert__close" onClick={onClose} aria-label="סגירת ההודעה">
          ×
        </button>
      )}
    </div>
  );
}
