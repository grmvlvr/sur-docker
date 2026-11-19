import { filterRisks } from './filterRisks';
import { sampleMeta } from './sampleMeta';
import { sampleRisk } from './sampleRisk';

describe('filterRisks', () => {
  const rows = [
    sampleRisk(),
    sampleRisk({ id: '246102', title: 'Срыв бюджета', status: 'Активен', processes: [{ processId: 'p-bud', isPrimary: true, ownerId: 'u-fin' }] })
  ];

  it('оставляет риск по названию, статусу и дирекции', () => {
    const found = filterRisks(rows, sampleMeta(), 'бюджет', 'Активен', 'd-fin', '');
    expect(found.map((risk) => risk.id)).toEqual(['246102']);
  });

  it('отсекает чужой процесс', () => {
    const found = filterRisks(rows, sampleMeta(), '', '', '', 'p-out');
    expect(found.map((risk) => risk.id)).toEqual(['246101']);
  });
});
