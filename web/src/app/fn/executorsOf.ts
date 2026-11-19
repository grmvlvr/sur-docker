import { User } from '../models';

export function executorsOf(users: User[], directorateId?: string): User[] {
  return users.filter((user) => user.directorateId === directorateId && (user.roles.includes('OWNER') || user.roles.includes('EMPLOYEE')));
}
