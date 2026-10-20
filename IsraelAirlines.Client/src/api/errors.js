// Turns an ApiError into a Hebrew message for the user. No stack traces or server internals.

const DEFAULT_MESSAGES = {
  0: 'לא ניתן להתחבר לשרת. ודאו שה־API פועל ונסו שוב.',
  400: 'הנתונים שנשלחו אינם תקינים.',
  401: 'נדרשת התחברות כדי לבצע פעולה זו.',
  403: 'אין לך הרשאה לבצע פעולה זו.',
  404: 'הפריט המבוקש לא נמצא.',
  409: 'הפעולה מתנגשת במצב הנוכחי בשרת.',
};

export function errorMessage(error, overrides = {}) {
  const status = error?.status;

  if (status === undefined) return 'אירעה שגיאה לא צפויה.';
  if (overrides[status]) return overrides[status];
  if (DEFAULT_MESSAGES[status]) return DEFAULT_MESSAGES[status];
  if (status >= 500) return 'אירעה שגיאה בשרת. נסו שוב מאוחר יותר.';

  return 'אירעה שגיאה לא צפויה.';
}

// Flattens ASP.NET ValidationProblemDetails "errors" into a list of messages.
export function validationMessages(error) {
  if (!error?.validationErrors) return [];
  return Object.values(error.validationErrors).flat();
}
