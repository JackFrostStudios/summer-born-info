import { Component } from '@angular/core';

@Component({
  selector: 'sbi-home',
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  protected readonly headingId = 'home-heading';
  protected readonly topicsHeadingId = 'home-topics-heading';
  protected readonly comingSoonHeadingId = 'home-coming-soon-heading';
  protected readonly noteHeadingId = 'home-note-heading';
}
