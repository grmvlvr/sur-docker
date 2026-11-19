import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SurApi } from '../api.service';
import { filterRisks } from '../fn/filterRisks';
import { modeLabel } from '../fn/modeLabel';
import { readEventValue } from '../fn/readEventValue';
import { statusPill } from '../fn/statusPill';
import { Risk } from '../models';

@Component({
  selector: 'app-risk-table',
  imports: [RouterLink],
  template: `
    <div class="filters">
      <input class="search" placeholder="Поиск" [value]="query" (input)="query = readEventValue($event)" />
      <select (change)="direction = readEventValue($event)">
        <option value="">Дирекция</option>
        @for (item of api.meta()?.directorates; track item.id) { <option [value]="item.id">{{ item.name }}</option> }
      </select>
      <select (change)="process = readEventValue($event)">
        <option value="">Процесс</option>
        @for (item of api.meta()?.processes; track item.id) { <option [value]="item.id">{{ item.name }}</option> }
      </select>
      <select (change)="status = readEventValue($event)">
        <option value="">Статус</option>
        @for (item of statuses; track item) { <option [value]="item">{{ item }}</option> }
      </select>
    </div>
    <div class="table-wrap"><table>
      <thead><tr><th>ID</th><th>Название</th><th>Опасность</th><th>Процесс</th><th>Дирекция</th><th>R</th><th>Уровень</th><th>Режим</th><th>Статус</th><th></th></tr></thead>
      <tbody>
        @for (risk of filtered(); track risk.id) {
          <tr>
            <td><a [routerLink]="['/risk', risk.id]">{{ risk.id }}</a></td>
            <td><a [routerLink]="['/risk', risk.id]">{{ risk.title }}</a></td>
            <td>{{ api.hazard(risk.hazardId)?.name }}</td>
            <td>{{ api.process(api.primary(risk).processId)?.name }}</td>
            <td>{{ api.directorate(api.process(api.primary(risk).processId)?.directorateId)?.name }}</td>
            <td>{{ risk.assessment?.score ?? '—' }}</td>
            <td>{{ api.level(risk.assessment?.level) }}</td>
            <td>{{ mode(risk.assessment?.responseMode) }}</td>
            <td><span class="pill" [class]="statusClass(risk.status)">{{ risk.status }}</span></td>
            <td><a class="btn" [routerLink]="['/risk', risk.id]">Открыть</a></td>
          </tr>
        } @empty { <tr><td colspan="10" class="empty">Ничего не найдено</td></tr> }
      </tbody>
    </table></div>
  `
})
export class RiskTableComponent {
  @Input() rows: Risk[] = [];
  query = '';
  direction = '';
  process = '';
  status = '';
  statuses = ['Черновик', 'Активен', 'Ожидает переоценки', 'Принят', 'Закрыт'];

  readonly readEventValue = readEventValue;

  constructor(readonly api: SurApi) {}

  filtered() { return filterRisks(this.rows, this.api.meta(), this.query, this.status, this.direction, this.process); }
  mode(value?: string | null) { return modeLabel(value); }
  statusClass(status: string) { return statusPill(status); }
}
