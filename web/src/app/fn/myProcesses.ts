import { Meta, Process, User } from '../models';
import { isAdmin } from './isAdmin';

export function myProcesses(meta: Meta | null): Process[] {
  const me: User | undefined = meta?.me;
  return (meta?.processes ?? []).filter((process) => process.isActive !== false && (isAdmin(me) || process.directorateId === me?.directorateId));
}
