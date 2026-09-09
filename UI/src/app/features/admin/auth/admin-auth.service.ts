import { HttpClient, HttpErrorResponse, type HttpResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const adminAuthApiPath = '/api/admin/auth';

export interface AdminSignInCredentials {
  email: string;
  password: string;
}

export type AdminSignInResult =
  | { kind: 'signed-in' }
  | { kind: 'invalid-credentials' }
  | { kind: 'not-admin' }
  | { kind: 'network-failure' }
  | { kind: 'unexpected-failure' };

type AdminAuthFailureResult = { kind: 'network-failure' } | { kind: 'unexpected-failure' };

export type AdminSignOutResult = { kind: 'signed-out' } | AdminAuthFailureResult;

@Injectable({ providedIn: 'root' })
export class AdminAuthService {
  private readonly http = inject(HttpClient);
  private readonly $authenticated = signal(false);
  private readonly $signingIn = signal(false);
  private readonly $signingOut = signal(false);

  readonly $isAuthenticated = this.$authenticated.asReadonly();
  readonly $isSigningIn = this.$signingIn.asReadonly();
  readonly $isSigningOut = this.$signingOut.asReadonly();

  async signIn(credentials: AdminSignInCredentials): Promise<AdminSignInResult> {
    this.$signingIn.set(true);

    try {
      const response = await firstValueFrom(
        this.http.post<unknown>(`${adminAuthApiPath}/sign-in`, credentials, {
          observe: 'response',
          withCredentials: true,
        }),
      );
      const result = this.mapSignInSuccess(response);

      this.$authenticated.set(result.kind === 'signed-in');

      return result;
    } catch (error: unknown) {
      const result = this.mapSignInError(error);

      this.$authenticated.set(false);

      return result;
    } finally {
      this.$signingIn.set(false);
    }
  }

  async signOut(): Promise<AdminSignOutResult> {
    this.$signingOut.set(true);

    try {
      const response = await firstValueFrom(
        this.http.post<unknown>(
          `${adminAuthApiPath}/sign-out`,
          {},
          {
            observe: 'response',
            withCredentials: true,
          },
        ),
      );
      const result = this.mapSignOutSuccess(response);

      if (result.kind === 'signed-out') {
        this.markUnauthenticated();
      }

      return result;
    } catch (error: unknown) {
      return this.mapSharedFailure(error);
    } finally {
      this.$signingOut.set(false);
    }
  }

  markUnauthenticated(): void {
    this.$authenticated.set(false);
  }

  private mapSignInSuccess(response: HttpResponse<unknown>): AdminSignInResult {
    if (response.status === 204) {
      return { kind: 'signed-in' };
    }

    return { kind: 'unexpected-failure' };
  }

  private mapSignOutSuccess(response: HttpResponse<unknown>): AdminSignOutResult {
    if (response.status === 204) {
      return { kind: 'signed-out' };
    }

    return { kind: 'unexpected-failure' };
  }

  private mapSignInError(error: unknown): AdminSignInResult {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 401) {
        return { kind: 'invalid-credentials' };
      }

      if (error.status === 403) {
        return { kind: 'not-admin' };
      }
    }

    return this.mapSharedFailure(error);
  }

  private mapSharedFailure(error: unknown): AdminAuthFailureResult {
    if (error instanceof HttpErrorResponse && error.status === 0) {
      return { kind: 'network-failure' };
    }

    return { kind: 'unexpected-failure' };
  }
}
