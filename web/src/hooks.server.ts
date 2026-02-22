import type { Handle, HandleServerError } from '@sveltejs/kit';
import { UsersService } from '$lib/services/users';
import { apiClient } from '$lib/services/api.server.ts';
import logger from '$lib/logger';
import { requestCounter, requestDuration, errorCounter } from '$lib/metrics';

export const handle: Handle = async ({ event, resolve }) => {
  const start = Date.now();
  const session = event.cookies.get('Snapflow.Auth.Cookie');
  event.locals.session = session ?? null;
  event.locals.user = null;

  if (session) {
    try {
      const result = await new UsersService(apiClient).getMe(event);
      if (result.ok) {
        event.locals.user = result.user;
      }
    } catch (err) {
      logger.error({ err }, 'Failed to fetch user data');
    }
  }

  const response = await resolve(event);
  const duration = Date.now() - start;

  requestCounter.add(1, {
    method: event.request.method,
    url: event.url.pathname,
    status: response.status
  });

  requestDuration.record(duration, {
    method: event.request.method,
    url: event.url.pathname,
    status: response.status
  });

  logger.info(
    {
      method: event.request.method,
      url: event.url.pathname,
      status: response.status,
      duration_ms: duration,
    },
    'Request processed'
  );

  return response;
};

export const handleError: HandleServerError = ({ error, event }) => {
  errorCounter.add(1, {
    method: event.request.method,
    url: event.url.pathname
  });

  logger.error(
    {
      error,
      method: event.request.method,
      url: event.url.pathname,
    },
    'Unhandled server error'
  );

  return {
    message: 'Internal processing error',
    code: 'InternalServerError'
  };
};
