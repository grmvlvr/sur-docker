import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SurApi } from '../api.service';
import { canAssess as allowAssess } from '../fn/canAssess';
import { canChooseMode } from '../fn/canChooseMode';
import { executorsOf } from '../fn/executorsOf';
import { modeLabel } from '../fn/modeLabel';
import { statusPill } from '../fn/statusPill';
import { taskLabel } from '../fn/taskLabel';
import { RiskCard, Treatment } from '../models';
import { AskField, AskFormComponent } from './ask-form.component';
import { AssessFormComponent } from './assess-form.component';
import { MoveFormComponent } from './move-form.component';
import { RiskFormComponent } from './risk-form.component';
import { TreatmentFormComponent } from './treatment-form.component';

@Component({
  selector: 'app-risk',
  imports: [RouterLink, FormsModule, RiskFormComponent, AssessFormComponent, TreatmentFormComponent, MoveFormComponent, AskFormComponent],
  template: `
    <a class="back" routerLink="/risks">← К реестру</a>
    @if (card(); as data) {
      <div class="detail-head"><div><h1>{{ data.risk.title }}</h1><div class="pills"><span class="pill" [class]="pill(data.risk.status)">{{ data.risk.status }}</span> <span>Id {{ data.risk.id }}</span></div></div></div>
      <div class="detail">
        <div>
          <div class="card"><div class="kv">
            <div><div class="k">Опасность</div><div>{{ api.hazard(data.risk.hazardId)?.name }}</div></div>
            <div><div class="k">Основной процесс</div><div>{{ api.process(api.primary(data.risk).processId)?.name }}</div></div>
            <div><div class="k">Владелец</div><div>{{ api.user(api.primary(data.risk).ownerId)?.name }}</div></div>
            <div><div class="k">Объект</div><div>{{ api.object(data.risk.objectId)?.name || 'Не связан' }}</div></div>
            <div><div class="k">Вероятность</div><div>{{ data.risk.assessment ? data.risk.assessment.likelihood : 'Нет оценки' }}</div></div>
            <div><div class="k">R и уровень</div><div>{{ data.risk.assessment?.score ?? '—' }} · {{ api.level(data.risk.assessment?.level) }}</div></div>
            <div><div class="k">Последствия</div><div>{{ data.risk.assessment ? api.money(data.risk.assessment.lossAmount) : '—' }}</div></div>
            <div><div class="k">Режим цикла</div><div>{{ mode(data.risk.assessment?.responseMode) }}</div></div>
            <div style="grid-column:1/-1"><div class="k">Описание</div><div>{{ data.risk.description }}</div></div>
          </div>
          <div class="actions" style="margin-top:16px">
            @if (data.risk.status === 'Черновик' && api.isPrimaryOwner(data.risk)) { <button class="btn" type="button" (click)="edit = true">Изменить паспорт</button> }
            @if (canAssess(data)) { <button class="btn primary" type="button" (click)="assess = true">Оценить риск</button> }
            @if (canMode(data)) {
              <button class="btn" type="button" (click)="run('/api/risks/' + data.risk.id + '/mode', { mode: 'treat' })">Обработать</button>
              <button class="btn" type="button" (click)="split = true">Разделить</button>
              <button class="btn" type="button" (click)="transfer = true">Передать</button>
              <button class="btn primary" type="button" (click)="run('/api/risks/' + data.risk.id + '/mode', { mode: 'accept' })">Принять</button>
            }
            @if (api.isPrimaryOwner(data.risk) && data.risk.status !== 'Закрыт') { <button class="btn" type="button" (click)="closeRisk(data.risk.id)">Закрыть</button> }
            <button class="btn" type="button" (click)="snapshot(data.risk.id)">Сформировать Audit Snapshot</button>
            @if (api.isPrimaryOwner(data.risk) && (data.risk.status === 'Принят' || data.risk.status === 'Закрыт')) { <button class="btn" type="button" (click)="run('/api/risks/' + data.risk.id + '/reopen')">На переоценку</button> }
            @if (data.risk.status === 'Активен' && (data.risk.assessment?.responseMode === 'treat' || data.risk.assessment?.responseMode === 'split')) { <button class="btn primary" type="button" (click)="treatment = true">+ Мера</button> }
          </div></div>
          <div class="tabs-line">
            <button type="button" [class.active]="tab === 'tasks'" (click)="tab = 'tasks'">Меры</button>
            <button type="button" [class.active]="tab === 'comments'" (click)="tab = 'comments'">Комментарии</button>
            <button type="button" [class.active]="tab === 'log'" (click)="tab = 'log'">Журнал</button>
          </div>
          @if (tab === 'tasks') {
            @for (item of data.treatments; track item.id) {
              <article class="risk-card">
                <div class="row-between"><span class="pill" [class]="pill(item.status)">{{ item.status }}</span><span>{{ task(item) }}</span></div>
                <h3>{{ item.title }}</h3>
                <p class="sub">{{ item.description }}</p>
                <p>План {{ api.money(item.costPlanned) }} · факт {{ item.costActual == null ? '—' : api.money(item.costActual) }} · {{ api.user(item.executorId)?.name || 'исполнитель не назначен' }}</p>
                @if (item.jira || item.taskProvider || item.taskError) {
                  <p class="sub">
                    Внешняя задача
                    · режим {{ item.taskProvider || 'MOCK' }}
                    · ключ {{ item.jira || 'не создан' }}
                    · статус {{ item.taskStatus || 'нет' }}
                    · ссылка {{ realLink(item.taskUrl) || 'не предоставлена' }}
                  </p>
                }
                @if (item.taskError) { <p class="sub" style="color:#b42318">{{ item.taskError }}</p> }
                @if (!item.jira && (item.status === 'Ожидает выполнения' || item.status === 'В работе')) {
                  <button class="btn" type="button" (click)="run('/api/treatments/' + item.id + '/mock', { failMock: false })">Повторить создание задачи</button>
                }
                <div class="actions">
                  @if (item.status === 'Черновик') {
                    <button class="btn" type="button" (click)="editTreatment(item)">Изменить</button>
                    <button class="btn" type="button" (click)="run('/api/treatments/' + item.id + '/approval')">На согласование</button>
                    <button class="btn" type="button" (click)="cancelTreatment(item.id)">Отменить черновик</button>
                  }
                  @if (item.status === 'На согласовании') {
                    <button class="btn primary" type="button" (click)="decide(item.id, true)">Согласовать</button>
                    <button class="btn" type="button" (click)="decide(item.id, false)">Отклонить</button>
                  }
                  @if (item.status === 'Согласовано') { <button class="btn primary" type="button" (click)="start(item.id, item.processId)">Назначить исполнителя</button> }
                  @if (item.status === 'Ожидает выполнения') { <button class="btn" type="button" (click)="run('/api/treatments/' + item.id + '/work')">В работу</button> }
                  @if (item.status === 'Ожидает выполнения' || item.status === 'В работе' || item.status === 'Холд') { <button class="btn" type="button" (click)="replaceExecutor(item.id, item.processId)">Заменить исполнителя</button> }
                  @if (item.status === 'В работе') {
                    <button class="btn" type="button" (click)="jira(item.id)">Завершить</button>
                    <button class="btn" type="button" (click)="run('/api/treatments/' + item.id + '/hold')">Холд</button>
                  }
                  @if (item.status === 'Холд') {
                    <button class="btn" type="button" (click)="run('/api/treatments/' + item.id + '/resume')">Вернуть в работу</button>
                    <button class="btn" type="button" (click)="run('/api/treatments/' + item.id + '/executor/clear')">Снять исполнителя</button>
                  }
                  @if (api.isAdmin() && item.status !== 'Готово' && item.status !== 'Отменено' && item.status !== 'Черновик') { <button class="btn" type="button" (click)="cancelTreatment(item.id)">Отменить</button> }
                </div>
              </article>
            } @empty { <div class="card empty">Мер пока нет.</div> }
          }
          @if (tab === 'comments') {
            <div class="card">
              @for (item of data.comments; track item.id) {
                <p><b>{{ item.isSystem ? 'Система' : api.user(item.authorId)?.name }}</b> · {{ item.at }}<br>{{ item.text }}
                  @if (!item.isSystem) {
                    <button class="btn" type="button" (click)="editComment(item.id, item.text)">Изменить</button>
                    <button class="btn" type="button" (click)="run('/api/comments/' + item.id + '/delete')">Удалить</button>
                  }
                </p>
              }
              <div class="field"><textarea [(ngModel)]="comment"></textarea></div>
              <button class="btn primary" type="button" (click)="sendComment(data.risk.id)">Отправить</button>
            </div>
          }
          @if (tab === 'log') { <div class="card">@for (item of data.log; track item.id) { <p>{{ item.at }} · {{ item.author }} · {{ item.text }}</p> }</div> }
        </div>
        <aside class="side-card"><h3 style="padding:8px">Связанные процессы</h3>
          @for (item of data.risk.processes; track item.processId) { <div class="measure">{{ item.isPrimary ? 'Основной: ' : '' }}{{ api.process(item.processId)?.name }}</div> }
        </aside>
      </div>
      @if (edit) { <app-risk-form [riskId]="data.risk.id" [source]="data.risk" (closed)="edit = false; load(data.risk.id)" /> }
      @if (assess) { <app-assess [risk]="data.risk" (closed)="assess = false; load(data.risk.id)" /> }
      @if (treatment) { <app-treatment [risk]="data.risk" (closed)="treatment = false; load(data.risk.id)" /> }
      @if (transfer || split) { <app-move [risk]="data.risk" [splitMode]="split" (closed)="transfer = false; split = false; load(data.risk.id)" /> }
    }
    @if (files) {
      <div class="card">
        <p>Снимок <b>{{ files.snapshotId }}</b> готов к скачиванию.</p>
        <div class="actions">
          <a class="btn primary" [href]="files.pdf" [download]="files.snapshotId + '.pdf'">Скачать PDF</a>
          <a class="btn" [href]="files.csv" [download]="files.snapshotId + '.csv'">Скачать CSV</a>
        </div>
      </div>
    }
    @if (dialog) { <app-ask [title]="dialog.title" [hint]="dialog.hint" [fields]="dialog.fields" [submitLabel]="dialog.submit" (closed)="dialog = null" (saved)="applyAsk($event)" /> }
  `
})
export class RiskPage implements OnInit {
  card = signal<RiskCard | null>(null);
  tab = 'tasks';
  edit = false;
  assess = false;
  treatment = false;
  transfer = false;
  split = false;
  comment = '';
  files: { snapshotId: string; csv: string; pdf: string } | null = null;
  dialog: { title: string; hint: string; fields: AskField[]; submit: string; apply: (values: Record<string, string>) => void } | null = null;

  constructor(readonly api: SurApi, private readonly route: ActivatedRoute) {}

  ngOnInit() {
    this.route.paramMap.subscribe((params) => this.load(params.get('id')!));
  }

  load(id: string) {
    this.api.card(id).subscribe({
      next: (card) => this.card.set(card),
      error: (err) => this.api.fail(err)
    });
  }

  run(url: string, body?: unknown) {
    this.api.httpPost(url, body, () => this.load(this.card()!.risk.id));
  }

  sendComment(id: string) {
    const text = this.comment.trim();
    if (!text) return;
    this.api.httpPost(`/api/risks/${id}/comments`, { text }, () => {
      this.comment = '';
      this.load(id);
    });
  }

  canAssess(data: RiskCard) { return allowAssess(this.api.meta()?.me, data); }

  canMode(data: RiskCard) { return canChooseMode(this.api.meta()?.me, data); }

  applyAsk(values: Record<string, string>) {
    const current = this.dialog;
    this.dialog = null;
    current?.apply(values);
  }

  start(id: string, processId: string) {
    const people = executorsOf(this.api.meta()?.users ?? [], this.api.process(processId)?.directorateId).filter((person) => person.isActive !== false);
    this.dialog = {
      title: 'Назначить исполнителя', submit: 'Назначить',
      hint: 'После назначения создаётся задача во внешней системе трекинга. В MVP используется демонстрационный адаптер MOCK: реальная Jira не вызывается, ссылка не выдумывается.',
      fields: [
        { name: 'executorId', label: 'Исполнитель', kind: 'select', required: true, options: people.map((person) => ({ value: person.id, label: `${person.name} (${person.login})` })) },
        { name: 'mock', label: 'Создание внешней задачи', kind: 'select', value: 'ok', required: true, options: [{ value: 'ok', label: 'Создать задачу' }, { value: 'fail', label: 'Имитировать ошибку создания' }] }
      ],
      apply: (values) => this.run(`/api/treatments/${id}/start`, { executorId: values['executorId'], failMock: values['mock'] === 'fail' })
    };
  }

  snapshot(id: string) {
    this.api.createSnapshot(id).subscribe({
      next: (file) => this.files = file,
      error: (err) => this.api.fail(err)
    });
  }

  realLink(url?: string | null) { return url && !url.startsWith('#mock') ? url : ''; }

  decide(id: string, approve: boolean) {
    this.dialog = {
      title: approve ? 'Согласовать меру' : 'Отклонить меру',
      hint: approve ? 'Комментарий необязателен.' : 'Укажите причину отклонения.',
      submit: approve ? 'Согласовать' : 'Отклонить',
      fields: [{ name: 'comment', label: approve ? 'Комментарий' : 'Причина', kind: 'textarea', required: !approve }],
      apply: (values) => this.run(`/api/treatments/${id}/decision`, { approve, comment: values['comment'].trim() })
    };
  }

  closeRisk(id: string) {
    this.dialog = { title: 'Закрыть риск', hint: 'Укажите причину. Пустое поле не отправится.', fields: [{ name: 'reason', label: 'Причина', kind: 'textarea', required: true }], submit: 'Закрыть', apply: (values) => this.run(`/api/risks/${id}/close`, { reason: values['reason'].trim() }) };
  }

  jira(id: string) {
    this.guardDrafts(id, () => {
      this.dialog = { title: 'Завершить меру', hint: 'Фактическая стоимость не может быть отрицательной.', fields: [{ name: 'cost', label: 'Фактическая стоимость, ₽', kind: 'number', value: '0', required: true }], submit: 'Завершить', apply: (values) => this.run(`/api/treatments/${id}/jira-done`, { costActual: Number(values['cost']) }) };
    });
  }

  cancelTreatment(id: string) {
    this.guardDrafts(id, () => this.run(`/api/treatments/${id}/cancel`));
  }

  guardDrafts(id: string, next: () => void) {
    const items = this.card()?.treatments ?? [];
    const open = ['На согласовании', 'Согласовано', 'Ожидает выполнения', 'В работе', 'Холд'];
    const others = items.filter((item) => item.id !== id && open.includes(item.status)).length;
    const drafts = items.some((item) => item.status === 'Черновик');
    if (others === 0 && drafts) {
      this.dialog = { title: 'Остались черновики мер', hint: 'После завершения текущей меры риск перейдёт в «Ожидает переоценки», и черновики нельзя будет активировать до новой оценки.', fields: [], submit: 'Продолжить', apply: () => next() };
      return;
    }
    next();
  }

  editComment(id: string, text: string) {
    this.dialog = { title: 'Изменить комментарий', hint: '', fields: [{ name: 'text', label: 'Комментарий', kind: 'textarea', value: text, required: true }], submit: 'Сохранить', apply: (values) => this.run(`/api/comments/${id}`, { text: values['text'].trim() }) };
  }

  editTreatment(item: Treatment) {
    const processes = (this.card()?.risk.processes ?? []).filter((link) => !link.unlinkedAt);
    this.dialog = {
      title: 'Изменить меру', hint: 'Черновик можно править, пока он не ушёл на согласование.', submit: 'Сохранить',
      fields: [
        { name: 'title', label: 'Название', kind: 'text', value: item.title, required: true },
        { name: 'processId', label: 'Процесс', kind: 'select', value: item.processId, required: true, options: processes.map((link) => ({ value: link.processId, label: this.api.process(link.processId)?.name ?? link.processId })) },
        { name: 'cost', label: 'Плановая стоимость, ₽', kind: 'number', value: String(item.costPlanned), required: true },
        { name: 'due', label: 'Срок', kind: 'text', value: item.due ?? '', required: true },
        { name: 'description', label: 'Описание', kind: 'textarea', value: item.description }
      ],
      apply: (values) => this.api.put(`/api/treatments/${item.id}`, { title: values['title'].trim(), processId: values['processId'], costPlanned: Number(values['cost']), due: values['due'].trim(), description: values['description'] }, () => this.load(this.card()!.risk.id))
    };
  }

  replaceExecutor(id: string, processId: string) {
    this.pickExecutor('Заменить исполнителя', 'Заменить', processId, (executorId) => this.run(`/api/treatments/${id}/executor`, { executorId }));
  }

  pickExecutor(title: string, submit: string, processId: string, apply: (executorId: string) => void) {
    const people = executorsOf(this.api.meta()?.users ?? [], this.api.process(processId)?.directorateId).filter((person) => person.isActive !== false);
    this.dialog = {
      title, submit, hint: people.length ? 'В списке активные владельцы и линейные сотрудники дирекции процесса.' : 'В дирекции процесса нет активного владельца или линейного сотрудника.',
      fields: [{ name: 'executorId', label: 'Исполнитель', kind: 'select', required: true, options: people.map((person) => ({ value: person.id, label: `${person.name} (${person.login})` })) }],
      apply: (values) => apply(values['executorId'])
    };
  }

  pill(status: string) { return statusPill(status); }
  mode(value?: string | null) { return modeLabel(value, 'Не выбран'); }
  task(item: Treatment) { return taskLabel(item); }
}
