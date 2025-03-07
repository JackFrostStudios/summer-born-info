import { TestBed } from '@angular/core/testing';

import { ImportFileParsingService } from './import-file-parsing.service';
import { DataRow, getDataRow, getHeaderRow } from '../import-csv-file.test';
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

  describe('Given an import file with a single record', () => {
    let inputFile: File;
    beforeEach(() => {
      let data = '';
      const dataRow = getCsvRow();
      data += getHeaderRow();
      data += getDataRow(dataRow);
      const blob = new Blob([data], { type: 'text/csv' });
      inputFile = new File([blob], 'import.csv', { type: 'text/csv' });
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
        expect(schoolResult.establishmentNumber).toBe(3);
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
        expect(result.establishmentGroups[0].code).toBe('groupCode');
        expect(result.establishmentGroups[0].name).toBe('groupName');
      });

      it('Then the result contains the establishment status', () => {
        expect(result.establishmentStatuses.length).toBe(1);
        expect(result.establishmentStatuses[0].code).toBe('statusCode');
        expect(result.establishmentStatuses[0].name).toBe('statusName');
      });

      it('Then the result contains the establishment type', () => {
        expect(result.establishmentTypes.length).toBe(1);
        expect(result.establishmentTypes[0].code).toBe('typeCode');
        expect(result.establishmentTypes[0].name).toBe('typeName');
      });

      it('Then the result contains the local authority', () => {
        expect(result.localAuthorities.length).toBe(1);
        expect(result.localAuthorities[0].code).toBe('laCode');
        expect(result.localAuthorities[0].name).toBe('laName');
      });

      it('Then the result contains the phase of education', () => {
        expect(result.phasesOfEducation.length).toBe(1);
        expect(result.phasesOfEducation[0].code).toBe('phaseCode');
        expect(result.phasesOfEducation[0].name).toBe('phaseName');
      });
    });
  });

  describe('Given an import file with a single record without closed or open dates', () => {
    let inputFile: File;
    beforeEach(() => {
      let data = '';
      const dataRow = getCsvRow();
      dataRow.openDate = '';
      dataRow.closeDate = '';
      data += getHeaderRow();
      data += getDataRow(dataRow);
      const blob = new Blob([data], { type: 'text/csv' });
      inputFile = new File([blob], 'import.csv', { type: 'text/csv' });
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
        expect(schoolResult.establishmentNumber).toBe(3);
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
        expect(result.establishmentGroups[0].code).toBe('groupCode');
        expect(result.establishmentGroups[0].name).toBe('groupName');
      });

      it('Then the result contains the establishment status', () => {
        expect(result.establishmentStatuses.length).toBe(1);
        expect(result.establishmentStatuses[0].code).toBe('statusCode');
        expect(result.establishmentStatuses[0].name).toBe('statusName');
      });

      it('Then the result contains the establishment type', () => {
        expect(result.establishmentTypes.length).toBe(1);
        expect(result.establishmentTypes[0].code).toBe('typeCode');
        expect(result.establishmentTypes[0].name).toBe('typeName');
      });

      it('Then the result contains the local authority', () => {
        expect(result.localAuthorities.length).toBe(1);
        expect(result.localAuthorities[0].code).toBe('laCode');
        expect(result.localAuthorities[0].name).toBe('laName');
      });

      it('Then the result contains the phase of education', () => {
        expect(result.phasesOfEducation.length).toBe(1);
        expect(result.phasesOfEducation[0].code).toBe('phaseCode');
        expect(result.phasesOfEducation[0].name).toBe('phaseName');
      });
    });
  });

  describe('Given an import file with multiple records with the same lookup codes', () => {
    let inputFile: File;
    beforeEach(() => {
      let data = '';
      const dataRow = getCsvRow();
      data += getHeaderRow();
      data += getDataRow(dataRow);
      dataRow.urn = '10';
      dataRow.ukprn = '20';
      dataRow.establishmentName = 'establishmentName2';
      dataRow.establishmentNumber = '30';
      data += getDataRow(dataRow);
      const blob = new Blob([data], { type: 'text/csv' });
      inputFile = new File([blob], 'import.csv', { type: 'text/csv' });
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
      let data = '';
      const dataRow = getCsvRow();
      data += getHeaderRow();
      data += getDataRow(dataRow);
      dataRow.urn = '10';
      dataRow.ukprn = '20';
      dataRow.establishmentName = 'establishmentName2';
      dataRow.establishmentNumber = '30';
      dataRow.typeCode += '2';
      dataRow.typeName += '2';
      data += getDataRow(dataRow);
      const blob = new Blob([data], { type: 'text/csv' });
      inputFile = new File([blob], 'import.csv', { type: 'text/csv' });
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
        expect(result.establishmentTypes.find(s => s.code === 'typeCode'))
          .withContext('Establishment type with code "typeCode" should be in results.')
          .toBeDefined();
        expect(result.establishmentTypes.find(s => s.code === 'typeCode2'))
          .withContext('Establishment type with code "typeCode2" should be in results.')
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
});

const getCsvRow = (): DataRow => {
  return {
    laCode: 'laCode',
    laName: 'laName',
    typeCode: 'typeCode',
    typeName: 'typeName',
    groupCode: 'groupCode',
    groupName: 'groupName',
    statusCode: 'statusCode',
    statusName: 'statusName',
    phaseCode: 'phaseCode',
    phaseName: 'phaseName',
    urn: '1',
    ukprn: '2',
    establishmentName: 'establishmentName',
    establishmentNumber: '3',
    openDate: '2020-03-30T12:00:00',
    closeDate: '2021-03-30T12:00:00',
    street: 'street',
    locality: 'locality',
    addressThree: 'addressThree',
    town: 'town',
    county: 'county',
    postcode: 'postcode',
  };
};
