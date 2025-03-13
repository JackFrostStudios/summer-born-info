import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { CreateEstablishmentTypeRequest, EstablishmentType } from './establishment-type.model';

@Injectable({
  providedIn: 'root',
})
export class EstablishmentTypeService {
  constructor(private readonly httpClient: HttpClient) { }

  async createEstablishmentType(request: CreateEstablishmentTypeRequest) {
    const request$ = this.httpClient.post<EstablishmentType>('/api/establishment-type', request);
    return await lastValueFrom(request$);
  }
}
