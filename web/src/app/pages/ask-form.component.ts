import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface AskOption { value: string; label: string; }
export interface AskField {
  name: string;
  label: string;
  kind: 'text' | 'number' | 'textarea' | 'select' | 'checks';
  value?: string;
  required?: boolean;
  options?: AskOption[];
}

@Component({
  selector: 'app-ask',
  imports: [FormsModule],
  template: `
    <div class="modal-back" (click)="back($event)"><form class="modal" (ngSubmit)="save()">
      <h2>{{ title }}</h2>
      @if (hint) { <p class="hint">{{ hint }}</p> }
      @for (field of fields; track field.name) {
        <div class="field" [class.invalid]="invalid(field)">
          <label>{{ field.label }}</label>
          @switch (field.kind) {
            @case ('textarea') { <textarea [name]="field.name" [ngModel]="text(field.name)" (ngModelChange)="set(field.name, $event)"></textarea> }
            @case ('select') {
              <select [name]="field.name" [ngModel]="text(field.name)" (ngModelChange)="set(field.name, $event)">
                <option value="">Выберите</option>
                @for (option of field.options; track option.value) { <option [value]="option.value">{{ option.label }}</option> }
              </select>
            }
            @case ('checks') {
              @for (option of field.options; track option.value) {
                <label class="switch">
                  <input type="checkbox" [checked]="checked(field.name, option.value)" (change)="toggle(field.name, option.value, $any($event.target).checked)" [disabled]="locked(field, option.value)" />
                  {{ option.label }}
                </label>
              }
            }
            @default { <input [name]="field.name" [type]="field.kind === 'number' ? 'number' : 'text'" [ngModel]="text(field.name)" (ngModelChange)="set(field.name, $event)" /> }
          }
        </div>
      }
      <div class="modal-actions"><button class="btn primary" type="submit">{{ submitLabel }}</button><button class="btn" type="button" (click)="closed.emit()">Отмена</button></div>
    </form></div>
  `
})
export class AskFormComponent {
  @Input({ required: true }) title = '';
  @Input() hint = '';
  @Input() fields: AskField[] = [];
  @Input() submitLabel = 'Сохранить';
  @Input() lockedValues: string[] = [];
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<Record<string, string>>();
  values: Record<string, string> = {};
  showError = false;

  ngOnInit() {
    for (const field of this.fields) this.values[field.name] = field.value ?? '';
  }

  text(name: string) { return this.values[name] ?? ''; }
  set(name: string, value: string | number | null) { this.values[name] = value == null ? '' : String(value); }
  selected(name: string) {
    return this.text(name).split(',').map((item) => item.trim()).filter(Boolean);
  }
  checked(name: string, value: string) { return this.selected(name).includes(value); }
  locked(field: AskField, value: string) { return field.name === 'rights' && this.lockedValues.includes(value); }
  toggle(name: string, value: string, on: boolean) {
    const next = new Set(this.selected(name));
    if (on) next.add(value); else next.delete(value);
    this.values[name] = [...next].join(',');
  }
  invalid(field: AskField) {
    if (!(this.showError && field.required)) return false;
    if (field.kind === 'checks') return this.selected(field.name).length === 0;
    return !this.text(field.name).trim();
  }

  save() {
    this.showError = true;
    if (this.fields.some((field) => this.invalid(field))) return;
    this.saved.emit({ ...this.values });
  }

  back(event: MouseEvent) { if (event.target === event.currentTarget) this.closed.emit(); }
}
