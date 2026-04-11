import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DealService, DealStage } from '../../../core/services/deal.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';

@Component({
  selector: 'app-deal-form',
  templateUrl: './deal-form.component.html',
  styleUrls: ['./deal-form.component.scss']
})
export class DealFormComponent implements OnInit {
  dealForm: FormGroup;
  isLoading = false;
  isEditMode = false;
  dealId: string | null = null;
  errorMessage = '';
  stages: DealStage[] = [];
  customers: Customer[] = [];

  constructor(
    private fb: FormBuilder,
    private dealService: DealService,
    private customerService: CustomerService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.dealForm = this.fb.group({
      title: ['', [Validators.required]],
      value: [0, [Validators.required, Validators.min(0)]],
      customerId: ['', [Validators.required]],
      stageId: [''],
      expectedCloseDate: [''],
      probability: [50, [Validators.min(0), Validators.max(100)]],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.loadStages();
    this.loadCustomers();

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.dealId = id;
      this.loadDeal();
    }
  }

  loadStages(): void {
    this.dealService.getStages().subscribe({
      next: (stages) => {
        this.stages = stages || [];
        if (!this.isEditMode && this.stages.length > 0) {
          const defaultStage = this.stages.find(s => s.isDefault) || this.stages[0];
          this.dealForm.patchValue({ stageId: defaultStage.id });
        }
      }
    });
  }

  loadCustomers(): void {
    this.customerService.getCustomers({ pageSize: 1000 }).subscribe({
      next: (response) => {
        this.customers = response?.items || [];
      }
    });
  }

  loadDeal(): void {
    if (!this.dealId) return;

    this.isLoading = true;
    this.dealService.getDeal(this.dealId).subscribe({
      next: (deal) => {
        this.dealForm.patchValue({
          title: deal.title,
          value: deal.value,
          customerId: deal.customerId,
          stageId: deal.stageId,
          expectedCloseDate: deal.expectedCloseDate ? this.formatDate(new Date(deal.expectedCloseDate)) : '',
          probability: deal.probability,
          notes: deal.notes
        });
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin giao dịch';
        this.isLoading = false;
      }
    });
  }

  formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  onSubmit(): void {
    if (this.dealForm.invalid) {
      this.dealForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const formData = {
      ...this.dealForm.value,
      value: Number(this.dealForm.value.value),
      probability: Number(this.dealForm.value.probability)
    };

    if (this.isEditMode && this.dealId) {
      this.dealService.updateDeal(this.dealId, { ...formData, id: this.dealId }).subscribe({
        next: () => {
          this.router.navigate(['/deals']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Cập nhật thất bại. Vui lòng thử lại.';
        }
      });
    } else {
      this.dealService.createDeal(formData).subscribe({
        next: () => {
          this.router.navigate(['/deals']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Thêm giao dịch thất bại. Vui lòng thử lại.';
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/deals']);
  }

  get f() {
    return this.dealForm.controls;
  }
}
