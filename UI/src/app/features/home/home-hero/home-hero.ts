import { NgOptimizedImage } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from '../../../../design-system/button/button';

@Component({
  selector: 'sbi-home-hero',
  imports: [NgOptimizedImage, Button],
  templateUrl: './home-hero.html',
  styleUrl: './home-hero.scss',
})
export class HomeHero {
  private readonly router = inject(Router);

  protected goToUnderConstruction(): void {
    void this.router.navigateByUrl('/under-construction');
  }
}
