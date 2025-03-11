import { TestBed } from '@angular/core/testing';

import { ImportFileValidatorService } from './import-file-validator.service';
import { ImportFileCsvRow } from '../import-file-csv-row.model';

describe('ImportFileValidatorService', () => {
  let service: ImportFileValidatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ImportFileValidatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Given a row with correct information is validated', () => {
    let result: string[];
    beforeEach(() => {
      const row = getRow();
      result = service.validateRow(row);
    });

    it('Then no error messages should be returned', () => {
      expect(result).toEqual([]);
    });
  });

  describe('Given the required integer values for a CSV row', () => {
    ['URN'].forEach(field => {
      describe(`When a row where ${field} is missing is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = '';
          result = service.validateRow(row);
        });

        it(`Then a "required ${field}" error message is returned`, () => {
          expect(result).toEqual([`${field} is required.`]);
        });
      });

      describe(`When a row where ${field} is not a number is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = 'abcdef';
          result = service.validateRow(row);
        });

        it(`Then a "${field} must be an integer number" error message should be returned`, () => {
          expect(result).toEqual([`${field} "abcdef" must be an integer number.`]);
        });
      });

      describe(`When a row where ${field} is not an integer is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = '1.2';
          result = service.validateRow(row);
        });

        it(`Then a "${field} must be an integer number" message should be returned`, () => {
          expect(result).toEqual([`${field} "1.2" must be an integer number.`]);
        });
      });
    });
  });

  describe('Given the optional integer values for a CSV row', () => {
    ['UKPRN'].forEach(field => {
      describe(`When a row where ${field} is missing is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = '';
          result = service.validateRow(row);
        });

        it(`Then no error messages are returned`, () => {
          expect(result).toEqual([]);
        });
      });

      describe(`When a row where ${field} is not a number is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = 'abcdef';
          result = service.validateRow(row);
        });

        it(`Then a "${field} must be an integer number" error message should be returned`, () => {
          expect(result).toEqual([`${field} "abcdef" must be an integer number.`]);
        });
      });

      describe(`When a row where ${field} is not an integer is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = '1.2';
          result = service.validateRow(row);
        });

        it(`Then a "${field} must be an integer number" message should be returned`, () => {
          expect(result).toEqual([`${field} "1.2" must be an integer number.`]);
        });
      });
    });
  });

  describe('Given the required code values for a CSV row', () => {
    [
      'LA (code)',
      'TypeOfEstablishment (code)',
      'EstablishmentTypeGroup (code)',
      'EstablishmentStatus (code)',
      'PhaseOfEducation (code)',
    ].forEach(field => {
      describe(`When a row where ${field} is missing is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = '';
          result = service.validateRow(row);
        });

        it(`Then a "required ${field}" error message is returned`, () => {
          expect(result).toEqual([`${field} is required.`]);
        });
      });

      describe(`When a row where ${field} is not a code is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = 'abcdef';
          result = service.validateRow(row);
        });

        it(`Then a "${field} must be a valid code" error message should be returned`, () => {
          expect(result).toEqual([`${field} "abcdef" must be a valid code.`]);
        });
      });
    });
  });

  describe('Given the optional code values for a CSV row', () => {
    ['EstablishmentNumber'].forEach(field => {
      describe(`When a row where ${field} is missing is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = '';
          result = service.validateRow(row);
        });

        it(`Then no error message is returned`, () => {
          expect(result).toEqual([]);
        });
      });

      describe(`When a row where ${field} is not a code is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = 'abcdef';
          result = service.validateRow(row);
        });

        it(`Then a "${field} must be a valid code" error message should be returned`, () => {
          expect(result).toEqual([`${field} "abcdef" must be a valid code.`]);
        });
      });
    });
  });

  describe('Given the required string values for a CSV row', () => {
    [
      'EstablishmentName',
      'PostCode',
      'LA (name)',
      'TypeOfEstablishment (name)',
      'EstablishmentTypeGroup (name)',
      'EstablishmentStatus (name)',
      'PhaseOfEducation (name)',
    ].forEach(field => {
      describe(`When a row where ${field} is missing is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = '';
          result = service.validateRow(row);
        });

        it(`Then a "required ${field}" error message is returned`, () => {
          expect(result).toEqual([`${field} is required.`]);
        });
      });
    });
  });

  describe('Given the optional date values for a CSV row', () => {
    ['OpenDate', 'CloseDate'].forEach(field => {
      describe(`When a row where ${field} is missing is validated`, () => {
        let result: string[];
        beforeEach(() => {
          const row = getRow();
          (row as unknown as Record<string, string>)[field] = '';
          result = service.validateRow(row);
        });

        it(`Then no error message is returned`, () => {
          expect(result).toEqual([]);
        });
      });
    });
  });
});

const getRow = (): ImportFileCsvRow => {
  return {
    URN: '1',
    UKPRN: '2',
    EstablishmentName: 'EstablishmentName',
    Street: 'Street',
    Locality: 'Locality',
    Address3: 'Address3',
    Town: 'Town',
    'County (name)': 'County',
    PostCode: 'PostCode',
    'LA (code)': '01',
    'LA (name)': 'LaName',
    EstablishmentNumber: '3',
    'TypeOfEstablishment (code)': '02',
    'TypeOfEstablishment (name)': 'TypeName',
    'EstablishmentTypeGroup (code)': '4',
    'EstablishmentTypeGroup (name)': 'GroupName',
    'EstablishmentStatus (code)': '1',
    'EstablishmentStatus (name)': 'StatusName',
    OpenDate: '2020-03-30T12:00:00',
    CloseDate: '2021-03-30T12:00:00',
    'PhaseOfEducation (code)': '0',
    'PhaseOfEducation (name)': 'PhaseName',
  };
};
