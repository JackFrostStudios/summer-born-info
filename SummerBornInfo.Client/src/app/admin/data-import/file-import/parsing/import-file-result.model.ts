import {
  CreateEstablishmentGroupRequest,
  CreateEstablishmentStatusRequest,
  CreateEstablishmentTypeRequest,
  CreateLocalAuthorityRequest,
  CreatePhaseOfEducationRequest,
  CreateSchoolRequest,
} from '@entities';
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
