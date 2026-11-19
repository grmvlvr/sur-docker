import { RiskCard, User } from '../models';
import { canAssess } from './canAssess';
import { sampleRisk } from './sampleRisk';

describe('canAssess', () => {
  const owner: User = { id: 'u-own', name: 'Владелец выпуска', login: 'owner', roles: ['OWNER'], directorateId: 'd-prod' };

  function card(status: string): RiskCard {
    return { risk: sampleRisk({ status }), treatments: [], comments: [], log: [] };
  }

  it('разрешает оценку черновика владельцу', () => {
    expect(canAssess(owner, card('Черновик'))).toBeTrue();
  });

  it('запрещает оценку активного риска и чужому пользователю', () => {
    const employee: User = { id: 'u-emp', name: 'Сотрудник', login: 'employee', roles: ['EMPLOYEE'], directorateId: 'd-prod' };
    expect(canAssess(owner, card('Активен'))).toBeFalse();
    expect(canAssess(employee, card('Черновик'))).toBeFalse();
  });
});
