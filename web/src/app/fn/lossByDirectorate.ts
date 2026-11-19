import { Meta, Risk } from '../models';
import { findById } from './findById';
import { lossSum } from './lossSum';
import { primaryLink } from './primaryLink';

export function lossByDirectorate(risks: Risk[], meta: Meta | null): { name: string; count: number; sum: number }[] {
  return (meta?.directorates ?? []).map((directorate) => {
    const rows = risks.filter((risk) => findById(meta?.processes ?? [], primaryLink(risk).processId)?.directorateId === directorate.id);
    return { name: directorate.name, count: rows.length, sum: lossSum(rows) };
  });
}
