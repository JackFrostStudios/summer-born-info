import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BulkDataImportComponent } from './bulk-data-import.component';

describe('BulkDataImportComponent', () => {
  let component: BulkDataImportComponent;
  let fixture: ComponentFixture<BulkDataImportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BulkDataImportComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BulkDataImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
