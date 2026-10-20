// Seat types are derived from the aircraft layout the server returns, e.g. rows=30, seatLayout="ABC DEF", businessRows=3.
// A space in the layout is an aisle: first/last letters are windows, letters next to an aisle are aisle seats.

export function layoutGroups(aircraft) {
  return aircraft.seatLayout.split(' ').filter(Boolean);
}

export function describeSeat(aircraft, seatNumber) {
  const match = /^(\d+)([A-Z])$/.exec(seatNumber);
  if (!match) return null;

  const row = Number(match[1]);
  const letter = match[2];
  const groups = layoutGroups(aircraft);

  const cabin = row <= aircraft.businessRows ? 'business' : 'economy';

  let position = 'middle';
  groups.forEach((group, groupIndex) => {
    const index = group.indexOf(letter);
    if (index === -1) return;

    const isFirstGroup = groupIndex === 0;
    const isLastGroup = groupIndex === groups.length - 1;

    if ((isFirstGroup && index === 0) || (isLastGroup && index === group.length - 1)) {
      position = 'window';
    } else if ((!isFirstGroup && index === 0) || (!isLastGroup && index === group.length - 1)) {
      position = 'aisle';
    }
  });

  return { row, letter, cabin, position };
}

export const CABIN_LABELS = { business: 'מחלקת עסקים', economy: 'מחלקת תיירים' };
export const POSITION_LABELS = { window: 'חלון', aisle: 'מעבר', middle: 'אמצע' };
