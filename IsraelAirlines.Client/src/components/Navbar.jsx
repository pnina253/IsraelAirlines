import { Link, NavLink, useNavigate } from 'react-router-dom';
import { LogIn, LogOut, Map, Plane, PlusCircle, Ticket, UserRound, Warehouse } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';

const ROLE_LABELS = { Admin: 'מנהל', User: 'משתמש' };

export default function Navbar() {
  const { user, role, isAuthenticated, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <header className="navbar">
      <div className="navbar__inner container">
        <Link to="/flights" className="brand" aria-label="Israel Airlines, דף הטיסות">
          <span className="brand__mark" aria-hidden="true">
            <Plane size={20} />
          </span>
          <span className="brand__name">
            Israel <strong>Airlines</strong>
          </span>
        </Link>

        <nav className="navbar__links" aria-label="ניווט ראשי">
          <NavLink to="/flights" className="navlink">
            <Plane size={16} aria-hidden="true" /> טיסות
          </NavLink>
          <NavLink to="/map" className="navlink">
            <Map size={16} aria-hidden="true" /> מפת יעדים
          </NavLink>
          {isAuthenticated && (
            <NavLink to="/my-reservations" className="navlink">
              <Ticket size={16} aria-hidden="true" /> ההזמנות שלי
            </NavLink>
          )}
          {isAdmin && (
            <NavLink to="/admin/flights/new" className="navlink">
              <PlusCircle size={16} aria-hidden="true" /> יצירת טיסה
            </NavLink>
          )}
          {isAdmin && (
            <NavLink to="/admin/aircraft" className="navlink">
              <Warehouse size={16} aria-hidden="true" /> צי המטוסים
            </NavLink>
          )}
        </nav>

        <div className="navbar__user">
          {isAuthenticated ? (
            <>
              <span className="user-chip">
                <UserRound size={16} aria-hidden="true" />
                <span className="user-chip__name">{user.fullName}</span>
                <span className={`role-badge role-badge--${role?.toLowerCase()}`}>
                  {ROLE_LABELS[role] ?? role}
                </span>
              </span>
              <button type="button" className="btn btn--ghost-light btn--sm" onClick={handleLogout}>
                <LogOut size={16} aria-hidden="true" /> התנתקות
              </button>
            </>
          ) : (
            <Link to="/login" className="btn btn--accent btn--sm">
              <LogIn size={16} aria-hidden="true" /> התחברות
            </Link>
          )}
        </div>
      </div>
    </header>
  );
}
