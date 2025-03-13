import { TestBed } from '@angular/core/testing';

import { LocalAuthorityService } from './local-authority.service';

describe('LocalAuthorityService', () => {
  let service: LocalAuthorityService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LocalAuthorityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
