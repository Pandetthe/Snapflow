import pino from 'pino';
import { browser, dev } from '$app/environment';
import { trace, context } from '@opentelemetry/api';

const isServer = !browser;
const otlpEnabled = isServer && !!process.env.OTEL_EXPORTER_OTLP_ENDPOINT;

const transport = isServer
  ? pino.transport({
    targets: [
      {
        target: 'pino-pretty',
        options: { colorize: true },
        level: dev ? 'debug' : 'info'
      },
      ...(otlpEnabled
        ? [
          {
            target: 'pino-opentelemetry-transport',
            options: {
              url: otlpEnabled ?
                (process.env.OTEL_EXPORTER_OTLP_ENDPOINT?.endsWith('/v1/logs')
                  ? process.env.OTEL_EXPORTER_OTLP_ENDPOINT
                  : `${process.env.OTEL_EXPORTER_OTLP_ENDPOINT}/v1/logs`)
                : undefined,
              serviceName: process.env.OTEL_SERVICE_NAME || 'web-client',
              resourceAttributes: {
                'service.name': process.env.OTEL_SERVICE_NAME || 'web-client',
                'deployment.environment': dev ? 'development' : 'production',
                'telemetry.sdk.name': 'opentelemetry',
                'telemetry.sdk.language': 'nodejs',
                'instrumentation.name': 'web-client'
              },
              messageKey: 'msg',
              loggerName: 'web-client'
            },
            level: 'info'
          }
        ]
        : [])
    ]
  })
  : undefined;

const logger = pino(
  {
    level: dev ? 'debug' : 'info',
    browser: {
      asObject: true
    },
    mixin() {
      if (browser) return {};
      const span = trace.getSpan(context.active());
      if (span) {
        const spanContext = span.spanContext();
        return {
          trace_id: spanContext.traceId,
          span_id: spanContext.spanId,
          traceId: spanContext.traceId,
          spanId: spanContext.spanId,
          trace_flags: spanContext.traceFlags
        };
      }
      return {};
    }
  },
  transport
);

export default logger;
