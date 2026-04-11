import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerService } from '../../../core/services/customer.service';

@Component({
  selector: 'app-customer-form',
  templateUrl: './customer-form.component.html',
  styleUrls: ['./customer-form.component.scss']
})
export class CustomerFormComponent implements OnInit {
  customerForm: FormGroup;
  isLoading = false;
  isEditMode = false;
  customerId: string | null = null;
  errorMessage = '';
  industries: string[] = [];

  constructor(
    private fb: FormBuilder,
    private customerService: CustomerService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.customerForm = this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.email]],
      phone: [''],
      companyName: [''],
      industry: [''],
      address: [''],
      city: [''],
      website: [''],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.industries = this.customerService.getIndustries();

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.customerId = id;
      this.loadCustomer();
    }
  }

  loadCustomer(): void {
    if (!this.customerId) return;

    this.isLoading = true;
    this.customerService.getCustomer(this.customerId).subscribe({
      next: (customer) => {
        this.customerForm.patchValue({
          name: customer.name,
          email: customer.email,
          phone: customer.phone,
          companyName: customer.companyName,
          industry: customer.industry,
          address: customer.address,
          city: customer.city,
          website: customer.website,
          notes: customer.notes
        });
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin khách hàng';
        this.isLoading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.customerForm.invalid) {
      this.customerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const formData = this.customerForm.value;

    if (this.isEditMode && this.customerId) {
      this.customerService.updateCustomer(this.customerId, { ...formData, id: this.customerId }).subscribe({
        next: () => {
          this.router.navigate(['/customers']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Cập nhật thất bại. Vui lòng thử lại.';
        }
      });
    } else {
      this.customerService.createCustomer(formData).subscribe({
        next: () => {
          this.router.navigate(['/customers']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Thêm khách hàng thất bại. Vui lòng thử lại.';
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/customers']);
  }

  get f() {
    return this.customerForm.controls;
  }
}
