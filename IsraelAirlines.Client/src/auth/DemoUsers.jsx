import { FlaskConical } from 'lucide-react';

// DEVELOPMENT ONLY. These are the accounts the API seeds in Development (admin, user) and a second
// user created through the real register endpoint. Each button performs a real login against the API.
const DEMO_USERS = [
  { label: 'מנהל', role: 'Admin', email: 'admin@example.com', password: 'Admin123!' },
  { label: 'משתמש 1', role: 'User', email: 'user@example.com', password: 'User123!' },
  { label: 'משתמש 2', role: 'User', email: 'user2@example.com', password: 'User123!' },
];

export default function DemoUsers({ onPick, disabled }) {
  return (
    <section className="demo-users" aria-labelledby="demo-users-title">
      <h2 id="demo-users-title" className="demo-users__title">
        <FlaskConical size={16} aria-hidden="true" />
        משתמשי דמו · סביבת פיתוח בלבד
      </h2>
      <p className="demo-users__hint">התחברות אמיתית מול השרת עם חשבונות הפיתוח.</p>
      <div className="demo-users__list">
        {DEMO_USERS.map((demo) => (
          <button
            key={demo.email}
            type="button"
            className="demo-user"
            disabled={disabled}
            onClick={() => onPick(demo.email, demo.password)}
          >
            <span className="demo-user__label">{demo.label}</span>
            <span className="demo-user__email">{demo.email}</span>
          </button>
        ))}
      </div>
    </section>
  );
}
