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
  createdAt: Date;
  updatedAt?: Date;
  createdByUserId: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
}

export interface CreateCustomerRequest {
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

export interface UpdateCustomerRequest extends CreateCustomerRequest {
  id: string;
}

export interface CustomerFilter {
  search?: string;
  assignedTo?: string;
  isActive?: boolean;
  createdFrom?: Date;
  createdTo?: Date;
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}
