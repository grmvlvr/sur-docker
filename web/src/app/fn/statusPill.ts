import { STATUS_CLASS } from '../models';

export function statusPill(status: string): string {
  return `pill ${STATUS_CLASS[status] || 'gray'}`;
}
