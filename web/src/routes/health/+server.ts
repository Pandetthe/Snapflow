import { json } from '@sveltejs/kit';
import { apiClient } from '$lib/server/api.server';
import logger from '$lib/logger';

export async function GET(event) {
  const start = performance.now();
  let apiStatus: 'Healthy' | 'Unhealthy' = 'Unhealthy';
  let apiDuration = '00:00:00.000';

  try {
    const apiCallStart = performance.now();
    const res = await apiClient.fetch(
      'health',
      {
        method: 'GET',
        signal: AbortSignal.timeout(2000)
      },
      event
    );
    const duration = performance.now() - apiCallStart;
    const durationString = new Date(duration).toISOString().slice(11, -1);

    apiStatus = res.ok ? 'Healthy' : 'Unhealthy';
    apiDuration = durationString;
  } catch (err) {
    logger.debug({ err }, 'Health check: API unreachable');
    apiStatus = 'Unhealthy';
    const duration = performance.now() - start;
    apiDuration = new Date(duration).toISOString().slice(11, -1);
  }

  const isHealthy = apiStatus === 'Healthy';
  const totalDuration = performance.now() - start;
  const totalDurationString = new Date(totalDuration).toISOString().slice(11, -1);

  return json(
    {
      status: isHealthy ? 'Healthy' : 'Unhealthy',
      totalDuration: totalDurationString,
      entries: {
        api: {
          data: {},
          duration: apiDuration,
          status: apiStatus,
          tags: ['api', 'backend']
        }
      }
    },
    {
      status: isHealthy ? 200 : 503
    }
  );
}

export function HEAD() {
  return new Response(null, { status: 200 });
}
