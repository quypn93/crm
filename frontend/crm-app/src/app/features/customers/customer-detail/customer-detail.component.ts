import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CustomerService, Customer } from '../../../core/services/customer.service';

@Component({
  selector: 'app-customer-detail',
  templateUrl: './customer-detail.component.html',
  styleUrls: ['./customer-detail.component.scss']
})
export class CustomerDetailComponent implements OnInit {
  customer: Customer | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private customerService: CustomerService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadCustomer(id);
    }
  }

  loadCustomer(id: string): void {
    this.isLoading = true;
    this.customerService.getCustomer(id).subscribe({
      next: (customer) => {
        this.customer = customer;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin khách hàng';
        this.isLoading = false;
      }
    });
  }

  editCustomer(): void {
    if (this.customer) {
      this.router.navigate(['/customers', this.customer.id, 'edit']);
    }
  }

  deleteCustomer(): void {
    if (this.customer && confirm(`Bạn có chắc muốn xóa khách hàng "${this.customer.name}"?`)) {
      this.customerService.deleteCustomer(this.customer.id).subscribe({
        next: () => {
          this.router.navigate(['/customers']);
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/customers']);
  }
}
