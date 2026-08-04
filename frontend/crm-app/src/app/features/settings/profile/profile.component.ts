import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { User } from '../../../core/models';
import { UserManagementService } from '../../../core/services/user-management.service';
import { environment } from '../../../../environments/environment';

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
  isUploadingAvatar = false;
  avatarError = '';

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

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.avatarError = 'Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.';
      input.value = '';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.avatarError = 'Ảnh vượt quá dung lượng cho phép (5MB).';
      input.value = '';
      return;
    }

    this.avatarError = '';
    this.isUploadingAvatar = true;
    this.authService.uploadAvatar(file).subscribe({
      next: (user) => {
        this.currentUser = user;
        this.isUploadingAvatar = false;
        input.value = '';
        this.toast.success('Cập nhật ảnh đại diện thành công');
      },
      error: (err) => {
        this.isUploadingAvatar = false;
        this.avatarError = err.error?.message || 'Upload ảnh thất bại. Vui lòng thử lại.';
        input.value = '';
      }
    });
  }

  resolveUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    const origin = (environment.apiUrl || '').replace(/\/api\/?$/, '');
    return origin + (path.startsWith('/') ? path : '/' + path);
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
