import { Risk } from '../models';

export function heatCount(risks: Risk[], likelihood: number, impact: number): number {
  return risks.filter((risk) => risk.assessment?.isActive && risk.assessment.likelihood === likelihood && risk.assessment.impact === impact).length;
}
