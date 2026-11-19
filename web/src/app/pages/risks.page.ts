import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SurApi } from '../api.service';
import { openRisks } from '../fn/openRisks';
import { RiskFormComponent } from './risk-form.component';
import { RiskTableComponent } from './risk-table.component';

@Component({
  selector: 'app-risks',
  imports: [RiskTableComponent, FormsModule, RiskFormComponent],
  template: `
    <div class="page-head">
      <h1>Реестр рисков</h1>
      <div class="actions">
        @if (canImport()) { <button class="btn" type="button" (click)="template()">Шаблон CSV</button> }
        @if (api.hasRight('create_risk')) { <button class="btn primary" type="button" (click)="open = true">+ Добавить риск</button> }
      </div>
    </div>
    @if (canImport()) {
      <div class="card">
        <div class="muted-title">Импорт реестра рисков</div>
        <input type="file" accept=".csv,text/csv" (change)="file($event)" />
        <button class="btn" type="button" (click)="upload()" [disabled]="!csv">Импортировать</button>
        @if (report) {
          <p>Создано {{ report.created }}, пропущено {{ report.skipped }}, ошибок {{ report.errors.length }}.</p>
          @for (error of report.errors; track error.line + error.field) {
            <p>Строка {{ error.line }}, {{ error.field }}: {{ error.message }}</p>
          }
        }
      </div>
    }
    <p class="sub">Цикл: черновик → оценка L×I → режим → меры → пересмотр. Снимок — в карточке риска.</p>
    <app-risk-table [rows]="live()" />
    @if (open) { <app-risk-form (closed)="open = false" /> }
  `
})
export class RisksPage {
  open = false;
  csv = '';
  report: { created: number; skipped: number; errors: { line: number; field: string; message: string }[] } | null = null;
  constructor(readonly api: SurApi) {}
  live() { return openRisks(this.api.risks()); }
  canImport() { return this.api.hasRight('create_risk'); }

  file(event: Event) {
    const input = event.target as HTMLInputElement;
    const chosen = input.files?.[0];
    if (!chosen) return;
    chosen.text().then((text) => this.csv = text);
  }

  upload() {
    this.api.importRisks(this.csv).subscribe({
      next: (result) => {
        this.report = result;
        this.api.refresh();
      },
      error: (err) => this.api.fail(err)
    });
  }

  template() {
    const text = 'sourceRiskKey;title;description;hazard;category;primaryProcess;additionalProcesses\n';
    const link = document.createElement('a');
    link.href = URL.createObjectURL(new Blob([text], { type: 'text/csv;charset=utf-8' }));
    link.download = 'risks-template.csv';
    link.click();
  }
}
