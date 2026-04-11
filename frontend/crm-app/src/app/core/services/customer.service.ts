import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Customer {
  id: string;
  name: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  postalCode?: string;
  companyName?: string;
  industry?: string;
  website?: string;
  notes?: string;
  isActive: boolean;
  createdByUserId: string;
  createdByUserName?: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CustomerSearchParams {
  searchTerm?: string;
  industry?: string;
  assignedToUserId?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateCustomerDto {
  name: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  country?: string;
  postalCode?: string;
  companyName?: string;
  industry?: string;
  website?: string;
  notes?: string;
  assignedToUserId?: string;
}

export interface UpdateCustomerDto extends CreateCustomerDto {
  id: string;
}

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  constructor(private api: ApiService) {}

  getCustomers(params: CustomerSearchParams): Observable<PagedResult<Customer>> {
    return this.api.get<PagedResult<Customer>>('customers', this.api.buildParams(params));
  }

  getCustomer(id: string): Observable<Customer> {
    return this.api.get<Customer>(`customers/${id}`);
  }

  createCustomer(customer: CreateCustomerDto): Observable<Customer> {
    return this.api.post<Customer>('customers', customer);
  }

  updateCustomer(id: string, customer: UpdateCustomerDto): Observable<Customer> {
    return this.api.put<Customer>(`customers/${id}`, customer);
  }

  deleteCustomer(id: string): Observable<void> {
    return this.api.delete<void>(`customers/${id}`);
  }

  getIndustries(): string[] {
    return ['Công nghệ', 'Giáo dục', 'Khách sạn', 'Thực phẩm', 'Làm đẹp', 'Ngân hàng', 'Y tế', 'Vận tải', 'Khác'];
  }
}
