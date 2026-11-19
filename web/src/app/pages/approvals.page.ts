import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SurApi } from '../api.service';
import { AskField, AskFormComponent } from './ask-form.component';

interface ApprovalRow { id: string; treatmentId: string; level: string; status: string; title: string; riskId: string; cost: number; }

@Component({
  selector: 'app-approvals',
  imports: [RouterLink, AskFormComponent],
  template: `
    <div class="page-head"><h1>На согласование</h1></div>
    <p class="sub">Открытые запросы, которые вы можете решить. Отзыв доступен администратору.</p>
    @for (item of rows(); track item.id) {
      <article class="risk-card">
        <h3><a [routerLink]="['/risk', item.riskId]">{{ item.title }}</a></h3>
        <p>{{ item.level }} · {{ api.money(item.cost) }} · {{ item.status }}</p>
        <div class="actions">
          @if (item.status === 'На согласовании') {
            <button class="btn primary" type="button" (click)="decide(item.treatmentId, true)">Согласовать</button>
            <button class="btn" type="button" (click)="decide(item.treatmentId, false)">Отклонить</button>
          }
          @if (api.isAdmin()) { <button class="btn" type="button" (click)="act('/api/approvals/' + item.id + '/revoke')">Отозвать</button> }
        </div>
      </article>
    } @empty { <div class="card empty">Нет запросов для вашей роли.</div> }
    @if (dialog) { <app-ask [title]="dialog.title" [hint]="dialog.hint" [fields]="dialog.fields" [submitLabel]="dialog.submit" (closed)="dialog = null" (saved)="applyAsk($event)" /> }
  `
})
export class ApprovalsPage implements OnInit {
  rows = signal<ApprovalRow[]>([]);
  dialog: { title: string; hint: string; fields: AskField[]; submit: string; apply: (values: Record<string, string>) => void } | null = null;
  constructor(readonly api: SurApi) {}
  ngOnInit() { this.load(); }
  load() {
    this.api.get<ApprovalRow[]>('/api/approvals').subscribe({
      next: (rows) => this.rows.set(rows),
      error: (err: unknown) => this.api.fail(err)
    });
  }
  act(url: string, body?: unknown) {
    this.api.httpPost(url, body, () => this.load());
  }
  decide(treatmentId: string, approve: boolean) {
    this.dialog = {
      title: approve ? 'Согласовать меру' : 'Отклонить меру',
      hint: approve ? 'Комментарий необязателен.' : 'Укажите причину отклонения.',
      submit: approve ? 'Согласовать' : 'Отклонить',
      fields: [{ name: 'comment', label: approve ? 'Комментарий' : 'Причина', kind: 'textarea', required: !approve }],
      apply: (values) => this.act(`/api/treatments/${treatmentId}/decision`, { approve, comment: values['comment'].trim() })
    };
  }
  applyAsk(values: Record<string, string>) {
    const current = this.dialog;
    this.dialog = null;
    current?.apply(values);
  }
}
