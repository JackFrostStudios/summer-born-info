import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { Router } from '@angular/router';
import { AdminAuthService, type AdminSignOutResult } from '../auth/admin-auth.service';

type AdminSignOutErrorKind = Exclude<AdminSignOutResult['kind'], 'signed-out'>;

@Component({
  selector: 'sbi-admin-home',
  templateUrl: './admin-home.html',
  styleUrl: './admin-home.scss',
})
export class AdminHome {
  private readonly adminAuthService = inject(AdminAuthService);
  private readonly router = inject(Router);
  private readonly $error = signal<AdminSignOutErrorKind | null>(null);
  private readonly $errorSummary = viewChild<ElementRef<HTMLElement>>('errorSummary');

  protected readonly headingId = 'admin-home-heading';
  protected readonly $isSigningOut = computed(() => this.adminAuthService.$isSigningOut());
  protected readonly $errorMessage = computed(() => this.getErrorMessage(this.$error()));

  protected async signOut(): Promise<void> {
    if (this.$isSigningOut()) {
      return;
    }

    this.$error.set(null);

    const result = await this.adminAuthService.signOut();

    if (result.kind === 'signed-out') {
      await this.router.navigateByUrl('/admin/login');
      return;
    }

    this.$error.set(result.kind);
    setTimeout(() => this.$errorSummary()?.nativeElement.focus());
  }

  private getErrorMessage(error: AdminSignOutErrorKind | null): string | null {
    switch (error) {
      case 'network-failure':
        return $localize`:Admin home sign out network failure error|Shown when sign-out cannot reach the server and the admin remains on the protected page@@adminHomeSignOutNetworkFailureError:We could not reach the sign-out service. Stay signed in here and try again.`;
      case 'unexpected-failure':
        return $localize`:Admin home sign out unexpected failure error|Shown when sign-out fails in an unexpected recoverable way and the admin remains on the protected page@@adminHomeSignOutUnexpectedFailureError:Something went wrong while signing you out. Stay signed in here and try again.`;
      case null:
        return null;
    }
  }
}
