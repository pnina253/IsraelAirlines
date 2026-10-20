# IsraelAirlines.Client

React client for the IsraelAirlines Web API. Every screen talks to the real API; nothing is mocked.

## Run

1. Start the API (`IsraelAirlines` project, `http` profile, listens on `http://localhost:5154`).
2. In this folder:

```powershell
npm install
npm run dev
```

3. Open `http://localhost:5173`. The API's CORS policy allows this origin.

To use a different API address, copy `.env.example` to `.env.local` and change `VITE_API_URL`.

## Demo users (Development only)

The login page shows demo buttons only in `npm run dev`; they are removed from production builds.
Each button performs a real login.

| Role  | Email               | Created by                    |
|-------|---------------------|-------------------------------|
| Admin | admin@example.com   | API seeder (Development)      |
| User  | user@example.com    | API seeder (Development)      |
| User  | user2@example.com   | `POST /api/User/register`     |

## Seeing HTTP 409

Open the same flight in a normal window and in a private window, log in as two different users,
pick the same seat in both, and book. The second booking gets a real `409 Conflict` from the server:
the client shows "the seat is already taken" and reloads the seat map.

## Calendar, timeline and map

These views need a whole date range, so they walk the server's pages (`pageSize=100`) instead of
loading everything. The map places cities using the table in `src/utils/cities.js`; a city that is
not in the table is listed as "no location" rather than guessed. Map tiles come from OpenStreetMap.

## Structure

```
src/
  api/         http.js (fetch, Bearer token, ProblemDetails -> ApiError), one module per resource
  auth/        AuthContext (session in memory + sessionStorage), RequireAuth, DemoUsers
  components/  Navbar, FlightCard, FlightFilters, Pagination, SeatMap, Alert, Spinner, EmptyState
  pages/       Login, Flights (server paging), FlightDetails (seat map, booking, 409), MyReservations,
               Calendar, Timeline, Map, admin: CreateFlight, Aircraft
  utils/       formatting, dates, seat types, city coordinates, useApi / useQueryState hooks
  styles/      tokens.css (palette), global.css
```
