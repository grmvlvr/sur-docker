import { Routes } from '@angular/router';
import { ApprovalsPage } from './pages/approvals.page';
import { ArchivePage } from './pages/archive.page';
import { DashboardPage } from './pages/dashboard.page';
import { ReportsPage } from './pages/reports.page';
import { RiskPage } from './pages/risk.page';
import { RisksPage } from './pages/risks.page';
import { SettingsPage } from './pages/settings.page';
import { HelpPage } from './pages/help.page';
import { TopPage } from './pages/top.page';

export const routes: Routes = [
  { path: '', component: DashboardPage },
  { path: 'risks', component: RisksPage },
  { path: 'top', component: TopPage },
  { path: 'reports', component: ReportsPage },
  { path: 'archive', component: ArchivePage },
  { path: 'approvals', component: ApprovalsPage },
  { path: 'help', component: HelpPage },
  { path: 'settings', component: SettingsPage },
  { path: 'risk/:id', component: RiskPage }
];
