import { Treatment } from '../models';

export function taskLabel(item: Treatment): string {
  const provider = item.taskProvider || (item.jira ? 'MOCK' : '');
  if (!provider && !item.jira) return 'без внешней задачи';
  if (!item.jira) return `${provider || 'MOCK'}: задача не создана`;
  const status = item.taskStatus ? ` · ${item.taskStatus}` : '';
  return `${provider || 'MOCK'}: ${item.jira}${status}`;
}
