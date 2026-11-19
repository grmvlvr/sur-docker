import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SurApi } from '../api.service';
import { RiskCard } from '../models';

@Component({
  selector: 'app-assess',
  imports: [FormsModule],
  template: `
    <div class="modal-back" (click)="back($event)"><form class="modal" (ngSubmit)="save()">
      <h2>Оценка риска</h2>
      <div class="field"><label>Вероятность</label><select name="likelihood" [(ngModel)]="likelihood">@for (item of api.meta()?.scales?.likelihood; track item.level) { <option [ngValue]="item.level">{{ item.level }}. {{ item.name }}</option> }</select></div>
      @if (risk.objectId) {
        <div class="field"><label>Простой, недели</label><input name="downtimeWeeks" type="number" min="0" [(ngModel)]="downtimeWeeks" /></div>
        <div class="field"><label>Простой, дни</label><input name="downtimeDays" type="number" min="0" [(ngModel)]="downtimeDays" /></div>
        <div class="field"><label>Простой, часы</label><input name="downtimeHours" type="number" min="0" [(ngModel)]="downtimeHours" /></div>
      }
      @else { <div class="field"><label>Сумма последствий, ₽</label><input name="lossAmount" type="number" min="0" [(ngModel)]="lossAmount" /></div> }
      <div class="modal-actions"><button class="btn primary">Рассчитать и активировать</button><button class="btn" type="button" (click)="closed.emit()">Отменить</button></div>
    </form></div>
  `
})
export class AssessFormComponent {
  @Input({ required: true }) risk!: RiskCard['risk'];
  @Output() closed = new EventEmitter<void>();
  likelihood = 3;
  lossAmount = 0;
  downtimeWeeks = 0;
  downtimeDays = 0;
  downtimeHours = 0;
  constructor(readonly api: SurApi) {}
  save() {
    this.api.httpPost(`/api/risks/${this.risk.id}/assess`, { likelihood: this.likelihood, lossAmount: this.lossAmount, downtimeWeeks: this.downtimeWeeks, downtimeDays: this.downtimeDays, downtimeHours: this.downtimeHours }, () => this.closed.emit());
  }
  back(event: MouseEvent) { if (event.target === event.currentTarget) this.closed.emit(); }
}
