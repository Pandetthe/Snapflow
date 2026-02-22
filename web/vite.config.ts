import devtoolsJson from 'vite-plugin-devtools-json';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vitest/config';
import { playwright } from '@vitest/browser-playwright';
import { sveltekit } from '@sveltejs/kit/vite';
import pino from 'pino';

const otlpEndpoint = process.env.OTEL_EXPORTER_OTLP_ENDPOINT;

const logger = pino({
	transport: {
		targets: [
			{
				target: 'pino-pretty',
				options: { colorize: true }
			},
			...(otlpEndpoint
				? [
						{
							target: 'pino-opentelemetry-transport',
							options: {
								url: otlpEndpoint.endsWith('/v1/logs') ? otlpEndpoint : `${otlpEndpoint}/v1/logs`,
								serviceName: process.env.OTEL_SERVICE_NAME || 'web-client-vite',
								resourceAttributes: {
									'service.name': process.env.OTEL_SERVICE_NAME || 'web-client-vite',
									'telemetry.sdk.language': 'nodejs'
								},
								messageKey: 'msg',
								loggerName: 'vite'
							},
							level: 'info'
						}
					]
				: [])
		]
	}
});

const stripAnsi = (str: string) => str.replace(/[\u001b\u009b][[()#;?]*(?:[0-9]{1,4}(?:;[0-9]{0,4})*)?[0-m]/g, '');

export default defineConfig({
	customLogger: {
		info: (msg) => logger.info(stripAnsi(msg)),
		warn: (msg) => logger.warn(stripAnsi(msg)),
		error: (msg) => logger.error(stripAnsi(msg)),
		warnOnce: (msg) => logger.warn(stripAnsi(msg)),
		clearScreen: () => {},
		hasErrorLogged: () => false,
		hasWarned: false
	},
	plugins: [tailwindcss(), sveltekit(), devtoolsJson()],
  server: {
    allowedHosts: [
			'host.docker.internal',
			'localhost',
			'.localhost'
		],
  },
	test: {
		expect: { requireAssertions: true },
		projects: [
			{
				extends: './vite.config.ts',
				test: {
					name: 'client',
					browser: {
						enabled: true,
						provider: playwright(),
						instances: [{ browser: 'chromium', headless: true }]
					},
					include: ['src/**/*.svelte.{test,spec}.{js,ts}'],
					exclude: ['src/lib/server/**']
				}
			},
			{
				extends: './vite.config.ts',
				test: {
					name: 'server',
					environment: 'node',
					include: ['src/**/*.{test,spec}.{js,ts}'],
					exclude: ['src/**/*.svelte.{test,spec}.{js,ts}']
				}
			}
		]
	}
});
