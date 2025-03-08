import {
  CreateEstablishmentGroupRequest,
  CreateEstablishmentStatusRequest,
  CreateEstablishmentTypeRequest,
  CreateLocalAuthorityRequest,
  CreatePhaseOfEducationRequest,
} from '@entities';
import { CreateSchoolRequest } from '@entities/school/school.model';
import { ImportFileError } from './import-file-error.model';

export interface ImportFileResult {
  localAuthorities: CreateLocalAuthorityRequest[];
  establishmentTypes: CreateEstablishmentTypeRequest[];
  establishmentGroups: CreateEstablishmentGroupRequest[];
  establishmentStatuses: CreateEstablishmentStatusRequest[];
  phasesOfEducation: CreatePhaseOfEducationRequest[];
  schools: CreateSchoolRequest[];
  errors: ImportFileError[];
}
