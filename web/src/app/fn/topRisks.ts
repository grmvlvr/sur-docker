import { Risk } from '../models';

export function topRisks(risks: Risk[]): Risk[] {
  return risks
    .filter((risk) => risk.status !== 'Закрыт' && risk.assessment?.isActive)
    .sort((left, right) => (right.assessment?.score ?? 0) - (left.assessment?.score ?? 0));
}
