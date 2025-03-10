interface EstablishmentTypeBase {
  code: string;
  name: string;
}

export interface EstablishmentType extends EstablishmentTypeBase {
  id: string;
}

export type CreateEstablishmentTypeRequest = EstablishmentTypeBase;
