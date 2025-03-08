import { Component } from '@angular/core';
import { DataImportComponent } from '@admin';

@Component({
  selector: 'sb-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  imports: [DataImportComponent],
})
export class AppComponent {
  title = 'SummerBorn Info';
}
