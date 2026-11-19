import { Risk } from '../models';

export function sampleRisk(patch: Partial<Risk> = {}): Risk {
  return {
    id: '246101',
    iso: '9001',
    title: 'Срыв выпуска',
    status: 'Черновик',
    hazardId: 'h-break',
    description: 'Линия может встать',
    processes: [{ processId: 'p-out', isPrimary: true, ownerId: 'u-own' }],
    createdAt: '17.10.2026, 12:00',
    top: false,
    ...patch
  };
}
