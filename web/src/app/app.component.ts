import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SurApi } from './api.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, FormsModule],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  collapsed = false;
  settingsOpen = true;
  loginName = '';
  password = '';

  constructor(readonly api: SurApi) {}

  ngOnInit() {
    this.api.load().subscribe({
      error: () => this.api.authenticated.set(false)
    });
  }

  enter() {
    this.api.login(this.loginName, this.password).subscribe({
      next: () => {
        this.password = '';
        this.api.clearNotice();
        this.api.load().subscribe({ error: (err) => this.api.fail(err) });
      },
      error: (err) => this.api.fail(err)
    });
  }

  avatarLetter() {
    const login = this.api.meta()?.me?.login?.trim() ?? '';
    return (login[0] || '?').toUpperCase();
  }
}
