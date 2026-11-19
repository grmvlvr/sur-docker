import { MODE_LABEL } from '../models';

export function modeLabel(value?: string | null, empty = '—'): string {
  return value ? MODE_LABEL[value] : empty;
}
