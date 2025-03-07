interface EstablishmentStatusBase {
  code: string;
  name: string;
}

export interface EstablishmentStatus extends EstablishmentStatusBase {
  id: string;
}

export type CreateEstablishmentStatusRequest = EstablishmentStatusBase;
