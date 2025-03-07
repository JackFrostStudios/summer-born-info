//TODO: Roll up to index files and use named imports.
import { CreateEstablishmentGroupRequest } from '../../../../entities/establishment-group/establishment-group.model';
import { CreateEstablishmentStatusRequest } from '../../../../entities/establishment-status/establishment-status.model';
import { CreateEstablishmentTypeRequest } from '../../../../entities/establishment-type/establishment-type.model';
import { CreateLocalAuthorityRequest } from '../../../../entities/local-authority/local-authority.model';
import { CreatePhaseOfEducationRequest } from '../../../../entities/phase-of-education/phase-of-education.model';
import { ImportSchool } from '../../../../entities/school/school.model';
import { ImportFileResult } from './import-file-result.model';

export class ImportFileResultBuilder {
  private localAuthorities: Record<string, CreateLocalAuthorityRequest> = {};
  private establishmentTypes: Record<string, CreateEstablishmentTypeRequest> = {};
  private establishmentGroups: Record<string, CreateEstablishmentGroupRequest> = {};
  private establishmentStatuses: Record<string, CreateEstablishmentStatusRequest> = {};
  private phasesOfEducation: Record<string, CreatePhaseOfEducationRequest> = {};
  private schools: ImportSchool[] = [];

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

  public GetResults(): ImportFileResult {
    return {
      localAuthorities: Object.values(this.localAuthorities),
      establishmentTypes: Object.values(this.establishmentTypes),
      establishmentGroups: Object.values(this.establishmentGroups),
      establishmentStatuses: Object.values(this.establishmentStatuses),
      phasesOfEducation: Object.values(this.phasesOfEducation),
      schools: [...this.schools],
    };
  }
}
