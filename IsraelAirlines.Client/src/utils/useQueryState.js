import { useSearchParams } from 'react-router-dom';

// Keeps view state (dates, filters, mode) in the URL so links and the back button work.
export default function useQueryState(defaults) {
  const [params, setParams] = useSearchParams();

  const state = Object.fromEntries(Object.entries(defaults).map(([key, value]) => [key, params.get(key) ?? value]));

  function update(changes) {
    const next = { ...state, ...changes };
    setParams(Object.fromEntries(Object.entries(next).filter(([key, value]) => value !== '' && value !== defaults[key])));
  }

  return [state, update, params.toString()];
}
