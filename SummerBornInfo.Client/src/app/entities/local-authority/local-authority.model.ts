interface LocalAuthorityBase {
  code: string;
  name: string;
}

export interface LocalAuthority extends LocalAuthorityBase {
  id: string;
}

export type CreateLocalAuthorityRequest = LocalAuthorityBase;
