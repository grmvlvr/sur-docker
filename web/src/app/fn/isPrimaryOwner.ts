import { Risk, User } from '../models';
import { isAdmin } from './isAdmin';
import { primaryLink } from './primaryLink';

export function isPrimaryOwner(me: User | null | undefined, risk: Risk): boolean {
  return isAdmin(me) || primaryLink(risk).ownerId === me?.id;
}
