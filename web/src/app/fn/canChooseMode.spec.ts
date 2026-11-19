import { RiskCard, User } from '../models';
import { canChooseMode } from './canChooseMode';
import { sampleRisk } from './sampleRisk';

describe('canChooseMode', () => {
  const owner: User = { id: 'u-own', name: 'Владелец выпуска', login: 'owner', roles: ['OWNER'], directorateId: 'd-prod' };

  function card(status: string, responseMode?: string): RiskCard {
    return {
      risk: sampleRisk({
        status,
        assessment: { likelihood: 2, lossAmount: 50000, impact: 1, score: 2, level: 'OWNER', isActive: true, responseMode: responseMode ?? null }
      }),
      treatments: [],
      comments: [],
      log: []
    };
  }

  it('разрешает выбор режима активному риску без режима', () => {
    expect(canChooseMode(owner, card('Активен'))).toBeTrue();
  });

  it('запрещает повторный выбор режима', () => {
    expect(canChooseMode(owner, card('Активен', 'treat'))).toBeFalse();
  });
});
