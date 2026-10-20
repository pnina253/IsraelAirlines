import { cloneElement, useId } from 'react';

// The label names the control; hint and error are linked with aria-describedby so they are not part of the name.
export default function Field({ label, error, hint, children }) {
  const id = useId();
  const messageId = `${id}-message`;
  const message = error || hint;

  return (
    <div className="field">
      <label className="field__label" htmlFor={id}>
        {label}
      </label>
      {cloneElement(children, {
        id,
        'aria-invalid': error ? true : undefined,
        'aria-describedby': message ? messageId : undefined,
      })}
      {message && (
        <span id={messageId} className={error ? 'field__error' : 'field__hint'}>
          {message}
        </span>
      )}
    </div>
  );
}
