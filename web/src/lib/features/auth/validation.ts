import { authConfig } from '$lib/config/auth';

export function validateEmail(value: string): string | undefined {
  const trimmed = value.trim();
  if (!trimmed) return 'Email is required.';
  if (trimmed.length > authConfig.email.maxLength)
    return `Email must be less than ${authConfig.email.maxLength} characters.`;
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmed)) return 'Please enter a valid email address.';
}

export function validateUsername(value: string): string | undefined {
  const trimmed = value.trim();
  if (!trimmed) return 'Username is required.';
  if (trimmed.length < authConfig.userName.minLength)
    return `Username must be at least ${authConfig.userName.minLength} characters.`;
  if (trimmed.length > authConfig.userName.maxLength)
    return `Username must be less than ${authConfig.userName.maxLength} characters.`;
  if (!/^[a-zA-Z0-9_]+$/.test(trimmed))
    return 'Username can only contain letters, numbers, and underscores.';
}

export function validatePassword(value: string): string | undefined {
  if (!value) return 'Password is required.';
  if (value.length < authConfig.password.minLength)
    return `Password must be at least ${authConfig.password.minLength} characters.`;
  if (value.length > authConfig.password.maxLength)
    return `Password must be less than ${authConfig.password.maxLength} characters.`;
  if (authConfig.password.requireLowercase && !/[a-z]/.test(value))
    return 'Password must contain at least one lowercase letter.';
  if (authConfig.password.requireUppercase && !/[A-Z]/.test(value))
    return 'Password must contain at least one uppercase letter.';
  if (authConfig.password.requireDigit && !/\d/.test(value))
    return 'Password must contain at least one digit.';
  if (authConfig.password.requireNonAlphanumeric && !/[^a-zA-Z0-9]/.test(value))
    return 'Password must contain at least one special character.';
}

export function validatePasswordConfirm(password: string, repeatPassword: string): string | undefined {
  if (repeatPassword && password !== repeatPassword) return 'Passwords do not match.';
}
