import {
  CreateEstablishmentGroupRequest,
  CreateEstablishmentStatusRequest,
  CreateEstablishmentTypeRequest,
  CreateLocalAuthorityRequest,
  CreatePhaseOfEducationRequest,
} from '@entities';
import { getTestImportSchool } from '@test-helpers';
import { ImportFileError } from './import-file-error.model';
import { ImportFileResultBuilder } from './import-file-result-builder';

describe('ImportFileResultBuilder', () => {
  let importFileResultBuilder: ImportFileResultBuilder;

  beforeEach(() => {
    importFileResultBuilder = new ImportFileResultBuilder();
  });

  it('AddLocalAuthority should add a local authority to the result and return the builder', () => {
    const localAuthority: CreateLocalAuthorityRequest = { code: 'LA1', name: 'Local Authority 1' };
    const returnedBuilder = importFileResultBuilder.AddLocalAuthority(localAuthority);

    expect(returnedBuilder).toBe(importFileResultBuilder);

    const results = importFileResultBuilder.GetResults();
    expect(results.localAuthorities).toHaveSize(1);
    expect(results.localAuthorities[0].code).toBe('LA1');
    expect(results.localAuthorities[0].name).toBe('Local Authority 1');
  });

  it('AddLocalAuthority should not add a duplicate local authority', () => {
    const localAuthority: CreateLocalAuthorityRequest = { code: 'LA1', name: 'Local Authority 1' };
    importFileResultBuilder.AddLocalAuthority(localAuthority).AddLocalAuthority(localAuthority);

    const results = importFileResultBuilder.GetResults();
    expect(results.localAuthorities).toHaveSize(1);
  });

  it('AddEstablishmentType should add an establishment type to the result and return the builder', () => {
    const establishmentType: CreateEstablishmentTypeRequest = { code: 'ET1', name: 'Establishment Type 1' };
    const returnedBuilder = importFileResultBuilder.AddEstablishmentType(establishmentType);

    expect(returnedBuilder).toBe(importFileResultBuilder);

    const results = importFileResultBuilder.GetResults();
    expect(results.establishmentTypes).toHaveSize(1);
    expect(results.establishmentTypes[0].code).toBe('ET1');
    expect(results.establishmentTypes[0].name).toBe('Establishment Type 1');
  });

  it('AddEstablishmentType should not add a duplicate establishment type', () => {
    const establishmentType: CreateEstablishmentTypeRequest = { code: 'ET1', name: 'Establishment Type 1' };
    importFileResultBuilder.AddEstablishmentType(establishmentType).AddEstablishmentType(establishmentType);

    const results = importFileResultBuilder.GetResults();
    expect(results.establishmentTypes).toHaveSize(1);
  });

  it('AddEstablishmentGroup should add an establishment group to the list and return the builder', () => {
    const establishmentGroup: CreateEstablishmentGroupRequest = { code: 'EG1', name: 'Establishment Group 1' };
    const returnedBuilder = importFileResultBuilder.AddEstablishmentGroup(establishmentGroup);

    expect(returnedBuilder).toBe(importFileResultBuilder);

    const results = importFileResultBuilder.GetResults();
    expect(results.establishmentGroups).toHaveSize(1);
    expect(results.establishmentGroups[0].code).toBe('EG1');
    expect(results.establishmentGroups[0].name).toBe('Establishment Group 1');
  });

  it('AddEstablishmentGroup should not add a duplicate establishment group', () => {
    const establishmentGroup: CreateEstablishmentGroupRequest = { code: 'EG1', name: 'Establishment Group 1' };
    importFileResultBuilder.AddEstablishmentGroup(establishmentGroup).AddEstablishmentGroup(establishmentGroup);

    const results = importFileResultBuilder.GetResults();
    expect(results.establishmentGroups).toHaveSize(1);
  });

  it('AddEstablishmentStatus should add an establishment status to the list and return the builder', () => {
    const establishmentStatus: CreateEstablishmentStatusRequest = { code: 'ES1', name: 'Establishment Status 1' };
    const returnedBuilder = importFileResultBuilder.AddEstablishmentStatus(establishmentStatus);

    expect(returnedBuilder).toBe(importFileResultBuilder);

    const results = importFileResultBuilder.GetResults();
    expect(results.establishmentStatuses).toHaveSize(1);
    expect(results.establishmentStatuses[0].code).toBe('ES1');
    expect(results.establishmentStatuses[0].name).toBe('Establishment Status 1');
  });

  it('AddEstablishmentStatus should not add a duplicate establishment status', () => {
    const establishmentStatus: CreateEstablishmentStatusRequest = { code: 'ES1', name: 'Establishment Status 1' };
    importFileResultBuilder.AddEstablishmentStatus(establishmentStatus).AddEstablishmentStatus(establishmentStatus);

    const results = importFileResultBuilder.GetResults();
    expect(results.establishmentStatuses).toHaveSize(1);
  });

  it('AddPhaseOfEducation should add a phase of education to the result and return the builder', () => {
    const phaseOfEducation: CreatePhaseOfEducationRequest = { code: 'PE1', name: 'Phase of Education 1' };
    const returnedBuilder = importFileResultBuilder.AddPhaseOfEducation(phaseOfEducation);

    expect(returnedBuilder).toBe(importFileResultBuilder);

    const results = importFileResultBuilder.GetResults();
    expect(results.phasesOfEducation).toHaveSize(1);
    expect(results.phasesOfEducation[0].code).toBe('PE1');
    expect(results.phasesOfEducation[0].name).toBe('Phase of Education 1');
  });

  it('AddPhaseOfEducation not add a duplicate phase of education', () => {
    const phaseOfEducation: CreatePhaseOfEducationRequest = { code: 'PE1', name: 'Phase of Education 1' };
    importFileResultBuilder.AddPhaseOfEducation(phaseOfEducation).AddPhaseOfEducation(phaseOfEducation);

    const results = importFileResultBuilder.GetResults();
    expect(results.phasesOfEducation).toHaveSize(1);
  });

  it('AddSchool should add a school to the list and return the builder', () => {
    const school = getTestImportSchool();
    const returnedBuilder = importFileResultBuilder.AddSchool(school);

    expect(returnedBuilder).toBe(importFileResultBuilder);

    const results = importFileResultBuilder.GetResults();
    expect(results.schools).toHaveSize(1);
    expect(results.schools[0]).toEqual(school);
  });

  it('AddError should add a error to the list and return the builder', () => {
    const error: ImportFileError = {
      rowNumber: 1,
      errors: ['Error Importing'],
    };
    const returnedBuilder = importFileResultBuilder.AddError(error);

    expect(returnedBuilder).toBe(importFileResultBuilder);

    const results = importFileResultBuilder.GetResults();
    expect(results.errors).toHaveSize(1);
    expect(results.errors[0]).toEqual(error);
  });

  it('GetResults should include all added data in the result and return the builder', () => {
    const localAuthority: CreateLocalAuthorityRequest = { code: 'LA1', name: 'Local Authority 1' };
    const establishmentType: CreateEstablishmentTypeRequest = { code: 'ET1', name: 'Establishment Type 1' };
    const establishmentGroup: CreateEstablishmentGroupRequest = { code: 'EG1', name: 'Establishment Group 1' };
    const establishmentStatus: CreateEstablishmentStatusRequest = { code: 'ES1', name: 'Establishment Status 1' };
    const phaseOfEducation: CreatePhaseOfEducationRequest = { code: 'PE1', name: 'Phase of Education 1' };
    const school = getTestImportSchool();
    const error: ImportFileError = {
      rowNumber: 1,
      errors: ['Error Importing'],
    };

    const returnedBuilder = importFileResultBuilder
      .AddLocalAuthority(localAuthority)
      .AddEstablishmentType(establishmentType)
      .AddEstablishmentGroup(establishmentGroup)
      .AddEstablishmentStatus(establishmentStatus)
      .AddPhaseOfEducation(phaseOfEducation)
      .AddSchool(school)
      .AddError(error);

    expect(returnedBuilder).toBe(importFileResultBuilder);

    const results = importFileResultBuilder.GetResults();
    expect(results.localAuthorities).toHaveSize(1);
    expect(results.establishmentTypes).toHaveSize(1);
    expect(results.establishmentGroups).toHaveSize(1);
    expect(results.establishmentStatuses).toHaveSize(1);
    expect(results.phasesOfEducation).toHaveSize(1);
    expect(results.schools).toHaveSize(1);
    expect(results.errors).toHaveSize(1);
  });
});
