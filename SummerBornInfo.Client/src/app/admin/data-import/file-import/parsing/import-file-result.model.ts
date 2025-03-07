import { CreateEstablishmentGroupRequest } from '../../../../entities/establishment-group/establishment-group.model';
import { CreateEstablishmentStatusRequest } from '../../../../entities/establishment-status/establishment-status.model';
import { CreateEstablishmentTypeRequest } from '../../../../entities/establishment-type/establishment-type.model';
import { CreateLocalAuthorityRequest } from '../../../../entities/local-authority/local-authority.model';
import { CreatePhaseOfEducationRequest } from '../../../../entities/phase-of-education/phase-of-education.model';
import { CreateSchoolRequest } from '../../../../entities/school/school.model';

export interface ImportFileResult {
  localAuthorities: CreateLocalAuthorityRequest[];
  establishmentTypes: CreateEstablishmentTypeRequest[];
  establishmentGroups: CreateEstablishmentGroupRequest[];
  establishmentStatuses: CreateEstablishmentStatusRequest[];
  phasesOfEducation: CreatePhaseOfEducationRequest[];
  schools: CreateSchoolRequest[];
}
