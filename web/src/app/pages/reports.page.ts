import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { SurApi } from '../api.service';
import { countByStatus } from '../fn/countByStatus';
import { riskTitle } from '../fn/riskTitle';
import { statusPill } from '../fn/statusPill';
import { taskLabel } from '../fn/taskLabel';
import { Treatment } from '../models';
import { HeatComponent } from './heat.component';

@Component({
  selector: 'app-reports',
  imports: [RouterLink, HeatComponent, FormsModule],
  template: `
    <div class="page-head">
      <div>
        <h1>Отчёты</h1>
        <p class="sub">Audit Snapshot строится по одному риску: PDF и CSV с общим номером снимка. Ссылки остаются доступны после формирования.</p>
      </div>
    </div>
    <div class="card">
      <label>Риск
        <select [(ngModel)]="riskId">
          <option value="">Выберите</option>
          @for (risk of api.risks(); track risk.id) { <option [value]="risk.id">{{ risk.title }}</option> }
        </select>
      </label>
      <button class="btn primary" type="button" (click)="snapshot()">Сформировать Audit Snapshot</button>
      @if (files) {
        <p>Снимок <b>{{ files.snapshotId }}</b> готов к скачиванию.</p>
        <div class="actions">
          <a class="btn primary" [href]="files.pdf" [download]="files.snapshotId + '.pdf'">Скачать PDF</a>
          <a class="btn" [href]="files.csv" [download]="files.snapshotId + '.csv'">Скачать CSV</a>
        </div>
      }
    </div>
    <div class="charts">
      <div class="card"><div class="muted-title">Heat-map 5×5</div><app-heat /></div>
      <div class="card">
        <div class="muted-title">Статус мер</div>
        @for (status of statuses; track status) {
          <p><span class="pill" [class]="pill(status)">{{ status }}</span> {{ count(status) }}</p>
        }
      </div>
    </div>
    <h2>Меры</h2>
    <div class="table-wrap"><table>
      <thead><tr><th>Мера</th><th>Риск</th><th>Исполнитель</th><th>Внешняя задача</th><th>План</th><th>Факт</th><th>Статус</th></tr></thead>
      <tbody>
        @for (item of api.treatments(); track item.id) {
          <tr>
            <td>{{ item.title }}</td>
            <td><a [routerLink]="['/risk', item.riskId]">{{ title(item.riskId) }}</a></td>
            <td>{{ api.user(item.executorId)?.name || '—' }}</td>
            <td>{{ task(item) }}</td>
            <td>{{ api.money(item.costPlanned) }}</td>
            <td>{{ item.costActual == null ? '—' : api.money(item.costActual) }}</td>
            <td><span class="pill" [class]="pill(item.status)">{{ item.status }}</span></td>
          </tr>
        }
      </tbody>
    </table></div>
  `
})
export class ReportsPage {
  statuses = ['Черновик', 'На согласовании', 'Согласовано', 'Ожидает выполнения', 'В работе', 'Готово', 'Отменено'];
  riskId = '';
  files: { snapshotId: string; csv: string; pdf: string } | null = null;
  constructor(readonly api: SurApi) {}

  snapshot() {
    if (!this.riskId) return;
    this.api.createSnapshot(this.riskId).subscribe({
      next: (file) => this.files = file,
      error: (err) => this.api.fail(err)
    });
  }
  count(status: string) { return countByStatus(this.api.treatments(), status); }
  title(id: string) { return riskTitle(this.api.risks(), id); }
  pill(status: string) { return statusPill(status); }
  task(item: Treatment) { return taskLabel(item); }
}
