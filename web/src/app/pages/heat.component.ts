import { Component } from '@angular/core';
import { SurApi } from '../api.service';
import { heatClass } from '../fn/heatClass';
import { heatCount } from '../fn/heatCount';

@Component({
  selector: 'app-heat',
  template: `
    <div class="heat">
      <div></div>
      @for (col of [1,2,3,4,5]; track col) { <div class="heat-label">L{{ col }}</div> }
      @for (impact of [5,4,3,2,1]; track impact) {
        <div class="heat-label">I{{ impact }}</div>
        @for (likelihood of [1,2,3,4,5]; track likelihood) {
          <div class="heat-cell" [class]="cell(likelihood, impact)">{{ count(likelihood, impact) }}</div>
        }
      }
    </div>
  `
})
export class HeatComponent {
  constructor(private readonly api: SurApi) {}
  count(likelihood: number, impact: number) { return heatCount(this.api.risks(), likelihood, impact); }
  cell(likelihood: number, impact: number) { return heatClass(likelihood, impact); }
}
