interface PhaseOfEducationBase {
  code: string;
  name: string;
}

export interface PhaseOfEducation extends PhaseOfEducationBase {
  id: string;
}

export type CreatePhaseOfEducationRequest = PhaseOfEducationBase;
