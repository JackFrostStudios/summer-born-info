import { Component } from '@angular/core';
import { HomeHero } from './home-hero/home-hero';

@Component({
  selector: 'sbi-home',
  imports: [HomeHero],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  protected readonly headingId = 'home-heading';
  protected readonly topicsHeadingId = 'home-topics-heading';
  protected readonly comingSoonHeadingId = 'home-coming-soon-heading';
  protected readonly noteHeadingId = 'home-note-heading';
}
