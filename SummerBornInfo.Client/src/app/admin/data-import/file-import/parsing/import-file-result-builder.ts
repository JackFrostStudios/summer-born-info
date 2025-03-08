import {
  CreateLocalAuthorityRequest,
  CreateEstablishmentTypeRequest,
  CreateEstablishmentGroupRequest,
  CreateEstablishmentStatusRequest,
  CreatePhaseOfEducationRequest,
  ImportSchool,
} from '@entities';
import { ImportFileError } from './import-file-error.model';
import { ImportFileResult } from './import-file-result.model';

export class ImportFileResultBuilder {
  private localAuthorities: Record<string, CreateLocalAuthorityRequest> = {};
  private establishmentTypes: Record<string, CreateEstablishmentTypeRequest> = {};
  private establishmentGroups: Record<string, CreateEstablishmentGroupRequest> = {};
  private establishmentStatuses: Record<string, CreateEstablishmentStatusRequest> = {};
  private phasesOfEducation: Record<string, CreatePhaseOfEducationRequest> = {};
  private schools: ImportSchool[] = [];
  private errors: ImportFileError[] = [];

  public AddLocalAuthority(localAuthority: CreateLocalAuthorityRequest): ImportFileResultBuilder {
    if (this.localAuthorities[localAuthority.code] === undefined) {
      this.localAuthorities[localAuthority.code] = localAuthority;
    }
    return this;
  }

  public AddEstablishmentType(establishmentType: CreateEstablishmentTypeRequest): ImportFileResultBuilder {
    if (this.establishmentTypes[establishmentType.code] === undefined) {
      this.establishmentTypes[establishmentType.code] = establishmentType;
    }
    return this;
  }

  public AddEstablishmentGroup(establishmentGroup: CreateEstablishmentGroupRequest): ImportFileResultBuilder {
    if (this.establishmentGroups[establishmentGroup.code] === undefined) {
      this.establishmentGroups[establishmentGroup.code] = establishmentGroup;
    }
    return this;
  }

  public AddEstablishmentStatus(establishmentStatus: CreateEstablishmentStatusRequest): ImportFileResultBuilder {
    if (this.establishmentStatuses[establishmentStatus.code] === undefined) {
      this.establishmentStatuses[establishmentStatus.code] = establishmentStatus;
    }
    return this;
  }

  public AddPhaseOfEducation(phaseOfEducation: CreatePhaseOfEducationRequest): ImportFileResultBuilder {
    if (this.phasesOfEducation[phaseOfEducation.code] === undefined) {
      this.phasesOfEducation[phaseOfEducation.code] = phaseOfEducation;
    }
    return this;
  }

  public AddSchool(school: ImportSchool): ImportFileResultBuilder {
    this.schools.push(school);
    return this;
  }

  public AddError(error: ImportFileError) {
    this.errors.push(error);
    return this;
  }

  public GetResults(): ImportFileResult {
    return {
      localAuthorities: Object.values(this.localAuthorities),
      establishmentTypes: Object.values(this.establishmentTypes),
      establishmentGroups: Object.values(this.establishmentGroups),
      establishmentStatuses: Object.values(this.establishmentStatuses),
      phasesOfEducation: Object.values(this.phasesOfEducation),
      schools: [...this.schools],
      errors: this.errors,
    };
  }
}
