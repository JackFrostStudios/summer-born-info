import { Component, ElementRef, OnInit, afterRenderEffect, computed, inject, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormField, form, required, submit, validate } from '@angular/forms/signals';
import { AdminAuthService, type AdminSignInResult } from '../auth/admin-auth.service';

type AdminLoginErrorKind = Exclude<AdminSignInResult['kind'], 'signed-in'>;

interface AdminLoginModel {
  email: string;
  password: string;
}

@Component({
  selector: 'sbi-admin-login',
  imports: [FormField],
  templateUrl: './admin-login.html',
  styleUrl: './admin-login.scss',
})
export class AdminLogin implements OnInit {
  private readonly adminAuthService = inject(AdminAuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
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
        onInvalid: (field) => field().errorSummary()[0]?.fieldTree().focusBoundControl(),
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
  protected readonly headingId = 'admin-login-heading';
  protected readonly $isSubmitting = computed(() => this.$form().submitting());
  protected readonly $canSubmit = computed(
    () => this.$form.email().valid() && this.$form.password().valid() && !this.$isSubmitting(),
  );
  protected readonly $errorMessage = computed(
    () =>
      this.$form()
        .errors()
        .find(
          (error) =>
            error.kind === 'invalid-credentials' ||
            error.kind === 'not-admin' ||
            error.kind === 'network-failure' ||
            error.kind === 'unexpected-failure',
        )?.message ?? null,
  );
  protected readonly $showEmailRequiredError = computed(
    () => this.$form.email().touched() && this.$form.email().invalid(),
  );
  protected readonly $showPasswordRequiredError = computed(
    () => this.$form.password().touched() && this.$form.password().invalid(),
  );

  protected readonly $emailErrorMessage = computed(() =>
    this.$form
      .email()
      .errors()
      .map((error) => error.message)
      .join(' '),
  );
  protected readonly $passwordErrorMessage = computed(() =>
    this.$form
      .password()
      .errors()
      .map((error) => error.message)
      .join(' '),
  );

  constructor() {
    afterRenderEffect(() => {
      if (this.$errorMessage()) {
        this.$errorSummary()?.nativeElement.focus();
      }
    });
  }

  ngOnInit(): void {
    if (this.adminAuthService.$isAuthenticated()) {
      void this.router.navigateByUrl(this.getSafeReturnUrl());
    }
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();

    if (this.$isSubmitting()) {
      return;
    }

    if (this.$errorMessage()) {
      // A fresh root value clears submission errors even when retrying unchanged credentials.
      this.$model.update((model) => ({ ...model }));
    }

    await submit(this.$form);
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

  private getErrorMessage(error: AdminLoginErrorKind): string {
    switch (error) {
      case 'invalid-credentials':
        return $localize`:Admin login invalid credentials error|Shown when admin sign-in credentials are rejected@@adminLoginInvalidCredentialsError:We could not sign you in with those details. Check your email and password, then try again.`;
      case 'not-admin':
        return $localize`:Admin login not admin error|Shown when a non-admin account tries to access admin tools@@adminLoginNotAdminError:That account does not have access to the admin tools. Use an admin account, or ask the site owner to check your access.`;
      case 'network-failure':
        return $localize`:Admin login network failure error|Shown when the sign-in request cannot reach the server@@adminLoginNetworkFailureError:We could not reach the sign-in service. Check your connection and try again.`;
      case 'unexpected-failure':
        return $localize`:Admin login unexpected failure error|Shown when sign-in fails in an unexpected recoverable way@@adminLoginUnexpectedFailureError:Something went wrong while signing you in. Please try again.`;
    }
  }
}
