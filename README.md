# IsraelAirlines

מערכת להזמנת מושבים בטיסות: שרת ASP.NET Core Web API (‏.NET 10) עם PostgreSQL, ולקוח React שמדגים אותו.

## תיאור המערכת

מנהלת (Admin) מגדירה מטוסים, כולל מבנה המושבים (מספר שורות ואותיות, למשל `ABC DEF`), ומתזמנת עליהם טיסות. כשנוצרת טיסה, השרת בונה לה רשומת מושב (`FlightSeat`) לכל מושב פיזי במטוס. נוסעים (User) נרשמים, מתחברים עם JWT, מחפשים טיסות (סינון, מיון ו-pagination בצד השרת), ובוחרים מושב פנוי. נוסע יכול להחליף מושב או לבטל את ההזמנה שלו, ולא של אחרים. ל-Admin יש גישה לכל ההזמנות ולכל המשתמשים.

## המשאב המוגבל והתחרות עליו

**המשאב המוגבל הוא מושב בטיסה** (`FlightSeat`). לכל מושב בטיסה מסוימת יש רק נוסע אחד. התחרות קורית כששני נוסעים בוחרים את אותו מושב באותו רגע (`POST /api/reservations`), וגם כשנוסע עובר למושב שמישהו אחר תופס באותו זמן (`PUT /api/reservations/{id}`). המשפט שמתאר את הבעיה: *"שני נוסעים לחצו 'הזמן' על מושב 12A באותו רגע, שניהם קיבלו אישור, ואסור היה ששניהם יקבלו".*

**איך המערכת מונעת את זה (optimistic concurrency):**

1. ל-`FlightSeat` יש concurrency token בשם `Version` (`Guid`). הוא מוגדר ב-Fluent API עם `IsConcurrencyToken()`, ו-`DataContext.SaveChangesAsync` מחליף אותו בכל עדכון. כך אף service לא צריך לזכור לעשות את זה.
2. `ReservationService` קורא את המושב, בודק שהוא `Available`, מסמן אותו `Reserved` ומוסיף `Reservation`. שני השינויים נשמרים ב-`SaveChanges` אחד, כלומר בטרנזקציה אחת.
3. EF מוסיף את ה-`Version` הישן לתנאי ה-UPDATE (‏`WHERE "Id" = @id AND "Version" = @oldVersion`). אם בקשה אחרת הספיקה לתפוס את המושב, אף שורה לא מתעדכנת ו-EF זורק `DbUpdateConcurrencyException`.
4. ‏`UnitOfWork` (שכבת Data) תופס את החריגה הספציפית הזו וזורק במקומה `ConcurrencyConflictException` מ-Core. כך שכבת ה-Service לא תלויה ב-EF Core. ה-Service רושם לוג ברמת **Warning**, וה-API מחזיר **409 Conflict** עם ההודעה *"This seat was just taken by another passenger. Please choose another seat."* הלקוח מציג את ההודעה ומרענן את מפת המושבים.
5. **שכבת הגנה נוספת:** אינדקס ייחודי על `(FlightId, SeatNumber)` בטבלת ההזמנות. אם שתי הזמנות מגיעות לבסיס הנתונים, השנייה נדחית וגם היא מתורגמת ל-409.

ההוכחה נמצאת ב-`IsraelAirlines.Tests/IntegrationTests/FlightSeatConcurrencyTests.cs`: שני `DbContext` נפרדים קוראים את אותו מושב ושניהם משנים אותו. הראשון שומר בהצלחה, והשני מקבל `DbUpdateConcurrencyException`. בדיקה שנייה מוודאת שה-`UnitOfWork` מתרגם את המקרה ל-`ConcurrencyConflictException`.

## ארכיטקטורה

| פרויקט | תוכן | תלוי ב- |
|---|---|---|
| `IsraelAirlines.core` | Entities, ‏DTOs, ‏Interfaces, ‏Enums, ‏Exceptions | אף פרויקט וללא חבילות חיצוניות |
| `IsraelAirlines.data` | ‏`DataContext`, ‏Repositories, ‏`UnitOfWork`, ‏Migrations | Core |
| `IsraelAirlines.service` | לוגיקה עסקית, AutoMapper Profile, ‏JWT | Core |
| `IsraelAirlines` (API) | Controllers, ‏Middleware, ‏`Program.cs` | Service ו-Core. ‏Data רק לרישום ב-DI |
| `IsraelAirlines.Tests` | xUnit ו-Moq | |
| `IsraelAirlines.Client` | לקוח React (Vite) | |

**קשרים:**
- one-to-many: מטוס מול טיסות, טיסה מול מושבים, טיסה מול הזמנות, משתמש מול הזמנות.
- many-to-many: מטוס מול `Amenity` (Wi-Fi, ארוחה וכו'), דרך טבלת `AircraftAmenities`. בשאילתות של טיסה הם נטענים עם `Include(f => f.Aircraft).ThenInclude(a => a.Amenities)`.

**סדר ה-pipeline:** טיפול בשגיאות, CorrelationId, לוג בקשה, CORS, ‏Authentication ואז Authorization. כל שגיאה (400/401/403/404/409/500) חוזרת כ-`application/problem+json` עם `correlationId`.

## הרצה מקומית

**דרישות:** ‏.NET SDK 10 ו-PostgreSQL 14 ומעלה. הדרך הקלה להרים PostgreSQL היא Docker:

```bash
docker run --name israelairlines-pg -e POSTGRES_PASSWORD=<password> -p 5432:5432 -d postgres:16
```

**סודות (פעם אחת):** לא נמצאים ב-repo. הם נשמרים ב-User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=IsraelAirlinesDb;Username=postgres;Password=<password>" --project IsraelAirlines
dotnet user-secrets set "JwtSettings:Key" "<random string of at least 32 characters>" --project IsraelAirlines
```

**הרצת השרת** (משורש ה-solution):

```bash
dotnet run --project IsraelAirlines --launch-profile http
```

בעלייה השרת מריץ את כל ה-migrations ויוצר את משתמשי הדמו. אין צורך בשום פעולה ידנית בבסיס הנתונים. Swagger זמין בכתובת `http://localhost:5154/swagger`. הלוגים נכתבים לקונסול ול-`IsraelAirlines/bin/<Configuration>/net10.0/logs/`.

**הרצת הלקוח:**

```bash
cd IsraelAirlines.Client
npm install
npm run dev
```

הלקוח עולה בכתובת `http://localhost:5173` ומדבר עם השרת בכתובת `http://localhost:5154`.

## משתמשי דמו (סביבת Development)

| Role | אימייל | סיסמה |
|---|---|---|
| Admin | `admin@example.com` | `Admin123!` |
| User | `user@example.com` | `User123!` |

הסיסמאות נטענות מ-`appsettings.Development.json` (`Seed:*`) ולא כתובות בקוד.

## בדיקות

```bash
# Unit tests (Moq): no database needed
dotnet test --filter "Category!=Integration"

# Integration tests: need a migrated PostgreSQL database
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection string>" --project IsraelAirlines.Tests
dotnet test --filter "Category=Integration"
```

## Endpoints עיקריים

| Method | נתיב | הרשאה |
|---|---|---|
| POST | `/api/auth/register`, ‏`/api/auth/login` | פתוח |
| GET | `/api/flights?origin=&destination=&fromDate=&toDate=&sortBy=&sortDescending=&page=&pageSize=` | פתוח |
| GET | `/api/flights/{id}` | פתוח |
| GET | `/api/flights/{id}/reserved-seats` | משתמש מחובר |
| POST, PUT, DELETE | `/api/flights`, ‏`/api/aircraft` | Admin |
| POST | `/api/reservations` | משתמש מחובר |
| GET | `/api/reservations/me` | משתמש מחובר |
| PUT, DELETE | `/api/reservations/{id}` | הבעלים או Admin |
| GET | `/api/reservations`, ‏`/api/users` | Admin |
| GET | `/api/users/{id}`, ‏`/api/users/{id}/reservations` | המשתמש עצמו או Admin |
