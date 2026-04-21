import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { User, LoginRequest, RegisterRequest, AuthResponse, RefreshTokenRequest, ChangePasswordRequest } from '../models';

// Role constants matching backend
export const RoleNames = {
  Admin: 'Admin',
  SalesManager: 'SalesManager',
  SalesRep: 'SalesRep',
  ProductionManager: 'ProductionManager',
  ProductionStaff: 'ProductionStaff',
  QualityManager: 'QualityManager',
  QualityControl: 'QualityControl',
  DeliveryManager: 'DeliveryManager',
  DeliveryStaff: 'DeliveryStaff',
  DesignManager: 'DesignManager',
  Designer: 'Designer'
} as const;

// Role groups for permission checks
export const RoleGroups = {
  AllRoles: Object.values(RoleNames),
  SalesRoles: [RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep],
  ManagerRoles: [RoleNames.Admin, RoleNames.SalesManager],
  ProductionRoles: [RoleNames.Admin, RoleNames.ProductionManager, RoleNames.ProductionStaff],
  QualityRoles: [RoleNames.Admin, RoleNames.QualityManager, RoleNames.QualityControl],
  DeliveryRoles: [RoleNames.Admin, RoleNames.DeliveryManager, RoleNames.DeliveryStaff],
  DesignRoles: [RoleNames.Admin, RoleNames.DesignManager, RoleNames.Designer],
  OperationalRoles: [RoleNames.Admin, RoleNames.ProductionManager, RoleNames.ProductionStaff, RoleNames.QualityManager, RoleNames.QualityControl, RoleNames.DeliveryManager, RoleNames.DeliveryStaff]
};

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

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.api.put<void>('auth/change-password', request);
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

  get currentUser(): User | null {
    return this.currentUserSubject.value;
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

  // Permission check methods
  isAdmin(): boolean {
    return this.hasRole(RoleNames.Admin);
  }

  isSalesManager(): boolean {
    return this.hasRole(RoleNames.SalesManager);
  }

  isSalesRep(): boolean {
    return this.hasRole(RoleNames.SalesRep);
  }

  isProductionManager(): boolean {
    return this.hasRole(RoleNames.ProductionManager);
  }

  isQualityControl(): boolean {
    return this.hasRole(RoleNames.QualityControl);
  }

  isDeliveryManager(): boolean {
    return this.hasRole(RoleNames.DeliveryManager);
  }

  // Specific permission checks
  canCreateOrders(): boolean {
    return this.hasAnyRole([RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep]);
  }

  canDeleteOrders(): boolean {
    return this.hasAnyRole([RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep]);
  }

  canUpdatePayment(): boolean {
    return this.hasAnyRole(RoleGroups.ManagerRoles);
  }

  canViewReports(): boolean {
    return this.hasAnyRole(RoleGroups.ManagerRoles);
  }

  canViewSalesPerformance(): boolean {
    return this.hasAnyRole(RoleGroups.ManagerRoles);
  }

  canViewProductionDashboard(): boolean {
    return this.hasAnyRole(RoleGroups.ProductionRoles);
  }

  canViewQCDashboard(): boolean {
    return this.hasAnyRole(RoleGroups.QualityRoles);
  }

  canViewDeliveryDashboard(): boolean {
    return this.hasAnyRole(RoleGroups.DeliveryRoles);
  }

  // Get user's primary role for dashboard routing
  getPrimaryRole(): string | null {
    const user = this.currentUserSubject.value;
    if (!user || !user.roles || user.roles.length === 0) return null;

    // Priority order: Admin > Manager roles > Operational roles
    const priorityOrder = [
      RoleNames.Admin,
      RoleNames.SalesManager,
      RoleNames.ProductionManager,
      RoleNames.QualityControl,
      RoleNames.DeliveryManager,
      RoleNames.SalesRep
    ];

    for (const role of priorityOrder) {
      if (user.roles.includes(role)) {
        return role;
      }
    }

    return user.roles[0];
  }

  getUserRoles(): string[] {
    return this.currentUserSubject.value?.roles ?? [];
  }
}
