import { Link, Navigate, useLocation } from 'react-router-dom';
import { ShieldAlert } from 'lucide-react';
import { useAuth } from './AuthContext';
import EmptyState from '../components/EmptyState';

// Hides screens the user should not see. The server enforces the same rules on every request.
export default function RequireAuth({ role, children }) {
  const { isAuthenticated, role: userRole } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (role && userRole !== role) {
    return (
      <EmptyState
        icon={ShieldAlert}
        title="אין הרשאה"
        text="המסך הזה זמין למנהלי מערכת בלבד."
        action={<Link className="btn btn--secondary" to="/flights">חזרה לטיסות</Link>}
      />
    );
  }

  return children;
}
