import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CreateEstablishmentGroupRequest, EstablishmentGroup } from './establishment-group.model';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EstablishmentGroupService {
  constructor(private readonly httpClient: HttpClient) {}

  async createEstablishmentGroup(request: CreateEstablishmentGroupRequest) {
    const request$ = this.httpClient.post<EstablishmentGroup>('/api/establishment-group', request);
    return await lastValueFrom(request$);
  }
}
