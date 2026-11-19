import { Meta, Risk } from '../models';
import { findById } from './findById';
import { primaryLink } from './primaryLink';

export function filterRisks(rows: Risk[], meta: Meta | null, query: string, status: string, direction: string, processId: string): Risk[] {
  return rows.filter((risk) => {
    const blob = `${risk.title} ${risk.description} ${risk.id}`.toLowerCase();
    if (query && !blob.includes(query.toLowerCase())) return false;
    if (status && risk.status !== status) return false;
    const process = findById(meta?.processes ?? [], primaryLink(risk).processId);
    if (direction && process?.directorateId !== direction) return false;
    if (processId && process?.id !== processId) return false;
    return true;
  });
}
