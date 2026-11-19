import { RiskCard, User } from '../models';
import { hasRole } from './hasRole';
import { isAdmin } from './isAdmin';
import { isPrimaryOwner } from './isPrimaryOwner';

export function canAssess(me: User | null | undefined, data: RiskCard): boolean {
  const status = data.risk.status;
  return (status === 'Черновик' || status === 'Ожидает переоценки') && isPrimaryOwner(me, data.risk) && (isAdmin(me) || hasRole(me, 'OWNER'));
}
