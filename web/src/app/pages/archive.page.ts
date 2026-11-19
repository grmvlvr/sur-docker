import { Component } from '@angular/core';
import { SurApi } from '../api.service';
import { closedRisks } from '../fn/closedRisks';
import { RiskTableComponent } from './risk-table.component';

@Component({
  selector: 'app-archive',
  imports: [RiskTableComponent],
  template: `
    <div class="page-head"><h1>Архив</h1></div>
    <p class="sub">Закрытые риски. Записи не удаляются.</p>
    <app-risk-table [rows]="rows()" />
  `
})
export class ArchivePage {
  constructor(readonly api: SurApi) {}
  rows() { return closedRisks(this.api.risks()); }
}
