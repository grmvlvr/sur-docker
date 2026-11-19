import { Component } from '@angular/core';
import { SurApi } from '../api.service';
import { topRisks } from '../fn/topRisks';
import { RiskTableComponent } from './risk-table.component';

@Component({
  selector: 'app-top',
  imports: [RiskTableComponent],
  template: `
    <div class="page-head"><h1>Топ-риски</h1></div>
    <app-risk-table [rows]="rows()" />
  `
})
export class TopPage {
  constructor(readonly api: SurApi) {}
  rows() { return topRisks(this.api.risks()); }
}
