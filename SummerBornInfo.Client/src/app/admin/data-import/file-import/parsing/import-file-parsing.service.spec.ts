import { TestBed } from '@angular/core/testing';

import { getCsvFile, getInvalidCsvFile, getValidCsvFile, getValidRowData } from '@test-helpers';
import { ParseError } from 'papaparse';
import { ImportFileParsingService } from './import-file-parsing.service';
import { ImportFileResult } from './import-file-result.model';

describe('ImportFileParsingService', () => {
  let service: ImportFileParsingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ImportFileParsingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Given an invalid CSV file', () => {
    let inputFile: File;
    beforeEach(() => {
      inputFile = getInvalidCsvFile();
    });

    describe('When the file is parsed', () => {
      let result: ImportFileResult;
      let errors: ParseError[];
      beforeEach(async () => {
        try {
          result = await service.parseImportFile(inputFile);
        } catch (e) {
          errors = e as ParseError[];
        }
      });

      it('Then the error is thrown', () => {
        expect(result).toBeUndefined();
        expect(errors).toBeDefined();
        expect(errors).toHaveSize(2);
        expect(errors[0].type).toBe('Quotes');
        expect(errors[1].type).toBe('Delimiter');
      });
    });
  });

  describe('Given an import file with a single record', () => {
    let inputFile: File;
    beforeEach(() => {
      inputFile = getValidCsvFile();
    });

    describe('When the file is imported', () => {
      let result: ImportFileResult;
      beforeEach(async () => {
        result = await service.parseImportFile(inputFile);
      });

      it('Then the result contains the school data', () => {
        expect(result.schools.length).toBe(1);
        const schoolResult = result.schools[0];
        expect(schoolResult.urn).toBe(1);
        expect(schoolResult.ukprn).toBe(2);
        expect(schoolResult.name).toBe('establishmentName');
        expect(schoolResult.establishmentNumber).toBe('03');
        expect(schoolResult.openDate).toEqual(new Date('2020-03-30T12:00:00'));
        expect(schoolResult.closeDate).toEqual(new Date('2021-03-30T12:00:00'));
        expect(schoolResult.address.street).toBe('street');
        expect(schoolResult.address.locality).toBe('locality');
        expect(schoolResult.address.addressThree).toBe('addressThree');
        expect(schoolResult.address.town).toBe('town');
        expect(schoolResult.address.county).toBe('county');
        expect(schoolResult.address.postCode).toBe('postcode');
      });

      it('Then the result contains the establishment group', () => {
        expect(result.establishmentGroups.length).toBe(1);
        expect(result.establishmentGroups[0].code).toBe('03');
        expect(result.establishmentGroups[0].name).toBe('groupName');
      });

      it('Then the result contains the establishment status', () => {
        expect(result.establishmentStatuses.length).toBe(1);
        expect(result.establishmentStatuses[0].code).toBe('04');
        expect(result.establishmentStatuses[0].name).toBe('statusName');
      });

      it('Then the result contains the establishment type', () => {
        expect(result.establishmentTypes.length).toBe(1);
        expect(result.establishmentTypes[0].code).toBe('02');
        expect(result.establishmentTypes[0].name).toBe('typeName');
      });

      it('Then the result contains the local authority', () => {
        expect(result.localAuthorities.length).toBe(1);
        expect(result.localAuthorities[0].code).toBe('01');
        expect(result.localAuthorities[0].name).toBe('laName');
      });

      it('Then the result contains the phase of education', () => {
        expect(result.phasesOfEducation.length).toBe(1);
        expect(result.phasesOfEducation[0].code).toBe('05');
        expect(result.phasesOfEducation[0].name).toBe('phaseName');
      });
    });
  });

  describe('Given an import file with a single record without closed or open dates', () => {
    let inputFile: File;
    beforeEach(() => {
      const dataRow = getValidRowData();
      dataRow.openDate = '';
      dataRow.closeDate = '';
      inputFile = getCsvFile([dataRow]);
    });

    describe('When the file is imported', () => {
      let result: ImportFileResult;
      beforeEach(async () => {
        result = await service.parseImportFile(inputFile);
      });

      it('Then the result contains the school data with the dates set to null', () => {
        expect(result.schools.length).toBe(1);
        const schoolResult = result.schools[0];
        expect(schoolResult.urn).toBe(1);
        expect(schoolResult.ukprn).toBe(2);
        expect(schoolResult.name).toBe('establishmentName');
        expect(schoolResult.establishmentNumber).toBe('03');
        expect(schoolResult.openDate).toBeNull();
        expect(schoolResult.closeDate).toBeNull();
        expect(schoolResult.address.street).toBe('street');
        expect(schoolResult.address.locality).toBe('locality');
        expect(schoolResult.address.addressThree).toBe('addressThree');
        expect(schoolResult.address.town).toBe('town');
        expect(schoolResult.address.county).toBe('county');
        expect(schoolResult.address.postCode).toBe('postcode');
      });

      it('Then the result contains the establishment group', () => {
        expect(result.establishmentGroups.length).toBe(1);
        expect(result.establishmentGroups[0].code).toBe('03');
        expect(result.establishmentGroups[0].name).toBe('groupName');
      });

      it('Then the result contains the establishment status', () => {
        expect(result.establishmentStatuses.length).toBe(1);
        expect(result.establishmentStatuses[0].code).toBe('04');
        expect(result.establishmentStatuses[0].name).toBe('statusName');
      });

      it('Then the result contains the establishment type', () => {
        expect(result.establishmentTypes.length).toBe(1);
        expect(result.establishmentTypes[0].code).toBe('02');
        expect(result.establishmentTypes[0].name).toBe('typeName');
      });

      it('Then the result contains the local authority', () => {
        expect(result.localAuthorities.length).toBe(1);
        expect(result.localAuthorities[0].code).toBe('01');
        expect(result.localAuthorities[0].name).toBe('laName');
      });

      it('Then the result contains the phase of education', () => {
        expect(result.phasesOfEducation.length).toBe(1);
        expect(result.phasesOfEducation[0].code).toBe('05');
        expect(result.phasesOfEducation[0].name).toBe('phaseName');
      });
    });
  });

  describe('Given an import file with a single record without any optional fields', () => {
    let inputFile: File;
    beforeEach(() => {
      const dataRow = getValidRowData();
      dataRow.openDate = '';
      dataRow.closeDate = '';
      dataRow.ukprn = '';
      dataRow.establishmentNumber = '';
      dataRow.street = '';
      dataRow.locality = '';
      dataRow.addressThree = '';
      dataRow.town = '';
      dataRow.county = '';
      inputFile = getCsvFile([dataRow]);
    });

    describe('When the file is imported', () => {
      let result: ImportFileResult;
      beforeEach(async () => {
        result = await service.parseImportFile(inputFile);
      });

      it('Then the result contains the school data with the optional values to null', () => {
        expect(result.schools.length).toBe(1);
        const schoolResult = result.schools[0];
        expect(schoolResult.urn).toBe(1);
        expect(schoolResult.ukprn).toBeNull();
        expect(schoolResult.name).toBe('establishmentName');
        expect(schoolResult.establishmentNumber).toBeNull();
        expect(schoolResult.openDate).toBeNull();
        expect(schoolResult.closeDate).toBeNull();
        expect(schoolResult.address.street).toBeNull();
        expect(schoolResult.address.locality).toBeNull();
        expect(schoolResult.address.addressThree).toBeNull();
        expect(schoolResult.address.town).toBeNull();
        expect(schoolResult.address.county).toBeNull();
        expect(schoolResult.address.postCode).toBe('postcode');
      });

      it('Then the result contains the establishment group', () => {
        expect(result.establishmentGroups.length).toBe(1);
        expect(result.establishmentGroups[0].code).toBe('03');
        expect(result.establishmentGroups[0].name).toBe('groupName');
      });

      it('Then the result contains the establishment status', () => {
        expect(result.establishmentStatuses.length).toBe(1);
        expect(result.establishmentStatuses[0].code).toBe('04');
        expect(result.establishmentStatuses[0].name).toBe('statusName');
      });

      it('Then the result contains the establishment type', () => {
        expect(result.establishmentTypes.length).toBe(1);
        expect(result.establishmentTypes[0].code).toBe('02');
        expect(result.establishmentTypes[0].name).toBe('typeName');
      });

      it('Then the result contains the local authority', () => {
        expect(result.localAuthorities.length).toBe(1);
        expect(result.localAuthorities[0].code).toBe('01');
        expect(result.localAuthorities[0].name).toBe('laName');
      });

      it('Then the result contains the phase of education', () => {
        expect(result.phasesOfEducation.length).toBe(1);
        expect(result.phasesOfEducation[0].code).toBe('05');
        expect(result.phasesOfEducation[0].name).toBe('phaseName');
      });
    });
  });

  describe('Given an import file with multiple records with the same lookup codes', () => {
    let inputFile: File;
    beforeEach(() => {
      const dataRowOne = getValidRowData();
      const dataRowTwo = getValidRowData();
      dataRowTwo.urn = '10';
      dataRowTwo.ukprn = '20';
      dataRowTwo.establishmentName = 'establishmentName2';
      dataRowTwo.establishmentNumber = '30';
      inputFile = getCsvFile([dataRowOne, dataRowTwo]);
    });

    describe('When the file is imported', () => {
      let result: ImportFileResult;
      beforeEach(async () => {
        result = await service.parseImportFile(inputFile);
      });

      it('Then the result contains two schools', () => {
        expect(result.schools.length).toBe(2);
        expect(result.schools.find(s => s.urn === 1))
          .withContext('School with URN 1 should be in results.')
          .toBeDefined();
        expect(result.schools.find(s => s.urn === 10))
          .withContext('School with URN 10 should be in results.')
          .toBeDefined();
      });

      it('Then the result contains one establishment group', () => {
        expect(result.establishmentGroups.length).toBe(1);
      });

      it('Then the result contains one establishment status', () => {
        expect(result.establishmentStatuses.length).toBe(1);
      });

      it('Then the result contains one establishment type', () => {
        expect(result.establishmentTypes.length).toBe(1);
      });

      it('Then the result contains one local authority', () => {
        expect(result.localAuthorities.length).toBe(1);
      });

      it('Then the result contains one phase of education', () => {
        expect(result.phasesOfEducation.length).toBe(1);
      });
    });
  });

  describe('Given an import file with multiple records with different establishment types', () => {
    let inputFile: File;
    beforeEach(() => {
      const dataRowOne = getValidRowData();
      const dataRowTwo = getValidRowData();
      dataRowTwo.urn = '10';
      dataRowTwo.ukprn = '20';
      dataRowTwo.establishmentName = 'establishmentName2';
      dataRowTwo.establishmentNumber = '30';
      dataRowTwo.typeCode = '20';
      dataRowTwo.typeName += '2';
      inputFile = getCsvFile([dataRowOne, dataRowTwo]);
    });

    describe('When the file is imported', () => {
      let result: ImportFileResult;
      beforeEach(async () => {
        result = await service.parseImportFile(inputFile);
      });

      it('Then the result contains two schools', () => {
        expect(result.schools.length).toBe(2);
        expect(result.schools.find(s => s.urn == 1))
          .withContext('School with URN 1 should be in results.')
          .toBeDefined();
        expect(result.schools.find(s => s.urn == 10))
          .withContext('School with URN 10 should be in results.')
          .toBeDefined();
      });

      it('Then the result contains one establishment group', () => {
        expect(result.establishmentGroups.length).toBe(1);
      });

      it('Then the result contains one establishment status', () => {
        expect(result.establishmentStatuses.length).toBe(1);
      });

      it('Then the result contains one establishment type', () => {
        expect(result.establishmentTypes.length).toBe(2);
        expect(result.establishmentTypes.find(s => s.code === '02'))
          .withContext('Establishment type with code "02" should be in results.')
          .toBeDefined();
        expect(result.establishmentTypes.find(s => s.code === '20'))
          .withContext('Establishment type with code "20" should be in results.')
          .toBeDefined();
      });

      it('Then the result contains one local authority', () => {
        expect(result.localAuthorities.length).toBe(1);
      });

      it('Then the result contains one phase of education', () => {
        expect(result.phasesOfEducation.length).toBe(1);
      });
    });
  });

  describe('Given an import file with a single record with invalid data', () => {
    let inputFile: File;
    beforeEach(() => {
      const dataRow = getValidRowData();
      dataRow.ukprn = 'invalidukprn';
      inputFile = getCsvFile([dataRow]);
    });

    describe('When the file is imported', () => {
      let result: ImportFileResult;
      beforeEach(async () => {
        result = await service.parseImportFile(inputFile);
      });

      it('Then the result contains no import data', () => {
        expect(result.schools.length).toBe(0);
        expect(result.establishmentGroups.length).toBe(0);
        expect(result.establishmentStatuses.length).toBe(0);
        expect(result.establishmentTypes.length).toBe(0);
        expect(result.localAuthorities.length).toBe(0);
        expect(result.phasesOfEducation.length).toBe(0);
      });

      it('Then the result contains the error details', () => {
        expect(result.errors.length).toBe(1);
        expect(result.errors[0].rowNumber).toBe(1);
        expect(result.errors[0].errors).toEqual(['UKPRN "invalidukprn" must be an integer number.']);
      });
    });
  });
});
