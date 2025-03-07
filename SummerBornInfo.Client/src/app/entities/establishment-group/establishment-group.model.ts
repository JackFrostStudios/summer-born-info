interface EstablishmentGroupBase {
  code: string;
  name: string;
}

export interface EstablishmentGroup extends EstablishmentGroupBase {
  id: string;
}

export type CreateEstablishmentGroupRequest = EstablishmentGroupBase;
