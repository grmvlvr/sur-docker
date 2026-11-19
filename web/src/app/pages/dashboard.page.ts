import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { SurApi } from '../api.service';
import { countByStatus } from '../fn/countByStatus';
import { lossByDirectorate } from '../fn/lossByDirectorate';
import { lossSum } from '../fn/lossSum';
import { openRisks } from '../fn/openRisks';
import { statusPill } from '../fn/statusPill';
import { HeatComponent } from './heat.component';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, HeatComponent],
  template: `
    <div class="page-head"><h1>Основные показатели</h1><a class="btn primary" routerLink="/risks">Реестр рисков</a></div>
    <div class="kpi-grid">
      <div class="kpi total"><div class="label">Рисков в реестре</div><div class="value">{{ api.risks().length }}</div></div>
      @for (item of cards(); track item.label) {
        <div class="kpi"><div class="label">{{ item.label }}</div><div class="value">{{ item.value }}</div></div>
      }
    </div>
    <div class="charts">
      <div class="card">
        <div class="muted-title">Сумма последствий по дирекциям</div>
        <div class="big-money">{{ api.money(sum()) }}</div>
        @for (row of byDirectorate(); track row.name) { <p>{{ row.name }}: <b>{{ row.count }}</b> · {{ api.money(row.sum) }}</p> }
      </div>
      <div class="card"><div class="muted-title">Тепловая карта 5×5</div><app-heat /></div>
    </div>
    <h2>Последние риски</h2>
    @for (risk of live().slice(0, 3); track risk.id) {
      <article class="risk-card">
        <div class="pills"><span class="pill" [class]="pill(risk.status)">{{ risk.status }}</span> <span>Id {{ risk.id }}</span></div>
        <h3><a [routerLink]="['/risk', risk.id]">{{ risk.title }}</a></h3>
        <p class="sub">{{ risk.description }}</p>
      </article>
    }
  `
})
export class DashboardPage {
  constructor(readonly api: SurApi) {}
  live() { return openRisks(this.api.risks()); }
  sum() { return lossSum(this.live()); }
  cards() {
    const risks = this.api.risks();
    const open = this.live();
    return [
      { label: 'Черновики', value: countByStatus(risks, 'Черновик') },
      { label: 'Активные', value: countByStatus(risks, 'Активен') },
      { label: 'Ждут переоценки', value: countByStatus(risks, 'Ожидает переоценки') },
      { label: 'Принятые', value: countByStatus(risks, 'Принят') },
      { label: 'Обязательная обработка', value: open.filter((risk) => risk.assessment?.level === 'TREATMENT_REQUIRED').length },
      { label: 'Мер в работе', value: this.api.treatments().filter((item) => ['В работе', 'Ожидает выполнения', 'На согласовании'].includes(item.status)).length },
      { label: 'Сумма оценённых', value: this.api.money(this.sum()) }
    ];
  }
  byDirectorate() { return lossByDirectorate(this.live(), this.api.meta()); }
  pill(status: string) { return statusPill(status); }
}
