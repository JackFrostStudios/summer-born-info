import { TestBed } from '@angular/core/testing';

import { EstablishmentStatusService } from './establishment-status.service';

describe('EstablishmentStatusService', () => {
  let service: EstablishmentStatusService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EstablishmentStatusService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
