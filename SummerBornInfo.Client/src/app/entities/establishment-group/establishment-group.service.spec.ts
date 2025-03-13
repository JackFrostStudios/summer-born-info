import { TestBed } from '@angular/core/testing';

import { EstablishmentGroupService } from './establishment-group.service';

describe('EstablishmentGroupService', () => {
  let service: EstablishmentGroupService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EstablishmentGroupService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
