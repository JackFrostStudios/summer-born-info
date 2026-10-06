import { Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { form, required, validate } from '@angular/forms/signals';
import { AdminAuthService, type AdminSignInResult } from '../auth/admin-auth.service';

type AdminLoginErrorKind = Exclude<AdminSignInResult['kind'], 'signed-in'>;

interface AdminLoginModel {
  email: string;
  password: string;
}

@Component({
  selector: 'sbi-admin-login',
  templateUrl: './admin-login.html',
  styleUrl: './admin-login.scss',
})
export class AdminLogin implements OnInit {
  private readonly adminAuthService = inject(AdminAuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly $error = signal<AdminLoginErrorKind | null>(null);
  private readonly $errorSummary = viewChild<ElementRef<HTMLElement>>('errorSummary');
  protected readonly $model = signal<AdminLoginModel>({ email: '', password: '' });
  protected readonly $form = form(
    this.$model,
    (schema) => {
      const emailRequiredMessage = $localize`:Admin login email required error|Shown when the admin email field is left empty@@adminLoginEmailRequiredError:Enter your email address.`;
      required(schema.email, { message: emailRequiredMessage });
      validate(schema.email, ({ value }) =>
        value().length > 0 && value().trim().length === 0
          ? { kind: 'required', message: emailRequiredMessage }
          : undefined,
      );
      required(schema.password, {
        message: $localize`:Admin login password required error|Shown when the admin password field is left empty@@adminLoginPasswordRequiredError:Enter your password.`,
      });
    },
    {
      submission: {
        action: async () => {
          const { email, password } = this.$model();
          const result = await this.adminAuthService.signIn({ email: email.trim(), password });

          if (result.kind === 'signed-in' || result.kind === 'invalid-credentials') {
            this.$model.update((model) => ({ ...model, password: '' }));
          }

          if (result.kind === 'signed-in') {
            await this.router.navigateByUrl(this.getSafeReturnUrl());
            return undefined;
          }

          return { kind: result.kind, message: this.getErrorMessage(result.kind) };
        },
      },
    },
  );
  protected readonly $emailTouched = signal(false);
  protected readonly $passwordTouched = signal(false);

  protected readonly headingId = 'admin-login-heading';
  protected readonly $email = signal('');
  protected readonly $password = signal('');
  protected readonly $isSubmitting = computed(() => this.adminAuthService.$isSigningIn());
  protected readonly $canSubmit = computed(
    () => this.$email().trim().length > 0 && this.$password().length > 0 && !this.$isSubmitting(),
  );
  protected readonly $errorMessage = computed(() => this.getErrorMessage(this.$error()));
  protected readonly $showEmailRequiredError = computed(
    () => this.$emailTouched() && this.$email().trim().length === 0,
  );
  protected readonly $showPasswordRequiredError = computed(
    () => this.$passwordTouched() && this.$password().length === 0,
  );

  ngOnInit(): void {
    if (this.adminAuthService.$isAuthenticated()) {
      void this.router.navigateByUrl(this.getSafeReturnUrl());
    }
  }

  protected updateEmail(event: Event): void {
    this.$email.set(this.getInputValue(event));
    this.$error.set(null);
  }

  protected updatePassword(event: Event): void {
    this.$password.set(this.getInputValue(event));
    this.$error.set(null);
  }

  protected markEmailTouched(): void {
    this.$emailTouched.set(true);
  }

  protected markPasswordTouched(): void {
    this.$passwordTouched.set(true);
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();

    if (!this.$canSubmit()) {
      return;
    }

    this.$error.set(null);

    const result = await this.adminAuthService.signIn({
      email: this.$email().trim(),
      password: this.$password(),
    });

    if (result.kind === 'signed-in') {
      this.$password.set('');
      await this.router.navigateByUrl(this.getSafeReturnUrl());
      return;
    }

    if (result.kind === 'invalid-credentials') {
      this.$password.set('');
    }

    this.$error.set(result.kind);
    setTimeout(() => this.$errorSummary()?.nativeElement.focus());
  }

  private getSafeReturnUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl')?.trim();

    if (
      returnUrl === undefined ||
      returnUrl.length === 0 ||
      !returnUrl.startsWith('/') ||
      returnUrl.startsWith('//') ||
      returnUrl === '/admin/login' ||
      returnUrl.startsWith('/admin/login?') ||
      returnUrl.startsWith('/admin/login#')
    ) {
      return '/admin';
    }

    return returnUrl;
  }

  private getErrorMessage(error: AdminLoginErrorKind): string;
  private getErrorMessage(error: AdminLoginErrorKind | null): string | null;
  private getErrorMessage(error: AdminLoginErrorKind | null): string | null {
    switch (error) {
      case 'invalid-credentials':
        return $localize`:Admin login invalid credentials error|Shown when admin sign-in credentials are rejected@@adminLoginInvalidCredentialsError:We could not sign you in with those details. Check your email and password, then try again.`;
      case 'not-admin':
        return $localize`:Admin login not admin error|Shown when a non-admin account tries to access admin tools@@adminLoginNotAdminError:That account does not have access to the admin tools. Use an admin account, or ask the site owner to check your access.`;
      case 'network-failure':
        return $localize`:Admin login network failure error|Shown when the sign-in request cannot reach the server@@adminLoginNetworkFailureError:We could not reach the sign-in service. Check your connection and try again.`;
      case 'unexpected-failure':
        return $localize`:Admin login unexpected failure error|Shown when sign-in fails in an unexpected recoverable way@@adminLoginUnexpectedFailureError:Something went wrong while signing you in. Please try again.`;
      case null:
        return null;
    }
  }

  private getInputValue(event: Event): string {
    return event.target instanceof HTMLInputElement ? event.target.value : '';
  }
}
