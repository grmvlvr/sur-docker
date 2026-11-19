export function readEventValue(event: Event): string {
  const target = event.target;
  if (target instanceof HTMLInputElement || target instanceof HTMLSelectElement) return target.value;
  return '';
}
