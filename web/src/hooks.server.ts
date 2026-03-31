import type { Handle, HandleServerError } from '@sveltejs/kit';
import { env } from '$env/dynamic/public';
import { UsersService } from '$lib/features/users/api/users';
import { apiClient } from '$lib/server/api.server.ts';
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
      } else if ('status' in result && result.status === 401) {
        event.locals.session = null;
        event.cookies.delete('Snapflow.Auth.Cookie', { path: '/' });
      }
    } catch (err) {
      logger.error({ err }, 'Failed to fetch user data');
    }
  }

  const response = await resolve(event);

  const apiBaseUrl = env.PUBLIC_API_BASE_URL || '';

  const csp = [
    "default-src 'self'",
    "script-src 'self' 'unsafe-inline'",
    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
    "font-src 'self' https://fonts.gstatic.com",
    `img-src 'self' data: ${apiBaseUrl}`,
    "object-src 'none'",
    "require-trusted-types-for 'script'"
  ].join('; ');

  response.headers.set('Content-Security-Policy', csp);
  response.headers.set('Strict-Transport-Security', 'max-age=31536000; includeSubDomains; preload');
  response.headers.set('Cross-Origin-Opener-Policy', 'same-origin');
  response.headers.set('X-Frame-Options', 'DENY');

  const duration = Date.now() - start;

  if (event.url.pathname.endsWith('/health') || event.url.pathname.endsWith('/alive')) {
    return response;
  }

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
      user_id: event.locals.user?.id,
      user_name: event.locals.user?.userName
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

  const errorObj =
    error instanceof Error
      ? { message: error.message, stack: error.stack, name: error.name }
      : { message: String(error) };

  logger.error(
    {
      err: errorObj,
      method: event.request.method,
      url: event.url.pathname,
      user_id: event.locals.user?.id,
      user_name: event.locals.user?.userName
    },
    'Unhandled server error'
  );

  return {
    message: 'Internal processing error',
    code: 'InternalServerError'
  };
};