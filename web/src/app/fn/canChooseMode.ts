import { RiskCard, User } from '../models';
import { isPrimaryOwner } from './isPrimaryOwner';

export function canChooseMode(me: User | null | undefined, data: RiskCard): boolean {
  return data.risk.status === 'Активен' && !!data.risk.assessment?.isActive && !data.risk.assessment.responseMode && isPrimaryOwner(me, data.risk);
}
