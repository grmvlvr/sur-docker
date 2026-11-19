export function formatMoney(value?: number | null): string {
  return `${Number(value || 0).toLocaleString('ru-RU')} ₽`;
}
