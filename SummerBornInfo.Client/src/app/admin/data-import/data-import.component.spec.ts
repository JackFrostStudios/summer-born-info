import { ComponentFixture, TestBed } from '@angular/core/testing';

import { By } from '@angular/platform-browser';
import { getCsvFileWithValidationErrors, getInvalidCsvFile, getValidCsvFile } from '@test-helpers';
import { DataImportComponent } from './data-import.component';

describe('DataImportComponent', async () => {
  let component: DataImportComponent;
  let fixture: ComponentFixture<DataImportComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataImportComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DataImportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Given no file is selected', () => {
    beforeEach(() => {
      const fileInput = getFileInputEl();
      fileInput.dispatchEvent(new Event('change'));
      fixture.detectChanges();
    });

    it('Then the "unexpected error" message is displayed', () => {
      const messages = getDisplayedMessages();
      expect(messages.length).toBe(1);
      expect(messages[0].nativeElement.innerText).toBe('We ran into an unexpected error processing the file.');
    });
  });

  describe('Given a valid import file is selected', async () => {
    beforeEach(async () => {
      const fileInput = getFileInputEl();
      const selectedFile = getValidCsvFile();
      const fileList = new DataTransfer();
      fileList.items.add(selectedFile);
      fileInput.files = fileList.files;
      // We have to mock the event, for some reason NgZone / Await Stable isn't waiting for papaparse to resolve.
      const event = {
        currentTarget: fileInput,
      };
      await component.fileSelected(event as Event);
      fixture.detectChanges();
    });

    it('Then the "File processed successfully" message is displayed', async () => {
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const messages = getDisplayedMessages();
      expect(messages.length).toBe(1);
      expect(messages[0].nativeElement.innerText).toBe('File processed successfully.');
    });
  });

  describe('Given a file with validation errors is selected', async () => {
    beforeEach(async () => {
      const fileInput = getFileInputEl();
      const selectedFile = getCsvFileWithValidationErrors();
      const fileList = new DataTransfer();
      fileList.items.add(selectedFile);
      fileInput.files = fileList.files;
      // We have to mock the event, for some reason NgZone / Await Stable isn't waiting for papaparse to resolve.
      const event = {
        currentTarget: fileInput,
      };
      await component.fileSelected(event as Event);
      fixture.detectChanges();
    });

    it('Then the "File processed with errors" message is displayed', async () => {
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const messages = getDisplayedMessages();
      expect(messages.length).toBe(1);
      expect(messages[0].nativeElement.innerText).toBe('File processed with errors.');
    });
  });

  describe('Given a file in invalid format is selected', async () => {
    beforeEach(async () => {
      const fileInput = getFileInputEl();
      const selectedFile = getInvalidCsvFile();
      const fileList = new DataTransfer();
      fileList.items.add(selectedFile);
      fileInput.files = fileList.files;
      // We have to mock the event, for some reason NgZone / Await Stable isn't waiting for papaparse to resolve.
      const event = {
        currentTarget: fileInput,
      };
      await component.fileSelected(event as Event);
      fixture.detectChanges();
    });

    it('Then the "Unexpected Error" message is displayed', async () => {
      fixture.detectChanges();
      await fixture.whenStable();
      fixture.detectChanges();
      const messages = getDisplayedMessages();
      expect(messages.length).toBe(1);
      expect(messages[0].nativeElement.innerText).toBe('We ran into an unexpected error processing the file.');
    });
  });

  const getDisplayedMessages = () => {
    return fixture.debugElement.queryAll(By.css('p'));
  };

  const getFileInputEl = () => {
    return fixture.debugElement.query(By.css('input[type="file"]')).nativeElement;
  };
});
