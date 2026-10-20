import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { LogIn, Plane, UserPlus } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { register } from '../api/auth';
import { errorMessage, validationMessages } from '../api/errors';
import Alert from '../components/Alert';
import DemoUsers from '../auth/DemoUsers';
import Field from '../components/Field';

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

// Mirrors the server's DataAnnotations so obvious mistakes are caught before sending.
function validate(mode, form) {
  const errors = {};
  if (mode === 'register' && !form.fullName.trim()) errors.fullName = 'יש להזין שם מלא.';
  if (!EMAIL_PATTERN.test(form.email.trim())) errors.email = 'יש להזין כתובת אימייל תקינה.';
  if (form.password.length < 8) errors.password = 'הסיסמה חייבת להכיל לפחות 8 תווים.';
  if (mode === 'register' && form.password !== form.confirm) errors.confirm = 'הסיסמאות אינן תואמות.';
  return errors;
}

export default function LoginPage() {
  const { login, isAuthenticated, notice, clearNotice } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const redirectTo = location.state?.from?.pathname ?? '/flights';

  const [mode, setMode] = useState('login');
  const [form, setForm] = useState({ fullName: '', email: '', password: '', confirm: '' });
  const [fieldErrors, setFieldErrors] = useState({});
  const [serverError, setServerError] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  if (isAuthenticated && !submitting) return <Navigate to={redirectTo} replace />;

  const update = (field) => (event) => {
    setForm((current) => ({ ...current, [field]: event.target.value }));
    setFieldErrors((current) => ({ ...current, [field]: undefined }));
  };

  function switchMode(next) {
    setMode(next);
    setFieldErrors({});
    setServerError(null);
  }

  async function signIn(email, password) {
    await login(email, password);
    navigate(redirectTo, { replace: true });
  }

  async function handleSubmit(event) {
    event.preventDefault();
    clearNotice();
    const errors = validate(mode, form);
    setFieldErrors(errors);
    if (Object.keys(errors).length) return;

    setSubmitting(true);
    setServerError(null);
    try {
      if (mode === 'register') {
        await register(form.fullName.trim(), form.email.trim(), form.password);
      }
      await signIn(form.email.trim(), form.password);
    } catch (error) {
      setServerError(error);
      setSubmitting(false);
    }
  }

  async function handleDemo(email, password) {
    clearNotice();
    setSubmitting(true);
    setServerError(null);
    try {
      await signIn(email, password);
    } catch (error) {
      setServerError(error);
      setSubmitting(false);
    }
  }

  const overrides =
    mode === 'login'
      ? { 401: 'אימייל או סיסמה שגויים.' }
      : { 409: 'כבר קיים משתמש עם כתובת האימייל הזו.', 401: 'ההרשמה הצליחה, אך ההתחברות נכשלה.' };

  return (
    <div className="auth-page">
      <section className="auth-hero" aria-hidden="true">
        <div className="auth-hero__content">
          <span className="auth-hero__icon">
            <Plane size={28} />
          </span>
          <h2>טסים בביטחון</h2>
          <p>חיפוש טיסות, בחירת מושב והזמנה בזמן אמת מול מערכת Israel Airlines.</p>
        </div>
      </section>

      <section className="auth-card card">
        <div className="tabs" role="tablist" aria-label="התחברות או הרשמה">
          <button
            type="button"
            role="tab"
            className="tab"
            aria-selected={mode === 'login'}
            onClick={() => switchMode('login')}
          >
            התחברות
          </button>
          <button
            type="button"
            role="tab"
            className="tab"
            aria-selected={mode === 'register'}
            onClick={() => switchMode('register')}
          >
            הרשמה
          </button>
        </div>

        <h1 className="auth-card__title">{mode === 'login' ? 'ברוכים השבים' : 'יצירת חשבון'}</h1>

        {notice && <Alert variant="warning">{notice}</Alert>}
        {serverError && (
          <Alert
            variant="error"
            details={validationMessages(serverError)}
            correlationId={serverError.status >= 500 ? serverError.correlationId : null}
          >
            {errorMessage(serverError, overrides)}
          </Alert>
        )}

        <form className="form" onSubmit={handleSubmit} noValidate>
          {mode === 'register' && (
            <Field label="שם מלא" error={fieldErrors.fullName}>
              <input className="input" value={form.fullName} onChange={update('fullName')} autoComplete="name" maxLength={100} />
            </Field>
          )}
          <Field label="אימייל" error={fieldErrors.email}>
            <input
              className="input"
              type="email"
              dir="ltr"
              value={form.email}
              onChange={update('email')}
              autoComplete="email"
              maxLength={254}
            />
          </Field>
          <Field label="סיסמה" error={fieldErrors.password} hint={mode === 'register' ? 'לפחות 8 תווים' : null}>
            <input
              className="input"
              type="password"
              dir="ltr"
              value={form.password}
              onChange={update('password')}
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
            />
          </Field>
          {mode === 'register' && (
            <Field label="אימות סיסמה" error={fieldErrors.confirm}>
              <input
                className="input"
                type="password"
                dir="ltr"
                value={form.confirm}
                onChange={update('confirm')}
                autoComplete="new-password"
              />
            </Field>
          )}

          <button type="submit" className="btn btn--primary btn--block btn--lg" disabled={submitting}>
            {mode === 'login' ? <LogIn size={18} aria-hidden="true" /> : <UserPlus size={18} aria-hidden="true" />}
            {submitting ? 'שולח…' : mode === 'login' ? 'התחברות' : 'הרשמה והתחברות'}
          </button>
        </form>

        {import.meta.env.DEV && <DemoUsers onPick={handleDemo} disabled={submitting} />}
      </section>
    </div>
  );
}
