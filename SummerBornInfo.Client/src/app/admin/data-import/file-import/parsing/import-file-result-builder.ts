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

  public AddLocalAuthority(localAuthority: CreateLocalAuthorityRequest) {
    if (this.localAuthorities[localAuthority.code] === undefined) {
      this.localAuthorities[localAuthority.code] = localAuthority;
    }
  }

  public AddEstablishmentType(establishmentType: CreateEstablishmentTypeRequest) {
    if (this.establishmentTypes[establishmentType.code] === undefined) {
      this.establishmentTypes[establishmentType.code] = establishmentType;
    }
  }

  public AddEstablishmentGroup(establishmentGroup: CreateEstablishmentGroupRequest) {
    if (this.establishmentGroups[establishmentGroup.code] === undefined) {
      this.establishmentGroups[establishmentGroup.code] = establishmentGroup;
    }
  }

  public AddEstablishmentStatus(establishmentStatus: CreateEstablishmentStatusRequest) {
    if (this.establishmentStatuses[establishmentStatus.code] === undefined) {
      this.establishmentStatuses[establishmentStatus.code] = establishmentStatus;
    }
  }

  public AddPhaseOfEducation(phaseOfEducation: CreatePhaseOfEducationRequest) {
    if (this.phasesOfEducation[phaseOfEducation.code] === undefined) {
      this.phasesOfEducation[phaseOfEducation.code] = phaseOfEducation;
    }
  }

  public AddSchool(school: ImportSchool) {
    this.schools.push(school);
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
