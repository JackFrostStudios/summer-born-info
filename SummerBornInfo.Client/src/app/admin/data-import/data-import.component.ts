import { Component } from '@angular/core';
import { ImportFileParsingService } from './file-import/parsing/import-file-parsing.service';

@Component({
  selector: 'sb-data-import',
  imports: [],
  templateUrl: './data-import.component.html',
  styleUrl: './data-import.component.css',
})
export class DataImportComponent {
  constructor(private readonly importFileParsingService: ImportFileParsingService) {}

  async fileSelected(event: Event) {
    const target = event?.currentTarget as HTMLInputElement;
    const file = target?.files?.[0];
    if (file) {
      await this.importFileParsingService.parseImportFile(file);
    }
  }
}
