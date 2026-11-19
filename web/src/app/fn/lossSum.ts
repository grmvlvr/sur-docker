import { Risk } from '../models';

export function lossSum(risks: Risk[]): number {
  return risks.reduce((sum, risk) => sum + (risk.assessment?.lossAmount || 0), 0);
}
