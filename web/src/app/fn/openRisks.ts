import { Risk } from '../models';

export function openRisks(risks: Risk[]): Risk[] {
  return risks.filter((risk) => risk.status !== 'Закрыт');
}
