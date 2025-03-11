import {
  Address,
  EstablishmentGroup,
  EstablishmentStatus,
  EstablishmentType,
  LocalAuthority,
  PhaseOfEducation,
} from '@entities';

interface SchoolBase {
  urn: number;
  ukprn: number | null;
  establishmentNumber: string | null;
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
