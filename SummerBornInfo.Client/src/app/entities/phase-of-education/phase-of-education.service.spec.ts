import { TestBed } from '@angular/core/testing';

import { PhaseOfEducationService } from './phase-of-education.service';

describe('PhaseOfEducationService', () => {
  let service: PhaseOfEducationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PhaseOfEducationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
