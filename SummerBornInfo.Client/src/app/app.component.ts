import { Component } from '@angular/core';
import { BulkDataImportComponent } from './admin/bulk-data-import/bulk-data-import.component';


@Component({
  selector: 'sb-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  imports: [BulkDataImportComponent],
})
export class AppComponent {
  title = 'SummerBorn Info';
}
