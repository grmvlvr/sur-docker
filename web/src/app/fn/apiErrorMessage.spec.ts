import { apiErrorMessage } from './apiErrorMessage';

describe('apiErrorMessage', () => {
  it('показывает сообщение сервера', () => {
    expect(apiErrorMessage({ error: { message: 'Риск не найден.' } })).toBe('Риск не найден.');
  });

  it('сообщает, что сервер недоступен', () => {
    expect(apiErrorMessage(new Error('network'))).toBe('Сервер недоступен. Запустите Sur.Api на порту 5080.');
  });
});
