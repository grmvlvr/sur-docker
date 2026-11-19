import { myProcesses } from './myProcesses';
import { sampleMeta } from './sampleMeta';

describe('myProcesses', () => {
  it('показывает владельцу только процессы своей дирекции', () => {
    expect(myProcesses(sampleMeta()).map((process) => process.id)).toEqual(['p-out']);
  });

  it('показывает администратору процессы всех дирекций', () => {
    const meta = sampleMeta();
    meta.me = { ...meta.me, roles: ['ADMIN'] };
    expect(myProcesses(meta).map((process) => process.id)).toEqual(['p-out', 'p-bud']);
  });
});
