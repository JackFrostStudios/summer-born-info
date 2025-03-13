import { TestBed } from '@angular/core/testing';

import { EstablishmentTypeService } from './establishment-type.service';

describe('EstablishmentTypeService', () => {
  let service: EstablishmentTypeService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EstablishmentTypeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
