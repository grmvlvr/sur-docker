import { User } from '../models';

export function hasRight(me: User | null | undefined, right: string): boolean {
  if (!me) return false;
  if (me.roles.includes('ADMIN')) return true;
  return (me.rights ?? []).includes(right);
}
