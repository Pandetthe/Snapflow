export const authConfig = {
	password: {
		minLength: 8,
		maxLength: 64,
		requireLowercase: true,
		requireUppercase: true,
		requireDigit: true,
		requireNonAlphanumeric: true
	},
	email: {
		maxLength: 254
	},
	userName: {
		minLength: 3,
		maxLength: 20
	}
} as const;

export type AuthConfig = typeof authConfig;
