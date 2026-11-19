import { executorsOf } from './executorsOf';
import { sampleMeta } from './sampleMeta';

describe('executorsOf', () => {
  it('оставляет владельцев и сотрудников дирекции', () => {
    expect(executorsOf(sampleMeta().users, 'd-prod').map((user) => user.login)).toEqual(['owner@sur.local', 'employee@sur.local']);
  });
});
