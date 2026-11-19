import { Risk } from '../models';

export function riskTitle(risks: Risk[], id: string): string | undefined {
  return risks.find((risk) => risk.id === id)?.title;
}
