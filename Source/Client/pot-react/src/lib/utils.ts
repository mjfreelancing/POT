import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

const isDevelopment = () => process.env.NODE_ENV === 'development';

// A no-op named function is required to avoid the linter error 'Unexpected empty arrow function'
const noop = () => void 0;

function isNumber(value: unknown): value is number {
  // isNaN caters for NaN, Infinity, Number('abc'), Math.sqrt(-1) etc.
  return typeof value === 'number' && !isNaN(value);
}

export { cn, isDevelopment, noop, isNumber };
