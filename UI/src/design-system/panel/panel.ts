import { Component, input } from '@angular/core';

export type PanelMediaWidth = 'default' | 'compact';

@Component({
  selector: 'sbi-panel',
  templateUrl: './panel.html',
  styleUrl: './panel.scss',
})
export class Panel {
  readonly $mediaWidth = input<PanelMediaWidth>('default');
}
