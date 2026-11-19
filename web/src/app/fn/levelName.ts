export function levelName(levels: Record<string, string> | undefined, code?: string | null): string {
  if (!code) return '—';
  return levels?.[code] ?? code;
}
