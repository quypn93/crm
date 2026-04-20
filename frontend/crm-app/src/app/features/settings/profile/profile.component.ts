import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { User } from '../../../core/models';
import { UserManagementService } from '../../../core/services/user-management.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  currentUser: User | null = null;
  passwordForm: FormGroup;
  isSaving = false;
  errorMessage = '';
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toast: ToastService,
    private userService: UserManagementService
  ) {
    this.passwordForm = this.fb.group(
      {
        currentPassword: ['', [Validators.required]],
        newPassword: ['', [Validators.required, Validators.minLength(6)]],
        confirmNewPassword: ['', [Validators.required]]
      },
      { validators: this.passwordMatchValidator }
    );
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
  }

  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const newPwd = group.get('newPassword')?.value;
    const confirm = group.get('confirmNewPassword')?.value;
    return newPwd && confirm && newPwd !== confirm ? { passwordMismatch: true } : null;
  }

  onSubmit(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    this.authService.changePassword(this.passwordForm.value).subscribe({
      next: () => {
        this.isSaving = false;
        this.toast.success('Đổi mật khẩu thành công');
        this.passwordForm.reset();
      },
      error: (err) => {
        this.isSaving = false;
        this.errorMessage = err.error?.message || 'Đổi mật khẩu thất bại. Vui lòng thử lại.';
      }
    });
  }

  getInitials(): string {
    if (!this.currentUser) return 'U';
    const first = this.currentUser.firstName?.charAt(0) || '';
    const last = this.currentUser.lastName?.charAt(0) || '';
    return (first + last).toUpperCase() || 'U';
  }

  getRoleLabel(roleName: string): string {
    return this.userService.getRoleLabel(roleName);
  }

  getRoleBadgeClass(roleName: string): string {
    return this.userService.getRoleBadgeClass(roleName);
  }

  get f() {
    return this.passwordForm.controls;
  }
}
