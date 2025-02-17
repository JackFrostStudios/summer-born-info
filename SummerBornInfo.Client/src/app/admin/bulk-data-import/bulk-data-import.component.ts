import { Component } from '@angular/core';
import Papa from 'papaparse';
import { BulkDataRecord } from './bulk-data-record';

@Component({
  selector: 'sb-bulk-data-import',
  imports: [],
  templateUrl: './bulk-data-import.component.html',
  styleUrl: './bulk-data-import.component.css',
})
export class BulkDataImportComponent {
  async fileSelected(event: Event) {
    const target = event?.currentTarget as HTMLInputElement;
    const file = target?.files?.[0];
    if (file) {
      const localAuthorities: Record<string, { code: string; name: string }> = {};
      const establishmentTypes: Record<string, { code: string; name: string }> = {};
      const establishmentTypeGroups: Record<string, { code: string; name: string }> = {};
      const establishmentStatuses: Record<string, { code: string; name: string }> = {};
      const phasesOfEducation: Record<string, { code: string; name: string }> = {};
      const schools = [];
      Papa.parse<BulkDataRecord>(file, {
        dynamicTyping: true,
        header: true,
        complete: parsingResult => {
          parsingResult.data.forEach(record => {
            if (localAuthorities[record['LA (code)']] === undefined) {
              localAuthorities[record['LA (code)']] = {
                code: record['LA (code)'],
                name: record['LA (name)'],
              };
            }
            if (establishmentTypes[record['TypeOfEstablishment (code)']] === undefined) {
              establishmentTypes[record['TypeOfEstablishment (code)']] = {
                code: record['TypeOfEstablishment (code)'],
                name: record['TypeOfEstablishment (name)'],
              };
            }
            if (establishmentTypeGroups[record['EstablishmentTypeGroup (code)']] === undefined) {
              establishmentTypeGroups[record['EstablishmentTypeGroup (code)']] = {
                code: record['EstablishmentTypeGroup (code)'],
                name: record['EstablishmentTypeGroup (name)'],
              };
            }
            if (establishmentStatuses[record['EstablishmentStatus (code)']] === undefined) {
              establishmentStatuses[record['EstablishmentStatus (code)']] = {
                code: record['EstablishmentStatus (code)'],
                name: record['EstablishmentStatus (name)'],
              };
            }

            if (phasesOfEducation[record['PhaseOfEducation (code)']] === undefined) {
              phasesOfEducation[record['PhaseOfEducation (code)']] = {
                code: record['PhaseOfEducation (code)'],
                name: record['PhaseOfEducation (name)'],
              };
            }

            schools.push({
              URN: record.URN,
              establishmentNumber: record.EstablishmentNumber,
              openDate: record.OpenDate,
              closeDate: record.CloseDate,
              localAuthorityCode: record['LA (code)'],
              establishmentTypeCode: record['TypeOfEstablishment (code)'],
              establishmentTypeGroupCode: record['EstablishmentTypeGroup (code)'],
              establishmentStatusCode: record['EstablishmentStatus (code)'],
              phasOfEducationCode: record['PhaseOfEducation (code)']
            });
          });

          console.log(`localAuthorities: ${Object.values(localAuthorities).length}`);
          console.log(`establishmentTypes: ${Object.values(establishmentTypes).length}`);
          console.log(`establishmentTypeGroups: ${Object.values(establishmentTypeGroups).length}`);
          console.log(`establishmentStatuses: ${Object.values(establishmentStatuses).length}`);
          console.log(`phasesOfEducation: ${Object.values(phasesOfEducation).length}`);
          console.log(`schools: ${schools.length}`);
        },
      });
    }
  }
}
