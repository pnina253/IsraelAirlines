import { Plane } from 'lucide-react';

export function seatCount(aircraft) {
  return aircraft.rows * aircraft.seatLayout.replace(/ /g, '').length;
}

// Aircraft data exactly as GET /api/Aircraft returns it, plus the seat count derived from the layout.
export default function AircraftDetails({ aircraft, compact = false }) {
  const seatsPerRow = aircraft.seatLayout.replace(/ /g, '').length;
  const businessSeats = aircraft.businessRows * seatsPerRow;

  return (
    <div className={`aircraft ${compact ? 'aircraft--compact' : ''}`}>
      <div className="aircraft__head">
        <span className="aircraft__icon" aria-hidden="true">
          <Plane size={20} />
        </span>
        <div>
          <p className="aircraft__model">
            <bdi>{aircraft.model}</bdi>
          </p>
          <p className="aircraft__reg">
            <bdi>{aircraft.registrationNumber}</bdi>
          </p>
        </div>
        <span className={`status-badge ${aircraft.isActive ? 'status-badge--on' : 'status-badge--off'}`}>
          {aircraft.isActive ? 'פעיל' : 'לא פעיל'}
        </span>
      </div>
      <dl className="aircraft__facts">
        <div>
          <dt>מושבים</dt>
          <dd>{seatCount(aircraft)}</dd>
        </div>
        <div>
          <dt>שורות</dt>
          <dd>{aircraft.rows}</dd>
        </div>
        <div>
          <dt>תצורה</dt>
          <dd>
            <bdi>{aircraft.seatLayout}</bdi>
          </dd>
        </div>
        <div>
          <dt>עסקים</dt>
          <dd>
            {aircraft.businessRows} שורות · {businessSeats} מושבים
          </dd>
        </div>
      </dl>
    </div>
  );
}
