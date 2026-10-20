import { NavLink } from 'react-router-dom';
import { CalendarDays, GanttChart, List, Map } from 'lucide-react';

const VIEWS = [
  { to: '/flights', label: 'רשימה', icon: List },
  { to: '/calendar', label: 'לוח שנה', icon: CalendarDays },
  { to: '/timeline', label: 'ציר זמן', icon: GanttChart },
  { to: '/map', label: 'מפה', icon: Map },
];

export default function ViewSwitcher() {
  return (
    <nav className="view-switcher" aria-label="תצוגת הטיסות">
      {VIEWS.map(({ to, label, icon: Icon }) => (
        <NavLink key={to} to={to} end className="view-switcher__item">
          <Icon size={16} aria-hidden="true" /> {label}
        </NavLink>
      ))}
    </nav>
  );
}
