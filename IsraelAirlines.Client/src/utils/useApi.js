import { useCallback, useEffect, useRef, useState } from 'react';

// Runs an API call when its dependencies change, cancels stale requests,
// and keeps the previous data visible while a new request is loading.
export default function useApi(fetcher, deps) {
  const [state, setState] = useState({ status: 'loading', data: null, error: null });
  const [reloadKey, setReloadKey] = useState(0);
  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  useEffect(() => {
    const controller = new AbortController();
    setState((previous) => ({ ...previous, status: 'loading', error: null }));

    fetcherRef
      .current(controller.signal)
      .then((data) => setState({ status: 'success', data, error: null }))
      .catch((error) => {
        if (error.name === 'AbortError') return;
        setState({ status: 'error', data: null, error });
      });

    return () => controller.abort();
  }, [...deps, reloadKey]);

  const reload = useCallback(() => setReloadKey((key) => key + 1), []);

  return { ...state, reload };
}
