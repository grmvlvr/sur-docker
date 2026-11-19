export function readEventChecked(event: Event): boolean {
  return event.target instanceof HTMLInputElement && event.target.checked;
}
