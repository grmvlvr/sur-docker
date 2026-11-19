import { heatClass } from './heatClass';

describe('heatClass', () => {
  it('красит ячейку по произведению вероятности и влияния', () => {
    expect(heatClass(1, 1)).toBe('s1');
    expect(heatClass(2, 3)).toBe('s2');
    expect(heatClass(3, 4)).toBe('s3');
    expect(heatClass(4, 4)).toBe('s4');
    expect(heatClass(5, 4)).toBe('s5');
  });
});
