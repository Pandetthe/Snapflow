import { metrics } from '@opentelemetry/api';

const meter = metrics.getMeter('snapflow-web');

export const requestCounter = meter.createCounter('http.requests.total', {
  description: 'Total count of incoming HTTP requests'
});

export const apiRequestCounter = meter.createCounter('api.client.requests.total', {
  description: 'Total count of outgoing API client requests'
});

export const errorCounter = meter.createCounter('app.errors.total', {
  description: 'Total count of unhandled application errors'
});

export const requestDuration = meter.createHistogram('http.requests.duration', {
  description: 'Duration of incoming HTTP requests',
  unit: 'ms'
});

export const apiRequestDuration = meter.createHistogram('api.client.duration', {
  description: 'Duration of outgoing API client requests',
  unit: 'ms'
});
