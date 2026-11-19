export function heatClass(likelihood: number, impact: number): string {
  const score = likelihood * impact;
  const tone = score >= 20 ? 5 : score >= 16 ? 4 : score >= 11 ? 3 : score >= 5 ? 2 : 1;
  return `s${tone}`;
}
