import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { CreatePhaseOfEducationRequest, PhaseOfEducation } from './phase-of-education.model';

@Injectable({
  providedIn: 'root',
})
export class PhaseOfEducationService {
  constructor(private readonly httpClient: HttpClient) { }

  async createPhaseOfEducation(request: CreatePhaseOfEducationRequest) {
    const request$ = this.httpClient.post<PhaseOfEducation>('/api/phase-of-education', request);
    return await lastValueFrom(request$);
  }
}
