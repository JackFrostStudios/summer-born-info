import { Injectable } from '@angular/core';
import { ImportFileCsvRow } from '../import-file-csv-row.model';

@Injectable({
  providedIn: 'root',
})
export class ImportFileValidatorService {
  private integerFields = ['URN', 'UKPRN', 'EstablishmentNumber'];

  private codeFields = [
    'LA (code)',
    'TypeOfEstablishment (code)',
    'EstablishmentTypeGroup (code)',
    'EstablishmentStatus (code)',
    'PhaseOfEducation (code)',
  ];

  private requireFields = [
    'URN',
    'UKPRN',
    'EstablishmentNumber',
    'EstablishmentName',
    'Street',
    'Locality',
    'Address3',
    'Town',
    'County (name)',
    'PostCode',
    'LA (code)',
    'LA (name)',
    'TypeOfEstablishment (code)',
    'TypeOfEstablishment (name)',
    'EstablishmentTypeGroup (code)',
    'EstablishmentTypeGroup (name)',
    'EstablishmentStatus (code)',
    'EstablishmentStatus (name)',
    'PhaseOfEducation (code)',
    'PhaseOfEducation (name)',
  ];

  validateRow(row: ImportFileCsvRow): string[] {
    const result: string[] = [];
    const record = row as unknown as Record<string, string>;

    this.requireFields.forEach(field => {
      if (record[field] === '') {
        result.push(`${field} is required.`);
      }
    });

    this.integerFields.forEach(field => {
      if (!this.isInteger(record[field])) {
        result.push(`${field} "${record[field]}" must be an integer number.`);
      }
    });

    this.codeFields.forEach(field => {
      if (!this.isValidCode(record[field])) {
        result.push(`${field} "${record[field]}" must be a valid code.`);
      }
    });

    return result;
  }

  private isInteger(value: string) {
    return /^\+?[1-9]\d*$/.test(value);
  }

  private isValidCode(value: string) {
    return /^\+?[0-9]\d*$/.test(value);
  }
}
