import { primaryLink } from './primaryLink';
import { sampleRisk } from './sampleRisk';

describe('primaryLink', () => {
  it('берёт основной процесс среди связанных', () => {
    const risk = sampleRisk({
      processes: [
        { processId: 'p-out', isPrimary: false, ownerId: 'u-own', unlinkedAt: '01.01.2026' },
        { processId: 'p-buy', isPrimary: true, ownerId: 'u-own' }
      ]
    });
    expect(primaryLink(risk).processId).toBe('p-buy');
  });

  it('падает, если связанного процесса нет', () => {
    const risk = sampleRisk({ processes: [{ processId: 'p-out', isPrimary: true, ownerId: 'u-own', unlinkedAt: '01.01.2026' }] });
    expect(() => primaryLink(risk)).toThrowError('У риска нет процесса.');
  });
});
