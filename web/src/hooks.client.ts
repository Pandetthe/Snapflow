import type { HandleClientError } from '@sveltejs/kit';
import logger from '$lib/logger';

export const handleError: HandleClientError = ({ error, event }) => {
  logger.error(
    {
      error: error instanceof Error ? error : String(error),
      url: event.url.pathname,
    },
    'Unhandled client side error'
  );

  return {
    message: 'Something went wrong',
  };
};
