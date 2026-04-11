import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CustomerService, Customer, CustomerSearchParams } from '../../../core/services/customer.service';

@Component({
  selector: 'app-customer-list',
  templateUrl: './customer-list.component.html',
  styleUrls: ['./customer-list.component.scss']
})
export class CustomerListComponent implements OnInit {
  customers: Customer[] = [];
  isLoading = false;
  searchTerm = '';
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  constructor(
    private customerService: CustomerService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading = true;
    const params: CustomerSearchParams = {
      searchTerm: this.searchTerm,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.customerService.getCustomers(params).subscribe({
      next: (response) => {
        this.customers = response?.items || [];
        this.totalItems = response?.totalCount || 0;
        this.totalPages = response?.totalPages || 0;
        this.isLoading = false;
      },
      error: () => {
        this.customers = [];
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadCustomers();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadCustomers();
  }

  viewCustomer(id: string): void {
    this.router.navigate(['/customers', id]);
  }

  editCustomer(id: string): void {
    this.router.navigate(['/customers', id, 'edit']);
  }

  deleteCustomer(customer: Customer): void {
    if (confirm(`Bạn có chắc muốn xóa khách hàng "${customer.name}"?`)) {
      this.customerService.deleteCustomer(customer.id).subscribe({
        next: () => {
          this.loadCustomers();
        }
      });
    }
  }

  addCustomer(): void {
    this.router.navigate(['/customers/new']);
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
}
