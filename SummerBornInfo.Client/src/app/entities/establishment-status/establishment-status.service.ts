import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { CreateEstablishmentStatusRequest, EstablishmentStatus } from './establishment-status.model';

@Injectable({
  providedIn: 'root',
})
export class EstablishmentStatusService {
  constructor(private readonly httpClient: HttpClient) { }

  async createEstablishmentStatus(request: CreateEstablishmentStatusRequest) {
    const request$ = this.httpClient.post<EstablishmentStatus>('/api/establishment-status', request);
    return await lastValueFrom(request$);
  }
}
