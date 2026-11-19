import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { SurApi } from '../api.service';
import { readEventValue } from '../fn/readEventValue';
import { Named, User } from '../models';
import { AskField, AskFormComponent } from './ask-form.component';

@Component({
  selector: 'app-settings',
  imports: [FormsModule, AskFormComponent],
  template: `
    @switch (section()) {
      @case ('scales') {
        <div class="page-head"><h1>Шкалы и пороги</h1></div>
        <div class="card">
          <h3>Тяжесть, нижняя граница ₽</h3>
          @for (item of api.meta()?.scales?.impact; track item.level) { <p>{{ item.level }}. {{ item.name }} — от {{ item.from }}</p> }
          @if (api.isAdmin()) {
            <div class="field"><label>Границы уровней 1–5 через запятую</label><input [value]="impactFrom" (input)="impactFrom = read($event)" /></div>
            <div class="field"><label>Потолок OWNER, DIRECTOR, CEO через запятую</label><input [value]="scoreCaps" (input)="scoreCaps = read($event)" /></div>
            <label class="switch"><input type="checkbox" [(ngModel)]="applyActive" /> Пересчитать активные риски</label>
            <button class="btn primary" type="button" (click)="saveScales()">Сохранить шкалу</button>
            <div class="field"><label>Пороги согласования OWNER, DIRECTOR, CEO</label><input [value]="costs" (input)="costs = read($event)" /></div>
            <button class="btn" type="button" (click)="saveCosts()">Сохранить согласование</button>
          }
        </div>
      }
      @case ('refs') {
        <div class="page-head"><h1>Опасности и категории</h1>@if (api.isAdmin()) { <button class="btn primary" type="button" (click)="addHazard()">+ Опасность</button> <button class="btn" type="button" (click)="addCategory()">+ Категория</button> }</div>
        <div class="table-wrap"><table><thead><tr><th>Опасность</th><th>Группа</th><th>Активна</th></tr></thead>
          <tbody>@for (item of api.meta()?.hazards; track item.id) { <tr><td>{{ item.name }}</td><td>{{ item.group }}</td><td><button class="btn" type="button" (click)="toggleHazard(item.id, item.isActive === false)">{{ item.isActive === false ? 'Включить' : 'Выключить' }}</button></td></tr> }</tbody>
        </table></div>
        <div class="table-wrap"><table><thead><tr><th>Категория</th><th></th></tr></thead>
          <tbody>@for (item of api.meta()?.categories; track item.id) { <tr><td>{{ item.name }}</td><td>@if (api.isAdmin()) { <button class="btn" type="button" (click)="toggleCategory(item)">{{ item.isActive === false ? 'Включить' : 'Выключить' }}</button> }</td></tr> }</tbody>
        </table></div>
      }
      @case ('roles') {
        <div class="page-head"><h1>Роли и доступы</h1></div>
        <div class="card"><p>Сейчас вы: <b>{{ api.meta()?.me?.name }}</b> ({{ api.meta()?.me?.roles?.join(', ') }}).</p>
          @if (api.isAdmin()) {
            @for (role of api.meta()?.roles; track role.code) {
              <p><b>{{ role.name }}</b> {{ role.isActive ? '' : '(неактивна)' }}<br>{{ role.rights.join(', ') }}
                <button class="btn" type="button" (click)="editRole(role)">Права</button>
              </p>
            }
            <button class="btn primary" type="button" (click)="addUser()">+ Пользователь</button>
          }
        </div>
        <div class="table-wrap"><table><thead><tr><th>Пользователь</th><th>Email</th><th>Роли</th><th>Дирекция</th></tr></thead>
          <tbody>@for (user of api.meta()?.users; track user.id) { <tr><td>{{ user.name }}</td><td>{{ user.login }}</td><td>{{ user.roles.join(', ') }}</td><td>{{ api.directorate(user.directorateId)?.name || '—' }}</td>@if (api.isAdmin()) { <td><button class="btn" type="button" (click)="toggleUser(user)">{{ user.isActive === false ? 'Включить' : 'Выключить' }}</button> <button class="btn" type="button" (click)="resetPassword(user.id)">Сбросить пароль</button></td> }</tr> }</tbody>
        </table></div>
      }
      @default {
        <div class="page-head"><h1>Оргструктура</h1>@if (api.isAdmin()) { <button class="btn" type="button" (click)="addDirectorate()">+ Дирекция</button> <button class="btn" type="button" (click)="addProcess()">+ Процесс</button> <button class="btn" type="button" (click)="addObject()">+ Объект</button> }</div>
        <div class="table-wrap"><table><thead><tr><th>Процесс</th><th>Дирекция</th><th>Владелец</th></tr></thead>
          <tbody>@for (item of api.meta()?.processes; track item.id) { <tr><td>{{ item.name }}</td><td>{{ api.directorate(item.directorateId)?.name }}</td><td>{{ api.user(item.ownerId)?.name }}</td></tr> }</tbody>
        </table></div>
        <h2>Объекты</h2>
        <div class="table-wrap"><table><thead><tr><th>Объект</th><th>Стоимость часа простоя</th></tr></thead>
          <tbody>@for (item of api.meta()?.objects; track item.id) { <tr><td>{{ item.name }}</td><td>{{ api.money(item.hourly) }}</td>@if (api.isAdmin()) { <td><button class="btn" type="button" (click)="editHourly(item)">Стоимость часа</button></td> }</tr> }</tbody>
        </table></div>
      }
    }
    @if (dialog) { <app-ask [title]="dialog.title" [hint]="dialog.hint" [fields]="dialog.fields" [submitLabel]="dialog.submit" [lockedValues]="dialog.locked" (closed)="dialog = null" (saved)="applyAsk($event)" /> }
  `
})
export class SettingsPage implements OnInit {
  section = signal('org');
  impactFrom = '0, 100000, 250000, 500000, 1000000';
  scoreCaps = '10, 15, 19';
  costs = '0, 50000, 100000';
  applyActive = false;
  readonly read = readEventValue;
  dialog: { title: string; hint: string; fields: AskField[]; submit: string; locked: string[]; apply: (values: Record<string, string>) => void } | null = null;
  constructor(readonly api: SurApi, private readonly route: ActivatedRoute) {}
  ngOnInit() {
    this.route.fragment.subscribe((fragment) => this.section.set(fragment || 'org'));
  }
  applyAsk(values: Record<string, string>) {
    const current = this.dialog;
    this.dialog = null;
    current?.apply(values);
  }
  ask(title: string, fields: AskField[], apply: (values: Record<string, string>) => void, hint = '', locked: string[] = []) {
    this.dialog = { title, hint, fields, submit: 'Сохранить', locked, apply };
  }
  addHazard() {
    this.ask('Новая опасность', [{ name: 'name', label: 'Название', kind: 'text', required: true }], (values) => this.api.post('/api/hazards', { name: values['name'].trim() }));
  }
  addCategory() {
    this.ask('Новая категория', [{ name: 'name', label: 'Название', kind: 'text', required: true }], (values) => this.api.post('/api/categories', { name: values['name'].trim(), isActive: true }));
  }
  toggleHazard(id: string, isActive: boolean) { this.api.post(`/api/hazards/${id}/active`, { name: '', isActive }); }
  toggleCategory(item: Named) { this.api.put(`/api/categories/${item.id}`, { name: item.name, isActive: item.isActive === false }); }
  addDirectorate() {
    this.ask('Новая дирекция', [{ name: 'name', label: 'Название', kind: 'text', required: true }], (values) => this.api.post('/api/directorates', { name: values['name'].trim(), isActive: true }));
  }
  addProcess() {
    const directorates = (this.api.meta()?.directorates ?? []).map((item) => ({ value: item.id, label: item.name }));
    const owners = (this.api.meta()?.users ?? []).filter((user) => user.isActive !== false && user.roles.includes('OWNER')).map((user) => ({ value: user.id, label: user.name }));
    this.ask('Новый процесс', [
      { name: 'name', label: 'Название', kind: 'text', required: true },
      { name: 'directorateId', label: 'Дирекция', kind: 'select', required: true, options: directorates },
      { name: 'ownerId', label: 'Владелец', kind: 'select', required: true, options: owners }
    ], (values) => this.api.post('/api/processes', { name: values['name'].trim(), directorateId: values['directorateId'], ownerId: values['ownerId'], isActive: true }));
  }
  addObject() {
    this.ask('Новый объект', [
      { name: 'name', label: 'Название', kind: 'text', required: true },
      { name: 'hourly', label: 'Стоимость часа простоя, ₽', kind: 'number', value: '1000', required: true }
    ], (values) => this.api.post('/api/objects', { name: values['name'].trim(), hourly: Number(values['hourly']), isActive: true }));
  }
  editHourly(item: { id: string; name: string; hourly: number }) {
    this.ask('Стоимость часа простоя', [{ name: 'hourly', label: item.name, kind: 'number', value: String(item.hourly), required: true }], (values) => this.api.put(`/api/objects/${item.id}`, { name: item.name, hourly: Number(values['hourly']), isActive: true }));
  }
  addUser() {
    const roles = (this.api.meta()?.roles ?? []).filter((role) => role.isActive).map((role) => ({ value: role.code, label: role.name }));
    const directorates = (this.api.meta()?.directorates ?? []).map((item) => ({ value: item.id, label: item.name }));
    this.ask('Новый пользователь', [
      { name: 'name', label: 'Имя', kind: 'text', required: true },
      { name: 'login', label: 'Email', kind: 'text', required: true },
      { name: 'role', label: 'Роль', kind: 'select', required: true, options: roles },
      { name: 'directorateId', label: 'Дирекция', kind: 'select', required: true, options: directorates }
    ], (values) => this.api.post('/api/users', { name: values['name'].trim(), login: values['login'].trim(), roles: [values['role']], directorateId: values['directorateId'], isActive: true }));
  }
  toggleUser(user: User) {
    this.api.put(`/api/users/${user.id}`, { name: user.name, login: user.login, roles: user.roles, directorateId: user.directorateId, isActive: user.isActive === false });
  }
  resetPassword(id: string) {
    this.api.post(`/api/users/${id}/password-reset`);
  }
  editRole(role: { code: string; rights: string[]; isActive: boolean }) {
    const catalog = this.api.meta()?.rightCatalog ?? [];
    const options = catalog.map((code) => ({ value: code, label: code }));
    const locked = role.code === 'ADMIN' ? ['admin'] : [];
    this.ask(
      'Права роли',
      [{ name: 'rights', label: 'Права', kind: 'checks', value: role.rights.join(','), required: true, options }],
      (values) => {
        const rights = values['rights'].split(',').map((item) => item.trim()).filter(Boolean);
        if (role.code === 'ADMIN' && !rights.includes('admin')) rights.push('admin');
        this.api.put(`/api/roles/${role.code}`, { rights, isActive: role.isActive });
      },
      role.code === 'ADMIN' ? 'Право admin у роли ADMIN снять нельзя.' : 'Отметьте права роли.',
      locked
    );
  }
  saveScales() {
    const impactFrom = this.impactFrom.split(',').map((item) => Number(item.trim()));
    const caps = this.scoreCaps.split(',').map((item) => Number(item.trim()));
    const likelihood = this.api.meta()?.scales.likelihood ?? [];
    this.api.put('/api/scales', { likelihood, impactFrom, ownerMaxScore: caps[0], directorMaxScore: caps[1], ceoMaxScore: caps[2], applyToActive: this.applyActive });
  }
  saveCosts() {
    const costs = this.costs.split(',').map((item) => Number(item.trim()));
    this.api.put('/api/thresholds', { ownerCost: costs[0], directorCost: costs[1], ceoCost: costs[2] });
  }
}
