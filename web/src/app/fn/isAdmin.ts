import { User } from '../models';

export function isAdmin(me?: User | null): boolean {
  return !!me?.roles.includes('ADMIN');
}
