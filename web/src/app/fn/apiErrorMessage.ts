export function apiErrorMessage(err: unknown): string {
  if (typeof err === 'object' && err !== null && 'error' in err) {
    const body = (err as { error?: { message?: string } }).error;
    if (body?.message) return body.message;
  }
  return 'Сервер недоступен. Запустите Sur.Api на порту 5080.';
}
