import { json } from '@sveltejs/kit';
import { apiClient } from '$lib/services/api.server';
import logger from '$lib/logger';

export async function GET(event) {
  let apiStatus: 'up' | 'down' | 'unreachable' = 'down';

  try {
    const res = await apiClient.fetch('health', {
      method: 'GET',
      signal: AbortSignal.timeout(2000)
    }, event);
    apiStatus = res.ok ? 'up' : 'down';
  } catch (err) {
    logger.debug({ err }, 'Health check: API unreachable');
    apiStatus = 'unreachable';
  }

  const isHealthy = apiStatus === 'up';

  return json(
    {
      status: isHealthy ? 'healthy' : 'degraded',
      api_server: apiStatus,
      time: new Date().toISOString()
    },
    {
      status: isHealthy ? 200 : 503
    }
  );
}

export function HEAD() {
  return new Response(null, { status: 200 });
}
