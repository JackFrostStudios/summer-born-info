import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ImportFileParsingService } from './file-import/parsing/import-file-parsing.service';
import { ImportFileResult } from './file-import/parsing/import-file-result.model';
import { EstablishmentGroupService } from '@entities/establishment-group/establishment-group.service';
import { EstablishmentStatusService } from '@entities/establishment-status/establishment-status.service';
import { LocalAuthorityService } from '@entities/local-authority/local-authority.service';
import { PhaseOfEducationService } from '@entities/phase-of-education/phase-of-education.service';
import { SchoolService } from '@entities/school/school.service';
import {
  EstablishmentGroup,
  EstablishmentStatus,
  EstablishmentType,
  LocalAuthority,
  PhaseOfEducation,
} from '@entities';
import { EstablishmentTypeService } from '@entities/establishment-type/establishment-type.service';

@Component({
  selector: 'sb-data-import',
  imports: [CommonModule],
  templateUrl: './data-import.component.html',
  styleUrl: './data-import.component.css',
})
export class DataImportComponent {
  constructor(
    private readonly importFileParsingService: ImportFileParsingService,
    private readonly establishmentGroupService: EstablishmentGroupService,
    private readonly establishmentStatusService: EstablishmentStatusService,
    private readonly establishmentTypeService: EstablishmentTypeService,
    private readonly localAuthorityService: LocalAuthorityService,
    private readonly phaseOfEducationService: PhaseOfEducationService,
    private readonly schoolService: SchoolService
  ) {}
  fileProcessedSuccessfully = false;
  fileProcessedWithErrors = false;
  unexpectedError = false;

  async fileSelected(event: Event) {
    this.resetStatus();
    const target = event?.currentTarget as HTMLInputElement;
    const file = target?.files?.[0];

    if (!file) {
      console.error('No files could be found.');
      this.unexpectedError = true;
      return;
    }

    try {
      const result = await this.importFileParsingService.parseImportFile(file);
      await this.processResult(result);
    } catch (e) {
      console.error(e);
      this.unexpectedError = true;
    }
  }

  private async processResult(result: ImportFileResult) {
    if (result.errors.length > 0) {
      this.fileProcessedWithErrors = true;
      console.error(result.errors);
      return;
    }

    this.fileProcessedSuccessfully = true;

    const createdGroups: EstablishmentGroup[] = [];
    for (const establishmentGroupRequest of result.establishmentGroups) {
      const estasblishmentGroup =
        await this.establishmentGroupService.createEstablishmentGroup(establishmentGroupRequest);
      console.log(estasblishmentGroup);
      createdGroups.push(estasblishmentGroup);
    }

    const createdStatuses: EstablishmentStatus[] = [];
    for (const establishmentStatusRequest of result.establishmentStatuses) {
      const establishmentStatus =
        await this.establishmentStatusService.createEstablishmentStatus(establishmentStatusRequest);
      console.log(establishmentStatus);
      createdStatuses.push(establishmentStatus);
    }

    const createdTypes: EstablishmentType[] = [];
    for (const establishmentTypeRequest of result.establishmentTypes) {
      const establishmentType = await this.establishmentTypeService.createEstablishmentType(establishmentTypeRequest);
      console.log(establishmentType);
      createdTypes.push(establishmentType);
    }

    const createdAuthorities: LocalAuthority[] = [];
    for (const localAuthorityRequest of result.localAuthorities) {
      const localAuthority = await this.localAuthorityService.createLocalAuthority(localAuthorityRequest);
      console.log(localAuthority);
      createdAuthorities.push(localAuthority);
    }

    const createdPhases: PhaseOfEducation[] = [];
    for (const phaseOfEducationRequest of result.phasesOfEducation) {
      const phaseOfEducation = await this.phaseOfEducationService.createPhaseOfEducation(phaseOfEducationRequest);
      console.log(phaseOfEducation);
      createdPhases.push(phaseOfEducation);
    }
  }

  private resetStatus() {
    this.fileProcessedSuccessfully = false;
    this.fileProcessedWithErrors = false;
    this.unexpectedError = false;
  }
}
