import { Component } from '@angular/core';
import { DataImportComponent } from './admin/data-import/data-import.component';

@Component({
  selector: 'sb-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  imports: [DataImportComponent],
})
export class AppComponent {
  title = 'SummerBorn Info';
}
