import { Injectable } from '@angular/core';
import { parse } from 'papaparse';
import { ImportFileCsvRow } from '../import-file-csv-row.model';
import { ImportFileResultBuilder } from './import-file-result-builder';
import { ImportFileResult } from './import-file-result.model';

@Injectable({
  providedIn: 'root',
})
export class ImportFileParsingService {
  parseImportFile(file: File) {
    return new Promise<ImportFileResult>((resolve, error) => {
      const result = new ImportFileResultBuilder();
      parse<ImportFileCsvRow>(file, {
        dynamicTyping: false,
        header: true,
        skipEmptyLines: 'greedy',
        complete: fileResult => {
          if (fileResult.errors && fileResult.errors.length > 0) {
            error(fileResult.errors);
          }
          fileResult.data.forEach(record => {
            //TODO: Validate record before processing & include errors in result.
            result
              .AddLocalAuthority({
                code: record['LA (code)'],
                name: record['LA (name)'],
              })
              .AddEstablishmentType({
                code: record['TypeOfEstablishment (code)'],
                name: record['TypeOfEstablishment (name)'],
              })
              .AddEstablishmentGroup({
                code: record['EstablishmentTypeGroup (code)'],
                name: record['EstablishmentTypeGroup (name)'],
              })
              .AddEstablishmentStatus({
                code: record['EstablishmentStatus (code)'],
                name: record['EstablishmentStatus (name)'],
              })
              .AddPhaseOfEducation({
                code: record['PhaseOfEducation (code)'],
                name: record['PhaseOfEducation (name)'],
              })
              .AddSchool({
                urn: Number.parseInt(record.URN),
                ukprn: Number.parseInt(record.UKPRN),
                name: record.EstablishmentName,
                address: {
                  street: record.Street,
                  locality: record.Locality,
                  addressThree: record.Address3,
                  town: record.Town,
                  county: record['County (name)'],
                  postCode: record.PostCode,
                },
                establishmentNumber: Number.parseInt(record.EstablishmentNumber),
                openDate: record.OpenDate !== '' ? new Date(record.OpenDate) : null,
                closeDate: record.CloseDate !== '' ? new Date(record.CloseDate) : null,
                localAuthorityCode: record['LA (code)'],
                establishmentTypeCode: record['TypeOfEstablishment (code)'],
                establishmentGroupCode: record['EstablishmentTypeGroup (code)'],
                establishmentStatusCode: record['EstablishmentStatus (code)'],
                phaseOfEducationCode: record['PhaseOfEducation (code)'],
              });
          });

          resolve(result.GetResults());
        },
      });
    });
  }
}
