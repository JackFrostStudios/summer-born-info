import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { CreateLocalAuthorityRequest, LocalAuthority } from './local-authority.model';

@Injectable({
  providedIn: 'root',
})
export class LocalAuthorityService {
  constructor(private readonly httpClient: HttpClient) { }

  async createLocalAuthority(request: CreateLocalAuthorityRequest) {
    const request$ = this.httpClient.post<LocalAuthority>('/api/local-authority', request);
    return await lastValueFrom(request$);
  }
}
