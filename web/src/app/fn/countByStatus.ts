export function countByStatus<T extends { status: string }>(items: T[], status: string): number {
  return items.filter((item) => item.status === status).length;
}
