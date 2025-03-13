import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { CreateSchoolRequest, School } from './school.model';

@Injectable({
  providedIn: 'root',
})
export class SchoolService {
  constructor(private readonly httpClient: HttpClient) { }

  async createSchool(request: CreateSchoolRequest) {
    const request$ = this.httpClient.post<School>('/api/school', request);
    return await lastValueFrom(request$);
  }
}
