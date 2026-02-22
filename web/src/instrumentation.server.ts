import { createAddHookMessageChannel } from 'import-in-the-middle';
import { register } from 'node:module';

if (!(globalThis as any).__OTEL_INITIALIZED__) {
	(globalThis as any).__OTEL_INITIALIZED__ = true;

	const { registerOptions } = createAddHookMessageChannel();
	register('import-in-the-middle/hook.mjs', import.meta.url, registerOptions);

	const connectionString = process.env.APPLICATIONINSIGHTS_CONNECTION_STRING;
	const otlpEndpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT;

if (connectionString || otlpEndpoint) {
	(async () => {
		const { diag, DiagConsoleLogger, DiagLogLevel } = await import('@opentelemetry/api');
		const { OTLPMetricExporter } = await import('@opentelemetry/exporter-metrics-otlp-proto');
		const opentelemetry = await import('@opentelemetry/sdk-node');
		const { PeriodicExportingMetricReader } = await import('@opentelemetry/sdk-metrics');
		const { OTLPTraceExporter } = await import('@opentelemetry/exporter-trace-otlp-proto');
		const { HttpInstrumentation } = await import('@opentelemetry/instrumentation-http');
		const { NetInstrumentation } = await import('@opentelemetry/instrumentation-net');
		const { DnsInstrumentation } = await import('@opentelemetry/instrumentation-dns');
		const { UndiciInstrumentation } = await import('@opentelemetry/instrumentation-undici');

		const dev = process.env.NODE_ENV !== 'production';
		diag.setLogger(new DiagConsoleLogger(), dev ? DiagLogLevel.INFO : DiagLogLevel.WARN);

		if (connectionString) {
			const { useAzureMonitor } = await import('@azure/monitor-opentelemetry');
			console.info('[OTEL] Azure Monitor Connection String found, initializing...');
			useAzureMonitor();
		} else if (otlpEndpoint) {
			console.info(`[OTEL] Initializing with OTLP/HTTP endpoint: ${otlpEndpoint}`);
			const sdk = new opentelemetry.NodeSDK({
				serviceName: process.env.OTEL_SERVICE_NAME || "web-client",
				traceExporter: new OTLPTraceExporter(),
				metricReaders: [new PeriodicExportingMetricReader({
					exportIntervalMillis: dev ? 5000 : 10000,
					exportTimeoutMillis: dev ? 5000 : 10000,
					exporter: new OTLPMetricExporter(),
				})],
				instrumentations: [
					new HttpInstrumentation({
						ignoreIncomingRequestHook: (req) => {
							return req.url?.includes('/health') || req.url?.includes('/alive') || false;
						},
					}),
					new UndiciInstrumentation(),
					new NetInstrumentation(),
					new DnsInstrumentation()
				],
			});

			sdk.start();
		}
	})().catch(err => {
		console.error('[OTEL] FAILED TO INITIALIZE', err);
	});
} else {
	console.info('[OTEL] No OTLP endpoint or Azure connection string found, skipping initialization');
}}
