/**
 * Local-date ISO (YYYY-MM-DD) so E2E-created records classify (overdue / future
 * / due today) the same way the app's local-date handling does.
 */
export function toIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
}
