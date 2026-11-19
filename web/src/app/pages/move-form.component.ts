import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SurApi } from '../api.service';
import { readEventChecked } from '../fn/readEventChecked';
import { RiskCard } from '../models';

@Component({
  selector: 'app-move',
  imports: [FormsModule],
  template: `
    <div class="modal-back" (click)="back($event)"><form class="modal" (ngSubmit)="save()">
      <h2>{{ splitMode ? 'Разделить риск' : 'Передать риск' }}</h2>
      @for (item of api.meta()?.processes; track item.id) {
        <label class="switch"><input [type]="splitMode ? 'checkbox' : 'radio'" [value]="item.id" (change)="pick(item.id, readEventChecked($event))" [checked]="ids.includes(item.id)" /> {{ item.name }}</label>
      }
      @if (splitMode) { <div class="field"><label>Основной процесс</label><select name="primaryId" [(ngModel)]="primaryId">@for (id of ids; track id) { <option [value]="id">{{ api.process(id)?.name }}</option> }</select></div> }
      <div class="modal-actions"><button class="btn primary">Подтвердить</button><button class="btn" type="button" (click)="closed.emit()">Отменить</button></div>
    </form></div>
  `
})
export class MoveFormComponent {
  @Input({ required: true }) risk!: RiskCard['risk'];
  @Input() splitMode = false;
  @Output() closed = new EventEmitter<void>();
  ids: string[] = [];
  primaryId = '';
  readonly readEventChecked = readEventChecked;
  constructor(readonly api: SurApi) {}
  pick(id: string, checked: boolean) {
    if (!this.splitMode) this.ids = [id];
    else this.ids = checked ? [...this.ids, id] : this.ids.filter((item) => item !== id);
    this.primaryId = this.ids[0] ?? '';
  }
  save() {
    const body = this.splitMode
      ? { mode: 'split', processIds: this.ids, primaryId: this.primaryId }
      : { mode: 'transfer', processId: this.ids[0] };
    this.api.httpPost(`/api/risks/${this.risk.id}/mode`, body, () => this.closed.emit());
  }
  back(event: MouseEvent) { if (event.target === event.currentTarget) this.closed.emit(); }
}
