import { Meta } from '../models';

export function sampleMeta(): Meta {
  return {
    me: { id: 'u-own', name: 'Владелец выпуска', login: 'owner@sur.local', roles: ['OWNER'], directorateId: 'd-prod' },
    users: [
      { id: 'u-own', name: 'Владелец выпуска', login: 'owner@sur.local', roles: ['OWNER'], directorateId: 'd-prod' },
      { id: 'u-emp', name: 'Линейный сотрудник', login: 'employee@sur.local', roles: ['EMPLOYEE'], directorateId: 'd-prod' },
      { id: 'u-dir', name: 'Директор производства', login: 'director@sur.local', roles: ['DIRECTOR'], directorateId: 'd-prod' }
    ],
    directorates: [
      { id: 'd-prod', name: 'Производство' },
      { id: 'd-fin', name: 'Финансы' }
    ],
    processes: [
      { id: 'p-out', name: 'Выпуск продукции', directorateId: 'd-prod', ownerId: 'u-own', isActive: true },
      { id: 'p-bud', name: 'Бюджетирование', directorateId: 'd-fin', ownerId: 'u-fin', isActive: true }
    ],
    objects: [],
    hazards: [],
    categories: [],
    roles: [],
    thresholds: { ownerCost: 0, directorCost: 50000, ceoCost: 100000, ownerMaxScore: 10, directorMaxScore: 15, ceoMaxScore: 19 },
    scales: { likelihood: [], impact: [], levels: { OWNER: 'Владелец процесса' } }
  };
}
