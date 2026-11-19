import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SurApi } from '../api.service';
import { RiskCard } from '../models';

@Component({
  selector: 'app-treatment',
  imports: [FormsModule],
  template: `
    <div class="modal-back" (click)="back($event)"><form class="modal" (ngSubmit)="save()">
      <h2>Новая мера</h2>
      <div class="field" [class.invalid]="showError && !title.trim()"><label>Название</label><input name="title" [(ngModel)]="title" /></div>
      <div class="field" [class.invalid]="showError && !processId"><label>Процесс</label><select name="processId" [(ngModel)]="processId"><option value="">Выберите</option>@for (item of risk.processes; track item.processId) { @if (!item.unlinkedAt) { <option [value]="item.processId">{{ api.process(item.processId)?.name }}</option> } }</select></div>
      <div class="field"><label>Плановая стоимость</label><input name="costPlanned" type="number" [(ngModel)]="costPlanned" /></div>
      <div class="field" [class.invalid]="showError && !due.trim()"><label>Срок</label><input name="due" [(ngModel)]="due" /></div>
      <div class="field"><label>Описание</label><textarea name="description" [(ngModel)]="description"></textarea></div>
      <div class="modal-actions"><button class="btn primary">Создать черновик</button><button class="btn" type="button" (click)="closed.emit()">Отменить</button></div>
    </form></div>
  `
})
export class TreatmentFormComponent implements OnInit {
  @Input({ required: true }) risk!: RiskCard['risk'];
  @Output() closed = new EventEmitter<void>();
  title = '';
  processId = '';
  costPlanned = 0;
  due = '';
  description = '';
  showError = false;
  constructor(readonly api: SurApi) {}
  ngOnInit() { this.processId = ''; }
  save() {
    this.showError = true;
    if (!this.title.trim() || !this.processId || !this.due.trim()) return;
    this.api.httpPost(`/api/risks/${this.risk.id}/treatments`, { title: this.title, processId: this.processId, costPlanned: this.costPlanned, due: this.due, description: this.description }, () => this.closed.emit());
  }
  back(event: MouseEvent) { if (event.target === event.currentTarget) this.closed.emit(); }
}
