import { Risk, RiskProcess } from '../models';

export function primaryLink(risk: Risk): RiskProcess {
  const active = risk.processes.filter((item) => !item.unlinkedAt);
  const link = active.find((item) => item.isPrimary) ?? active[0];
  if (!link) throw new Error('У риска нет процесса.');
  return link;
}
