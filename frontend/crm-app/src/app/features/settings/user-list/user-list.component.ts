import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  UserManagementService,
  UserListItem,
  UserSearchParams,
  RoleItem
} from '../../../core/services/user-management.service';
import { AuthService, RoleNames } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  users: UserListItem[] = [];
  roles: RoleItem[] = [];
  isLoading = false;
  // Admin: quản lý mọi user. Trưởng phòng: chỉ thấy/thao tác nhân viên phòng mình.
  isAdmin = false;
  manageableRoles: string[] = [];

  searchTerm = '';
  filterRole = '';
  filterActive: string = '';

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  constructor(
    private userService: UserManagementService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.hasAnyRole([RoleNames.Admin]);
    if (this.isAdmin) {
      this.loadRoles(); // getRoles là API riêng của Admin
    } else {
      // Trưởng phòng chỉ xem nhân viên thuộc phòng mình (lọc phía client theo vai trò).
      this.manageableRoles = this.authService.getManageableStaffRoles();
    }
    this.loadUsers();
  }

  loadRoles(): void {
    this.userService.getRoles().subscribe({
      next: (roles) => (this.roles = roles)
    });
  }

  loadUsers(): void {
    this.isLoading = true;
    const params: UserSearchParams = {
      searchTerm: this.searchTerm || undefined,
      role: this.filterRole || undefined,
      isActive: this.filterActive === '' ? undefined : this.filterActive === 'true',
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.userService.getUsers(params).subscribe({
      next: (result) => {
        let items = result.items;
        if (!this.isAdmin) {
          items = items.filter(u => (u.roles || []).some(r => this.manageableRoles.includes(r)));
        }
        this.users = items;
        this.totalItems = this.isAdmin ? result.totalCount : items.length;
        this.totalPages = this.isAdmin ? result.totalPages : 1;
        this.isLoading = false;
      },
      error: () => {
        this.users = [];
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadUsers();
  }

  addUser(): void {
    this.router.navigate(['/settings/users/new']);
  }

  editUser(id: string): void {
    this.router.navigate(['/settings/users', id, 'edit']);
  }

  toggleActive(user: UserListItem): void {
    const action = user.isActive ? 'vô hiệu hóa' : 'kích hoạt';
    if (confirm(`Bạn có chắc muốn ${action} tài khoản "${user.fullName}"?`)) {
      this.userService.toggleActive(user.id).subscribe({
        next: () => this.loadUsers()
      });
    }
  }

  deleteUser(user: UserListItem): void {
    if (confirm(`Bạn có chắc muốn xóa tài khoản "${user.fullName}"? Hành động này không thể hoàn tác.`)) {
      this.userService.deleteUser(user.id).subscribe({
        next: () => this.loadUsers()
      });
    }
  }

  getRoleLabel(role: string): string {
    return this.userService.getRoleLabel(role);
  }

  getRoleBadgeClass(role: string): string {
    return this.userService.getRoleBadgeClass(role);
  }

  getPages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  getInitials(user: UserListItem): string {
    return (user.firstName.charAt(0) + user.lastName.charAt(0)).toUpperCase();
  }
}
