export default function Spinner({ label = 'טוען…', inline = false }) {
  return (
    <div className={inline ? 'spinner spinner--inline' : 'spinner'} role="status" aria-live="polite">
      <span className="spinner__circle" aria-hidden="true" />
      <span className="spinner__label">{label}</span>
    </div>
  );
}
