import { NgOptimizedImage } from '@angular/common';
import { Component } from '@angular/core';
import { Button } from '../../../../design-system/button/button';

@Component({
  selector: 'sbi-home-hero',
  imports: [NgOptimizedImage, Button],
  templateUrl: './home-hero.html',
  styleUrl: './home-hero.scss',
})
export class HomeHero {}
