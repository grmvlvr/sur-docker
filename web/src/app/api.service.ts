import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { forkJoin, tap } from 'rxjs';
import { apiErrorMessage } from './fn/apiErrorMessage';
import { findById } from './fn/findById';
import { formatMoney } from './fn/formatMoney';
import { hasRight } from './fn/hasRight';
import { hasRole } from './fn/hasRole';
import { isAdmin } from './fn/isAdmin';
import { isPrimaryOwner } from './fn/isPrimaryOwner';
import { levelName } from './fn/levelName';
import { primaryLink } from './fn/primaryLink';
import { Meta, Risk, RiskCard, Treatment } from './models';

@Injectable({ providedIn: 'root' })
export class SurApi {
  readonly meta = signal<Meta | null>(null);
  readonly risks = signal<Risk[]>([]);
  readonly treatments = signal<Treatment[]>([]);
  readonly notice = signal('');
  readonly authenticated = signal(false);

  constructor(private readonly http: HttpClient) {}

  load() {
    return forkJoin({
      meta: this.http.get<Meta>('/api/meta'),
      risks: this.http.get<Risk[]>('/api/risks'),
      treatments: this.http.get<Treatment[]>('/api/treatments')
    }).pipe(tap(({ meta, risks, treatments }) => {
      this.meta.set(meta);
      this.risks.set(risks);
      this.treatments.set(treatments);
      this.authenticated.set(true);
    }));
  }

  login(login: string, password: string) {
    return this.http.post('/api/login', { login, password });
  }

  logout() {
    this.http.post('/api/logout', {}).subscribe({
      next: () => {
        this.authenticated.set(false);
        this.meta.set(null);
        this.risks.set([]);
        this.treatments.set([]);
        this.notice.set('');
      },
      error: (err: unknown) => this.fail(err)
    });
  }

  refresh() {
    this.load().subscribe({ error: (err: unknown) => this.fail(err) });
  }

  card(id: string) {
    return this.http.get<RiskCard>(`/api/risks/${id}`);
  }

  importRisks(csv: string) {
    return this.http.post<{ ok: boolean; created: number; skipped: number; errors: { line: number; field: string; message: string }[] }>('/api/import/risks', { csv });
  }

  createSnapshot(riskId: string) {
    return this.http.post<{ ok: boolean; snapshotId: string; csv: string; pdf: string }>(`/api/risks/${riskId}/snapshot`, {});
  }

  get<T>(url: string) {
    return this.http.get<T>(url);
  }

  post(url: string, body?: unknown) {
    this.httpPost(url, body);
  }

  httpPost(url: string, body?: unknown, done?: () => void) {
    this.http.post<{ message?: string }>(url, body ?? {}).subscribe({
      next: (result) => {
        if (result?.message) this.notice.set(result.message);
        this.refresh();
        done?.();
      },
      error: (err: unknown) => this.fail(err)
    });
  }

  put(url: string, body: unknown, done?: () => void) {
    this.http.put<{ message?: string }>(url, body).subscribe({
      next: (result) => {
        if (result?.message) this.notice.set(result.message);
        this.refresh();
        done?.();
      },
      error: (err: unknown) => this.fail(err)
    });
  }

  fail(err: unknown) {
    this.notice.set(apiErrorMessage(err));
  }

  clearNotice() { this.notice.set(''); }

  process(id: string) { return findById(this.meta()?.processes ?? [], id); }
  user(id?: string | null) { return findById(this.meta()?.users ?? [], id); }
  hazard(id: string) { return findById(this.meta()?.hazards ?? [], id); }
  category(id?: string | null) { return findById(this.meta()?.categories ?? [], id); }
  object(id?: string | null) { return findById(this.meta()?.objects ?? [], id); }
  directorate(id?: string | null) { return findById(this.meta()?.directorates ?? [], id); }
  primary(risk: Risk) { return primaryLink(risk); }
  money(value?: number | null) { return formatMoney(value); }
  level(code?: string | null) { return levelName(this.meta()?.scales.levels, code); }
  isAdmin() { return isAdmin(this.meta()?.me); }
  hasRole(role: string) { return hasRole(this.meta()?.me, role); }
  hasRight(right: string) { return hasRight(this.meta()?.me, right); }
  isPrimaryOwner(risk: Risk) { return isPrimaryOwner(this.meta()?.me, risk); }
}
