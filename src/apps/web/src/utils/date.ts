/**
 * Date utility functions for formatting ISO 8601 date strings
 */

/**
 * Parse an ISO date string to a Date object
 */
export function parseISODate(dateString: string | undefined): Date | null {
  if (!dateString) return null;
  const date = new Date(dateString);
  return isNaN(date.getTime()) ? null : date;
}

/**
 * Format an ISO date string to a localized date string
 */
export function formatDate(dateString: string | undefined): string {
  const date = parseISODate(dateString);
  if (!date) return '';
  return date.toLocaleDateString();
}

/**
 * Format an ISO date string to a localized date and time string
 */
export function formatDateTime(dateString: string | undefined): string {
  const date = parseISODate(dateString);
  if (!date) return '';
  return date.toLocaleString();
}

/**
 * Format an ISO date string to a relative time (e.g., "2 hours ago")
 */
export function formatRelativeTime(dateString: string | undefined): string {
  const date = parseISODate(dateString);
  if (!date) return '';

  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffSecs = Math.floor(diffMs / 1000);
  const diffMins = Math.floor(diffSecs / 60);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffDays > 0) {
    return `${diffDays} day${diffDays === 1 ? '' : 's'} ago`;
  } else if (diffHours > 0) {
    return `${diffHours} hour${diffHours === 1 ? '' : 's'} ago`;
  } else if (diffMins > 0) {
    return `${diffMins} minute${diffMins === 1 ? '' : 's'} ago`;
  } else {
    return 'just now';
  }
}

/**
 * Check if a date string represents a date in the past
 */
export function isPast(dateString: string | undefined): boolean {
  const date = parseISODate(dateString);
  if (!date) return false;
  return date < new Date();
}

/**
 * Check if a date string represents a date in the future
 */
export function isFuture(dateString: string | undefined): boolean {
  const date = parseISODate(dateString);
  if (!date) return false;
  return date > new Date();
}