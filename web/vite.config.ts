import devtoolsJson from 'vite-plugin-devtools-json';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vitest/config';
import { playwright } from '@vitest/browser-playwright';
import { sveltekit } from '@sveltejs/kit/vite';
import pino from 'pino';

const stripAnsi = (str: string) =>
  str.replace(/[\u001b\u009b][[()#;?]*(?:[0-9]{1,4}(?:;[0-9]{0,4})*)?[0-?]*[ -/]*[@-~]/g, '');

const createCustomLogger = () => {
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

  return {
    info: (msg: string) => logger.info(stripAnsi(msg)),
    warn: (msg: string) => logger.warn(stripAnsi(msg)),
    error: (msg: string) => logger.error(stripAnsi(msg)),
    warnOnce: (msg: string) => logger.warn(stripAnsi(msg)),
    clearScreen: () => {},
    hasErrorLogged: () => false,
    hasWarned: false
  };
};

export default defineConfig(({ command }) => {
  const isServe = command === 'serve';

  return {
    customLogger: createCustomLogger(),
    build: {
      target: 'esnext',
      sourcemap: false,
      minify: 'esbuild',
    },
    plugins: [tailwindcss(), sveltekit(), ...(isServe ? [devtoolsJson()] : [])],
    server: {
      allowedHosts: ['host.docker.internal', 'localhost', '.localhost']
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
  };
});
