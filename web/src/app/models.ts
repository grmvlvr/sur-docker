export interface Named { id: string; name: string; isActive?: boolean; }
export interface Process extends Named { directorateId: string; ownerId: string; }
export interface Asset extends Named { hourly: number; }
export interface Hazard extends Named { group: string; }
export interface User { id: string; name: string; login: string; roles: string[]; directorateId?: string | null; isActive?: boolean; rights?: string[] | null; }
export interface RiskProcess { processId: string; isPrimary: boolean; ownerId: string; unlinkedAt?: string | null; }
export interface Assessment {
  likelihood: number; lossAmount: number; downtimeHours?: number | null;
  impact: number; score: number; level: string; responseMode?: string | null; isActive: boolean;
}
export interface Risk {
  id: string; iso: string; title: string; status: string; hazardId: string; categoryId?: string | null;
  description: string; processes: RiskProcess[]; objectId?: string | null; createdAt: string; top: boolean;
  assessment?: Assessment | null; history?: Assessment[];
}
export interface Treatment {
  id: string; riskId: string; processId: string; title: string; description: string;
  costPlanned: number; due?: string | null; status: string; executorId?: string | null;
  jira?: string | null; taskProvider?: string | null; taskUrl?: string | null;
  taskStatus?: string | null; taskError?: string | null; taskCreatedAt?: string | null;
  authorId: string; costActual?: number | null;
}
export interface Comment { id: string; riskId: string; text: string; authorId: string; at: string; isSystem?: boolean; }
export interface LogItem { id: string; riskId: string; text: string; at: string; author: string; }
export interface Meta {
  me: User; users: User[]; directorates: Named[]; processes: Process[]; objects: Asset[];
  hazards: Hazard[]; categories: Named[];
  roles: { code: string; name: string; isActive: boolean; rights: string[] }[];
  rightCatalog?: string[];
  thresholds: { ownerCost: number; directorCost: number; ceoCost: number; ownerMaxScore: number; directorMaxScore: number; ceoMaxScore: number };
  scales: { likelihood: { level: number; name: string }[]; impact: { level: number; name: string; from: number; to?: number | null }[]; levels: Record<string, string> };
}
export interface RiskCard {
  risk: Risk; treatments: Treatment[]; comments: Comment[]; log: LogItem[];
}
export const MODE_LABEL: Record<string, string> = { split: 'Разделить', treat: 'Обработать', transfer: 'Передать', accept: 'Принять' };
export const STATUS_CLASS: Record<string, string> = {
  'Черновик': 'gray', 'Активен': 'blue', 'Ожидает переоценки': 'orange', 'Принят': 'green', 'Закрыт': 'gray',
  'В работе': 'blue', 'Готово': 'green', 'На согласовании': 'yellow', 'Согласовано': 'green',
  'Ожидает выполнения': 'orange', 'Отменено': 'red', 'Холд': 'gray'
};
