import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductionService, ProductionStage } from '../../../core/services/production.service';

@Component({
  selector: 'app-production-stage-form',
  templateUrl: './production-stage-form.component.html',
  styleUrls: ['./production-stage-form.component.scss']
})
export class ProductionStageFormComponent implements OnInit {
  form!: FormGroup;
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  isEditMode = false;
  stageId = '';

  roles = [
    { value: '', label: 'Tất cả (không giới hạn)' },
    { value: 'Admin', label: 'Quản trị viên' },
    { value: 'ProductionManager', label: 'Quản lý sản xuất' },
    { value: 'QualityControl', label: 'Kiểm tra chất lượng' },
    { value: 'Cutter', label: 'Thợ cắt' },
    { value: 'Sewer', label: 'Thợ may' },
    { value: 'Printer', label: 'Thợ in / thêu' },
    { value: 'Finisher', label: 'Hoàn thiện' },
    { value: 'Packer', label: 'Đóng gói' },
  ];

  constructor(
    private fb: FormBuilder,
    private productionService: ProductionService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      stageOrder: [1, [Validators.required, Validators.min(1)]],
      stageName: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      responsibleRole: [''],
      isActive: [true]
    });

    this.stageId = this.route.snapshot.paramMap.get('id') || '';
    this.isEditMode = !!this.stageId;

    if (this.isEditMode) {
      this.loadStage();
    }
  }

  loadStage(): void {
    this.isLoading = true;
    this.productionService.getStages().subscribe({
      next: (stages) => {
        const stage = stages.find(s => s.id === this.stageId);
        if (stage) {
          this.form.patchValue(stage);
        } else {
          this.errorMessage = 'Không tìm thấy khâu sản xuất.';
        }
        this.isLoading = false;
      },
      error: () => { this.errorMessage = 'Không thể tải thông tin.'; this.isLoading = false; }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isSaving = true;
    this.errorMessage = '';

    const dto = this.form.value;

    const obs = this.isEditMode
      ? this.productionService.updateStage(this.stageId, dto)
      : this.productionService.createStage(dto);

    obs.subscribe({
      next: () => this.router.navigate(['/production/stages']),
      error: (err) => {
        this.isSaving = false;
        this.errorMessage = err.error?.message || 'Lưu thất bại.';
      }
    });
  }

  get f() { return this.form.controls; }
}
