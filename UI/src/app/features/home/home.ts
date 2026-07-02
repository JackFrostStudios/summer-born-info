import { Component } from '@angular/core';
import { HomeHero } from './home-hero/home-hero';

@Component({
  selector: 'sbi-home',
  imports: [HomeHero],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {}
