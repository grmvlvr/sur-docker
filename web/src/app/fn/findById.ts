export function findById<T extends { id: string }>(items: T[], id?: string | null): T | undefined {
  return items.find((item) => item.id === id);
}
