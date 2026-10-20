import { Fragment } from 'react';
import { CABIN_LABELS, POSITION_LABELS, describeSeat, layoutGroups } from '../utils/seats';

// Draws the cabin from the aircraft layout returned by the server.
// Taken seats come from GET /api/Flight/{id}/reserved-seats; nothing here is invented locally.
export default function SeatMap({ aircraft, reservedSeats, selectedSeat, conflictSeat, onSelect, disabled }) {
  const groups = layoutGroups(aircraft);
  const taken = new Set(reservedSeats);
  const rows = Array.from({ length: aircraft.rows }, (_, index) => index + 1);

  return (
    <div className="seatmap" dir="ltr">
      <div className="seatmap__nose" aria-hidden="true">
        <span>קדמת המטוס</span>
      </div>

      <div className="seatmap__header" aria-hidden="true">
        <span className="seatmap__rownum" />
        {groups.map((group, groupIndex) => (
          <Fragment key={group}>
            {groupIndex > 0 && <span className="seatmap__aisle" />}
            {[...group].map((letter) => (
              <span key={letter} className="seatmap__letter">
                {letter}
              </span>
            ))}
          </Fragment>
        ))}
      </div>

      <div className="seatmap__rows" role="group" aria-label="מפת מושבים">
        {rows.map((row) => (
          <Fragment key={row}>
            {row === aircraft.businessRows + 1 && aircraft.businessRows > 0 && (
              <div className="seatmap__divider" aria-hidden="true">
                <span>מחלקת תיירים</span>
              </div>
            )}
            {row === 1 && aircraft.businessRows > 0 && (
              <div className="seatmap__divider seatmap__divider--business" aria-hidden="true">
                <span>מחלקת עסקים</span>
              </div>
            )}
            <div className={`seatmap__row ${row <= aircraft.businessRows ? 'seatmap__row--business' : ''}`}>
              <span className="seatmap__rownum" aria-hidden="true">
                {row}
              </span>
              {groups.map((group, groupIndex) => (
                <Fragment key={group}>
                  {groupIndex > 0 && <span className="seatmap__aisle" aria-hidden="true" />}
                  {[...group].map((letter) => {
                    const seat = `${row}${letter}`;
                    const isTaken = taken.has(seat);
                    const isSelected = seat === selectedSeat;
                    const info = describeSeat(aircraft, seat);
                    const label = `מושב ${seat}, ${CABIN_LABELS[info.cabin]}, ${POSITION_LABELS[info.position]}, ${
                      isTaken ? 'תפוס' : isSelected ? 'נבחר' : 'פנוי'
                    }`;

                    return (
                      <button
                        key={seat}
                        type="button"
                        className={[
                          'seat',
                          info.cabin === 'business' && 'seat--business',
                          isTaken && 'seat--taken',
                          isSelected && 'seat--selected',
                          seat === conflictSeat && 'seat--conflict',
                        ]
                          .filter(Boolean)
                          .join(' ')}
                        disabled={disabled || isTaken}
                        aria-pressed={isSelected}
                        aria-label={label}
                        title={label}
                        onClick={() => onSelect(isSelected ? null : seat)}
                      >
                        {letter}
                      </button>
                    );
                  })}
                </Fragment>
              ))}
              <span className="seatmap__rownum" aria-hidden="true">
                {row}
              </span>
            </div>
          </Fragment>
        ))}
      </div>
    </div>
  );
}

export function SeatLegend({ hasBusiness }) {
  return (
    <ul className="seat-legend" aria-label="מקרא">
      <li>
        <span className="seat seat--legend" aria-hidden="true" /> פנוי
      </li>
      {hasBusiness && (
        <li>
          <span className="seat seat--legend seat--business" aria-hidden="true" /> עסקים
        </li>
      )}
      <li>
        <span className="seat seat--legend seat--selected" aria-hidden="true" /> הבחירה שלך
      </li>
      <li>
        <span className="seat seat--legend seat--taken" aria-hidden="true" /> תפוס
      </li>
    </ul>
  );
}
