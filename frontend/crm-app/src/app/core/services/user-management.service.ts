import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface UserListItem {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber?: string;
  isActive: boolean;
  roles: string[];
  createdAt: string;
  lastLoginAt?: string;
}

export interface RoleItem {
  id: string;
  name: string;
  description?: string;
}

export interface PagedUsersResult {
  items: UserListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserSearchParams {
  searchTerm?: string;
  role?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CreateUserDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  roles: string[];
}

export interface CreateRoleDto {
  name: string;
  description?: string;
}

export interface UpdateRoleDto {
  description?: string;
}

export interface UpdateUserDto {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  isActive: boolean;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  constructor(private api: ApiService) {}

  getUsers(params: UserSearchParams): Observable<PagedUsersResult> {
    return this.api.get<PagedUsersResult>('users', this.api.buildParams(params));
  }

  getUser(id: string): Observable<UserListItem> {
    return this.api.get<UserListItem>(`users/${id}`);
  }

  createUser(dto: CreateUserDto): Observable<UserListItem> {
    return this.api.post<UserListItem>('users', dto);
  }

  updateUser(id: string, dto: UpdateUserDto): Observable<UserListItem> {
    return this.api.put<UserListItem>(`users/${id}`, dto);
  }

  deleteUser(id: string): Observable<void> {
    return this.api.delete<void>(`users/${id}`);
  }

  toggleActive(id: string): Observable<UserListItem> {
    return this.api.put<UserListItem>(`users/${id}/toggle-active`, {});
  }

  assignRoles(id: string, roles: string[]): Observable<UserListItem> {
    return this.api.put<UserListItem>(`users/${id}/roles`, { roles });
  }

  getRoles(): Observable<RoleItem[]> {
    return this.api.get<RoleItem[]>('users/roles');
  }

  getRole(id: string): Observable<RoleItem> {
    return this.api.get<RoleItem>(`users/roles/${id}`);
  }

  createRole(dto: CreateRoleDto): Observable<RoleItem> {
    return this.api.post<RoleItem>('users/roles', dto);
  }

  updateRole(id: string, dto: UpdateRoleDto): Observable<RoleItem> {
    return this.api.put<RoleItem>(`users/roles/${id}`, dto);
  }

  deleteRole(id: string): Observable<void> {
    return this.api.delete<void>(`users/roles/${id}`);
  }

  getRoleLabel(roleName: string): string {
    const labels: Record<string, string> = {
      'Admin':             'Quản trị viên',
      'SalesManager':      'Trưởng phòng kinh doanh',
      'SalesRep':          'Nhân viên kinh doanh',
      'ProductionManager': 'Quản lý sản xuất',
      'ProductionStaff':   'Nhân viên sản xuất',
      'QualityManager':    'Quản lý chất lượng',
      'QualityControl':    'Nhân viên kiểm tra chất lượng',
      'DeliveryManager':   'Quản lý giao hàng',
      'DeliveryStaff':     'Nhân viên giao hàng',
      'DesignManager':     'Trưởng phòng thiết kế',
      'Designer':          'Nhân viên thiết kế'
    };
    return labels[roleName] || roleName;
  }

  getRoleBadgeClass(roleName: string): string {
    const classes: Record<string, string> = {
      'Admin':             'role-admin',
      'SalesManager':      'role-manager',
      'SalesRep':          'role-sales',
      'ProductionManager': 'role-production',
      'ProductionStaff':   'role-production',
      'QualityManager':    'role-quality',
      'QualityControl':    'role-quality',
      'DeliveryManager':   'role-delivery',
      'DeliveryStaff':     'role-delivery',
      'DesignManager':     'role-design',
      'Designer':          'role-design'
    };
    return classes[roleName] || 'role-default';
  }
}
