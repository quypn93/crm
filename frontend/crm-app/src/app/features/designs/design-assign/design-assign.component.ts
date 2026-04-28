import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { DesignService, ColorFabric, CreateDesignAssignmentDto, DesignStatus, DesignStatusLabels } from '../../../core/services/design.service';
import { SettingsService } from '../../../core/services/settings.service';
import { UserManagementService, UserListItem } from '../../../core/services/user-management.service';
import { ToastService } from '../../../core/services/toast.service';
import { LookupItem } from '../../../core/models/lookup.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-design-assign',
  templateUrl: './design-assign.component.html',
  styleUrls: ['./design-assign.component.scss']
})
export class DesignAssignComponent implements OnInit {
  form: FormGroup;
  isSubmitting = false;
  isLoading = false;
  errorMessage = '';

  // Edit mode
  editMode = false;
  designId: string | null = null;
  currentStatus: DesignStatus | null = null;
  completedImageUrl: string | null = null;
  readonly statusLabels = DesignStatusLabels;
  readonly DesignStatus = DesignStatus;

  shirtForms: LookupItem[] = [];
  colorFabrics: ColorFabric[] = [];
  designers: UserListItem[] = [];

  chestLogoUploading = false;
  backLogoUploading = false;
  chestLogoError = '';
  backLogoError = '';

  constructor(
    private fb: FormBuilder,
    private designService: DesignService,
    private settingsService: SettingsService,
    private userService: UserManagementService,
    private toast: ToastService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.form = this.fb.group({
      designName: ['', Validators.required],
      customerFullName: [''],
      shirtFormId: ['', Validators.required],
      colorFabricId: ['', Validators.required],
      accentColorFabricId: [''],
      chestLogoUrl: [''],
      backLogoUrl: [''],
      assignedToUserId: ['', Validators.required],
      assignmentNotes: ['']
    });
  }

  ngOnInit(): void {
    this.designId = this.route.snapshot.paramMap.get('id');
    this.editMode = !!this.designId;

    forkJoin({
      forms: this.settingsService.getLookups('product-forms'),
      colors: this.designService.getAllColorFabrics(),
      designers: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true, role: 'Designer' })
    }).subscribe({
      next: (res) => {
        this.shirtForms = res.forms || [];
        this.colorFabrics = res.colors || [];
        this.designers = res.designers?.items || [];
        if (this.editMode && this.designId) this.loadExisting(this.designId);
      }
    });
  }

  private loadExisting(id: string): void {
    this.isLoading = true;
    this.designService.getDesign(id).subscribe({
      next: (d) => {
        this.currentStatus = d.status;
        this.completedImageUrl = d.completedImageUrl || null;
        this.form.patchValue({
          designName: d.designName || '',
          customerFullName: d.customerFullName || '',
          shirtFormId: d.shirtFormId || '',
          colorFabricId: d.colorFabricId || '',
          accentColorFabricId: d.accentColorFabricId || '',
          chestLogoUrl: d.chestLogoUrl || '',
          backLogoUrl: d.backLogoUrl || '',
          assignedToUserId: d.assignedToUserId || '',
          assignmentNotes: d.assignmentNotes || ''
        });

        // Khi designer đã hoàn thành — khoá form, chỉ cho xem.
        if (d.status === DesignStatus.Completed) {
          this.form.disable();
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Không tải được thiết kế.';
        this.isLoading = false;
      }
    });
  }

  get readOnly(): boolean { return this.currentStatus === DesignStatus.Completed; }

  onLogoSelected(event: Event, slot: 'chest' | 'back'): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (slot === 'chest') { this.chestLogoUploading = true; this.chestLogoError = ''; }
    else { this.backLogoUploading = true; this.backLogoError = ''; }

    this.designService.uploadImage(file).subscribe({
      next: (url) => {
        if (slot === 'chest') { this.form.get('chestLogoUrl')?.setValue(url); this.chestLogoUploading = false; }
        else { this.form.get('backLogoUrl')?.setValue(url); this.backLogoUploading = false; }
        input.value = '';
      },
      error: (err) => {
        const msg = err?.error?.message || 'Upload lỗi';
        if (slot === 'chest') { this.chestLogoError = msg; this.chestLogoUploading = false; }
        else { this.backLogoError = msg; this.backLogoUploading = false; }
      }
    });
  }

  clearLogo(slot: 'chest' | 'back'): void {
    if (slot === 'chest') this.form.get('chestLogoUrl')?.setValue('');
    else this.form.get('backLogoUrl')?.setValue('');
  }

  resolveUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    const origin = (environment.apiUrl || '').replace(/\/api\/?$/, '');
    return origin + (path.startsWith('/') ? path : '/' + path);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.isSubmitting = true;
    this.errorMessage = '';

    const dto: CreateDesignAssignmentDto = this.form.getRawValue();
    const obs$ = this.editMode && this.designId
      ? this.designService.updateAssignment(this.designId, dto)
      : this.designService.createAssignment(dto);

    obs$.subscribe({
      next: (design) => {
        const msg = this.editMode
          ? `Đã cập nhật thiết kế "${design.designName}".`
          : `Đã giao thiết kế "${design.designName}" cho designer.`;
        this.toast.success(msg);
        this.router.navigate(['/designs', design.id]);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = err?.error?.message || 'Lưu thất bại.';
      }
    });
  }

  cancel(): void { this.router.navigate(['/designs']); }

  get f() { return this.form.controls; }
}
