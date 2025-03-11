import { Injectable } from '@angular/core';
import { parse } from 'papaparse';
import { ImportFileCsvRow } from '../import-file-csv-row.model';
import { ImportFileResultBuilder } from './import-file-result-builder';
import { ImportFileResult } from './import-file-result.model';
import { ImportFileValidatorService } from '../validation/import-file-validator.service';

@Injectable({
  providedIn: 'root',
})
export class ImportFileParsingService {
  constructor(private readonly importFileValidatorService: ImportFileValidatorService) {}

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
          fileResult.data.forEach((record, i) => {
            const validationErrors = this.importFileValidatorService.validateRow(record);
            if (validationErrors.length > 0) {
              result.AddError({
                rowNumber: i + 1,
                errors: validationErrors,
              });

              return;
            }
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
                ukprn: this.parseOptionalInt(record.UKPRN),
                name: record.EstablishmentName,
                address: {
                  street: this.parseOptionalString(record.Street),
                  locality: this.parseOptionalString(record.Locality),
                  addressThree: this.parseOptionalString(record.Address3),
                  town: this.parseOptionalString(record.Town),
                  county: this.parseOptionalString(record['County (name)']),
                  postCode: record.PostCode,
                },
                establishmentNumber: this.parseOptionalString(record.EstablishmentNumber),
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

  private parseOptionalString(value: string) {
    if (value === '') return null;
    return value;
  }

  private parseOptionalInt(value: string) {
    if (value === '') return null;
    return Number.parseInt(value);
  }
}
