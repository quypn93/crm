import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { User, LoginRequest, RegisterRequest, AuthResponse, RefreshTokenRequest } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private api: ApiService,
    private storage: StorageService,
    private router: Router
  ) {
    this.loadCurrentUser();
  }

  private loadCurrentUser(): void {
    const token = this.storage.getToken();
    const user = this.storage.getUser<User>();

    if (token && user) {
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
    }
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('auth/login', credentials).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('auth/register', request).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.storage.getRefreshToken();
    const request: RefreshTokenRequest = { refreshToken: refreshToken || '' };

    return this.api.post<AuthResponse>('auth/refresh-token', request).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  logout(): void {
    this.api.post('auth/logout', {}).subscribe({
      complete: () => this.clearAuth()
    });
  }

  private handleAuthResponse(response: AuthResponse): void {
    this.storage.setToken(response.accessToken);
    this.storage.setRefreshToken(response.refreshToken);
    this.storage.setUser(response.user);
    this.currentUserSubject.next(response.user);
    this.isAuthenticatedSubject.next(true);
  }

  private clearAuth(): void {
    this.storage.clear();
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/auth/login']);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.roles?.includes(role) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.currentUserSubject.value;
    return roles.some(role => user?.roles?.includes(role)) ?? false;
  }

  getFullName(): string {
    const user = this.currentUserSubject.value;
    return user ? `${user.firstName} ${user.lastName}` : '';
  }
}
