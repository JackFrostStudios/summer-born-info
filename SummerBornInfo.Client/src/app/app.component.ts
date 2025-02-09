import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',

  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  title = 'SummerBornInfo.Client';
  group:
    | {
        id: string;
        code: number;
        name: string;
      }
    | undefined;

  constructor(
    private readonly httpClient: HttpClient,
    @Inject(DOCUMENT) private readonly document: Document
  ) {}

  async ngOnInit() {
    const api_host = this.document.location.host;
    const groupRequest$ = this.httpClient.get<any>(
      `http://${api_host}/api/establishment-group/0194e735-d21f-7583-ada9-ccc5c5e0e1dc`
    );
    this.group = await lastValueFrom(groupRequest$);
  }
}
