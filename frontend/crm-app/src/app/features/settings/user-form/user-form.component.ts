import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  UserManagementService,
  RoleItem,
  CreateUserDto,
  UpdateUserDto
} from '../../../core/services/user-management.service';
import { AuthService, RoleNames } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss']
})
export class UserFormComponent implements OnInit {
  userForm: FormGroup;
  isLoading = false;
  isSaving = false;
  isEditMode = false;
  userId: string | null = null;
  errorMessage = '';
  roles: RoleItem[] = [];
  selectedRoles: Set<string> = new Set();
  // Admin: chọn mọi vai trò. Trưởng phòng: chỉ chọn được staff role của phòng mình.
  isAdmin = false;
  manageableRoles: string[] = [];

  constructor(
    private fb: FormBuilder,
    private userService: UserManagementService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.userForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      phoneNumber: [''],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.isAdmin = this.authService.hasAnyRole([RoleNames.Admin]);
    if (this.isAdmin) {
      this.loadRoles(); // getRoles là API riêng của Admin
    } else {
      // Trưởng phòng: chỉ hiển thị staff role của phòng mình; nếu chỉ 1 thì chọn sẵn.
      this.manageableRoles = this.authService.getManageableStaffRoles();
      this.roles = this.manageableRoles.map(r => ({ id: r, name: r } as RoleItem));
      if (this.manageableRoles.length === 1) this.selectedRoles = new Set(this.manageableRoles);
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.userId = id;
      // In edit mode, password is not required
      this.userForm.get('password')?.clearValidators();
      this.userForm.get('email')?.disable();
      this.userForm.get('password')?.updateValueAndValidity();
      this.loadUser();
    }
  }

  loadRoles(): void {
    this.userService.getRoles().subscribe({
      next: (roles) => (this.roles = roles)
    });
  }

  loadUser(): void {
    if (!this.userId) return;
    this.isLoading = true;
    this.userService.getUser(this.userId).subscribe({
      next: (user) => {
        this.userForm.patchValue({
          firstName: user.firstName,
          lastName: user.lastName,
          phoneNumber: user.phoneNumber,
          isActive: user.isActive
        });
        this.selectedRoles = this.isAdmin
          ? new Set(user.roles)
          : new Set(user.roles.filter(r => this.manageableRoles.includes(r)));
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin người dùng';
        this.isLoading = false;
      }
    });
  }

  toggleRole(roleName: string): void {
    if (this.selectedRoles.has(roleName)) {
      this.selectedRoles.delete(roleName);
    } else {
      this.selectedRoles.add(roleName);
    }
  }

  isRoleSelected(roleName: string): boolean {
    return this.selectedRoles.has(roleName);
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode && this.userId) {
      const dto: UpdateUserDto = {
        firstName: this.userForm.value.firstName,
        lastName: this.userForm.value.lastName,
        phoneNumber: this.userForm.value.phoneNumber || undefined,
        isActive: this.userForm.value.isActive,
        roles: Array.from(this.selectedRoles)
      };
      this.userService.updateUser(this.userId, dto).subscribe({
        next: () => this.router.navigate(['/settings/users']),
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Cập nhật thất bại. Vui lòng thử lại.';
        }
      });
    } else {
      const dto: CreateUserDto = {
        email: this.userForm.value.email,
        password: this.userForm.value.password,
        firstName: this.userForm.value.firstName,
        lastName: this.userForm.value.lastName,
        phoneNumber: this.userForm.value.phoneNumber || undefined,
        roles: Array.from(this.selectedRoles)
      };
      this.userService.createUser(dto).subscribe({
        next: () => this.router.navigate(['/settings/users']),
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Tạo người dùng thất bại. Vui lòng thử lại.';
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/settings/users']);
  }

  getRoleLabel(roleName: string): string {
    return this.userService.getRoleLabel(roleName);
  }

  getRoleBadgeClass(roleName: string): string {
    return this.userService.getRoleBadgeClass(roleName);
  }

  get f() {
    return this.userForm.controls;
  }
}
