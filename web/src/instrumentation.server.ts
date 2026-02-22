import { useAzureMonitor } from "@azure/monitor-opentelemetry";
import * as opentelemetry from "@opentelemetry/sdk-node";
import { diag, DiagConsoleLogger, DiagLogLevel } from "@opentelemetry/api";
import { OTLPMetricExporter } from "@opentelemetry/exporter-metrics-otlp-proto";
import { OTLPLogExporter } from "@opentelemetry/exporter-logs-otlp-proto";
import { PeriodicExportingMetricReader } from "@opentelemetry/sdk-metrics";
import { SimpleLogRecordProcessor } from "@opentelemetry/sdk-logs";
import { createAddHookMessageChannel } from 'import-in-the-middle';
import { register } from 'node:module';
import { dev } from "$app/environment";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-proto";
import { getNodeAutoInstrumentations } from "@opentelemetry/auto-instrumentations-node";
import { PinoInstrumentation } from "@opentelemetry/instrumentation-pino";

// Set up diagnostic logging for OpenTelemetry Troubleshooting
diag.setLogger(new DiagConsoleLogger(), dev ? DiagLogLevel.INFO : DiagLogLevel.WARN);

const { registerOptions } = createAddHookMessageChannel();
register('import-in-the-middle/hook.mjs', import.meta.url, registerOptions);

console.log(`[OTEL] Initializing with endpoint: ${process.env.OTEL_EXPORTER_OTLP_ENDPOINT || 'none'}`);
const connectionString = process.env.APPLICATIONINSIGHTS_CONNECTION_STRING;
const otlpEndpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT;

if (connectionString) {
  console.log('[OTEL] Azure Monitor Connection String found');
  useAzureMonitor();
}

if (otlpEndpoint) {
  const sdk = new opentelemetry.NodeSDK({
    serviceName: process.env.OTEL_SERVICE_NAME || "web-client",
    traceExporter: new OTLPTraceExporter(),
    metricReaders: [new PeriodicExportingMetricReader({
      exportIntervalMillis: dev ? 5000 : 10000,
      exporter: new OTLPMetricExporter(),
    })],
    logRecordProcessors: [
      new SimpleLogRecordProcessor(new OTLPLogExporter())
    ],
    instrumentations: [
      getNodeAutoInstrumentations({
        '@opentelemetry/instrumentation-http': {
          ignoreIncomingRequestHook: (req) => {
            return req.url?.includes('/health') || req.url?.includes('/alive') || false;
          },
        },
      }),
      new PinoInstrumentation()
    ],
  });

  sdk.start();
}
