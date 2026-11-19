import { Risk } from '../models';

export function closedRisks(risks: Risk[]): Risk[] {
  return risks.filter((risk) => risk.status === 'Закрыт');
}
