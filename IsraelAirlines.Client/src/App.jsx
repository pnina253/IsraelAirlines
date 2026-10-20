import { Link, Navigate, Route, Routes } from 'react-router-dom';
import { Compass } from 'lucide-react';
import Navbar from './components/Navbar';
import EmptyState from './components/EmptyState';
import RequireAuth from './auth/RequireAuth';
import LoginPage from './pages/LoginPage';
import FlightsPage from './pages/FlightsPage';
import FlightDetailsPage from './pages/FlightDetailsPage';
import MyReservationsPage from './pages/MyReservationsPage';
import CalendarPage from './pages/CalendarPage';
import TimelinePage from './pages/TimelinePage';
import MapPage from './pages/MapPage';
import CreateFlightPage from './pages/CreateFlightPage';
import AircraftPage from './pages/AircraftPage';

export default function App() {
  return (
    <div className="app">
      <a href="#main" className="skip-link">
        דילוג לתוכן הראשי
      </a>
      <Navbar />
      <main id="main" className="main">
        <Routes>
          <Route path="/" element={<Navigate to="/flights" replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/flights" element={<FlightsPage />} />
          <Route path="/flights/:id" element={<FlightDetailsPage />} />
          <Route path="/calendar" element={<CalendarPage />} />
          <Route path="/timeline" element={<TimelinePage />} />
          <Route path="/map" element={<MapPage />} />
          <Route
            path="/admin/flights/new"
            element={
              <RequireAuth role="Admin">
                <CreateFlightPage />
              </RequireAuth>
            }
          />
          <Route
            path="/admin/aircraft"
            element={
              <RequireAuth role="Admin">
                <AircraftPage />
              </RequireAuth>
            }
          />
          <Route
            path="/my-reservations"
            element={
              <RequireAuth>
                <MyReservationsPage />
              </RequireAuth>
            }
          />
          <Route
            path="*"
            element={
              <EmptyState
                icon={Compass}
                title="העמוד לא נמצא"
                action={
                  <Link to="/flights" className="btn btn--secondary">
                    לרשימת הטיסות
                  </Link>
                }
              />
            }
          />
        </Routes>
      </main>
      <footer className="footer">
        <div className="container">
          <p>Israel Airlines · פרויקט גמר</p>
          <p className="footer__copyright">© 2026 כל הזכויות שמורות לפניני רוזנטל</p>
        </div>
      </footer>
    </div>
  );
}
