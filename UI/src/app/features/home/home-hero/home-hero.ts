import { NgOptimizedImage } from '@angular/common';
import { Component, input } from '@angular/core';

@Component({
  selector: 'sbi-home-hero',
  imports: [NgOptimizedImage],
  templateUrl: './home-hero.html',
  styleUrl: './home-hero.scss',
})
export class HomeHero {
  readonly headingId = input.required<string>();

  protected get heroHeadingId(): string {
    return this.headingId();
  }
}
