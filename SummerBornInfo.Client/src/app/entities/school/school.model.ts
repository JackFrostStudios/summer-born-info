import { EstablishmentGroup } from '../establishment-group/establishment-group.model';
import { EstablishmentStatus } from '../establishment-status/establishment-status.model';
import { EstablishmentType } from '../establishment-type/establishment-type.model';
import { LocalAuthority } from '../local-authority/local-authority.model';
import { PhaseOfEducation } from '../phase-of-education/phase-of-education.model';
import { Address } from './address.model';

interface SchoolBase {
  urn: number;
  ukprn: number;
  establishmentNumber: number;
  name: string;
  address: Address;
  openDate: Date | null;
  closeDate: Date | null;
}

export interface School extends SchoolBase {
  phaseOfEducation: PhaseOfEducation;
  localAuthority: LocalAuthority;
  establishmentType: EstablishmentType;
  establishmentGroup: EstablishmentGroup;
  establishmentStatus: EstablishmentStatus;
}

export interface ImportSchool extends SchoolBase {
  phaseOfEducationCode: string;
  localAuthorityCode: string;
  establishmentTypeCode: string;
  establishmentGroupCode: string;
  establishmentStatusCode: string;
}

export interface CreateSchoolRequest extends SchoolBase {
  phaseOfEducationId?: string;
  localAuthorityId?: string;
  establishmentTypeId?: string;
  establishmentGroupId?: string;
  establishmentStatusId?: string;
}
