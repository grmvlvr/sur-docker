import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SurApi } from '../api.service';
import { myProcesses } from '../fn/myProcesses';
import { Risk } from '../models';

@Component({
  selector: 'app-risk-form',
  imports: [FormsModule],
  template: `
    <div class="modal-back" (click)="close($event)">
      <form class="modal" (ngSubmit)="save()">
        <div class="row-between"><h2>{{ riskId ? 'Паспорт черновика' : 'Новый риск' }}</h2><button type="button" (click)="closed.emit()">✕</button></div>
        <p class="hint">Риск создаётся как «Черновик» без оценки. Основной процесс один; дополнительные связи можно указать сразу.</p>
        <div class="field" [class.invalid]="showError && !title.trim()"><label>Название</label><input name="title" [(ngModel)]="title" /></div>
        <div class="field" [class.invalid]="showError && !hazardId"><label>Опасность</label><select name="hazardId" [(ngModel)]="hazardId"><option value="">Выберите</option>@for (item of api.meta()?.hazards; track item.id) { <option [value]="item.id">{{ item.name }}</option> }</select></div>
        <div class="field" [class.invalid]="showError && !processId"><label>Основной процесс</label><select name="processId" [(ngModel)]="processId" [disabled]="!!riskId"><option value="">Выберите</option>@for (item of processes(); track item.id) { <option [value]="item.id">{{ item.name }}</option> }</select></div>
        @if (!riskId) {
          <div class="field">
            <label>Дополнительные процессы</label>
            @for (item of extras(); track item.id) {
              <label class="switch"><input type="checkbox" [checked]="extraIds.includes(item.id)" (change)="toggle(item.id, $any($event.target).checked)" /> {{ item.name }}</label>
            }
          </div>
        }
        <div class="field" [class.invalid]="showError && !categoryId"><label>Категория</label><select name="categoryId" [(ngModel)]="categoryId"><option value="">Выберите</option>@for (item of api.meta()?.categories; track item.id) { <option [value]="item.id">{{ item.name }}</option> }</select></div>
        <div class="field"><label>Объект</label><select name="objectId" [(ngModel)]="objectId"><option value="">Не связывать</option>@for (item of api.meta()?.objects; track item.id) { <option [value]="item.id">{{ item.name }}</option> }</select></div>
        <label class="switch"><input type="checkbox" name="top" [(ngModel)]="top" /> Топ-риск</label>
        <div class="field" [class.invalid]="showError && !description.trim()"><label>Описание</label><textarea name="description" [(ngModel)]="description"></textarea></div>
        <div class="modal-actions"><button class="btn primary">Сохранить</button><button class="btn" type="button" (click)="closed.emit()">Отменить</button></div>
      </form>
    </div>
  `
})
export class RiskFormComponent implements OnInit {
  @Input() riskId = '';
  @Input() source: Risk | null = null;
  title = '';
  hazardId = '';
  processId = '';
  categoryId = '';
  objectId = '';
  description = '';
  top = false;
  extraIds: string[] = [];
  showError = false;
  @Output() closed = new EventEmitter<void>();

  constructor(readonly api: SurApi) {}

  ngOnInit() {
    this.hazardId = this.source?.hazardId || '';
    this.categoryId = this.source?.categoryId || '';
    this.processId = this.source?.processes.find((item) => item.isPrimary && !item.unlinkedAt)?.processId || '';
    this.title = this.source?.title ?? '';
    this.objectId = this.source?.objectId ?? '';
    this.description = this.source?.description ?? '';
    this.top = this.source?.top ?? false;
  }

  processes() { return myProcesses(this.api.meta()); }
  extras() { return this.processes().filter((item) => item.id !== this.processId); }
  toggle(id: string, on: boolean) {
    this.extraIds = on ? [...this.extraIds.filter((item) => item !== id), id] : this.extraIds.filter((item) => item !== id);
  }

  save() {
    this.showError = true;
    if (!this.title.trim() || !this.hazardId || !this.processId || !this.categoryId || !this.description.trim()) return;
    const body = {
      title: this.title,
      hazardId: this.hazardId,
      processId: this.processId,
      additionalProcessIds: this.extraIds.filter((id) => id !== this.processId),
      categoryId: this.categoryId,
      objectId: this.objectId,
      description: this.description,
      top: this.top
    };
    const done = () => this.closed.emit();
    if (this.riskId) this.api.put(`/api/risks/${this.riskId}`, body, done);
    else this.api.httpPost('/api/risks', body, done);
  }

  close(event: MouseEvent) { if (event.target === event.currentTarget) this.closed.emit(); }
}
