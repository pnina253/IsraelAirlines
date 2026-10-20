// Single place for talking to the IsraelAirlines API: base URL, JSON, Bearer token and error parsing.

const BASE_URL = (import.meta.env.VITE_API_URL ?? 'http://localhost:5154').replace(/\/$/, '');

let authToken = null;
let unauthorizedHandler = null;

export function setAuthToken(token) {
  authToken = token;
}

// Called when a request that carried a token comes back 401 (expired or invalid session).
export function setUnauthorizedHandler(handler) {
  unauthorizedHandler = handler;
}

// Carries the real HTTP status and the server's ProblemDetails body.
export class ApiError extends Error {
  constructor(status, problem) {
    super(problem?.detail || problem?.title || `HTTP ${status}`);
    this.name = 'ApiError';
    this.status = status; // 0 means the server could not be reached
    this.problem = problem ?? null;
    this.validationErrors = problem?.errors ?? null;
    this.correlationId = problem?.correlationId ?? null;
  }
}

function parseBody(text) {
  if (!text) return null;
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

export async function request(path, { method = 'GET', body, query, signal } = {}) {
  const url = new URL(BASE_URL + path);

  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') {
        url.searchParams.set(key, value);
      }
    }
  }

  const headers = { Accept: 'application/json' };
  if (body !== undefined) headers['Content-Type'] = 'application/json';

  const sentToken = authToken;
  if (sentToken) headers.Authorization = `Bearer ${sentToken}`;

  let response;
  try {
    response = await fetch(url, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      signal,
    });
  } catch (error) {
    if (error.name === 'AbortError') throw error;
    throw new ApiError(0, null);
  }

  const data = parseBody(await response.text());

  if (!response.ok) {
    if (response.status === 401 && sentToken && unauthorizedHandler) {
      unauthorizedHandler();
    }
    throw new ApiError(response.status, data);
  }

  return data;
}
