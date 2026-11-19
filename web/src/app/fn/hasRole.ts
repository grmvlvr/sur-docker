import { User } from '../models';
import { isAdmin } from './isAdmin';

export function hasRole(me: User | null | undefined, role: string): boolean {
  return isAdmin(me) || !!me?.roles.includes(role);
}
