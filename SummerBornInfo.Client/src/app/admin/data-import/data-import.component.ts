import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ImportFileParsingService } from './file-import/parsing/import-file-parsing.service';
import { ImportFileResult } from './file-import/parsing/import-file-result.model';

@Component({
  selector: 'sb-data-import',
  imports: [CommonModule],
  templateUrl: './data-import.component.html',
  styleUrl: './data-import.component.css',
})
export class DataImportComponent {
  constructor(private readonly importFileParsingService: ImportFileParsingService) {}
  fileProcessedSuccessfully = false;
  fileProcessedWithErrors = false;
  unexpectedError = false;

  async fileSelected(event: Event) {
    this.resetStatus();
    const target = event?.currentTarget as HTMLInputElement;
    const file = target?.files?.[0];

    if (!file) {
      console.error('No files could be found.');
      this.unexpectedError = true;
      return;
    }

    try {
      const result = await this.importFileParsingService.parseImportFile(file);
      this.processResult(result);
    } catch (e) {
      console.error(e);
      this.unexpectedError = true;
    }
  }

  private processResult(result: ImportFileResult) {
    if (result.errors.length > 0) {
      this.fileProcessedWithErrors = true;
      return;
    }

    this.fileProcessedSuccessfully = true;
  }

  private resetStatus() {
    this.fileProcessedSuccessfully = false;
    this.fileProcessedWithErrors = false;
    this.unexpectedError = false;
  }
}
