export interface LookupItem {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CreateLookupItem {
  name: string;
  description?: string;
  isActive?: boolean;
}

export interface Collection {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  materialIds: string[];
  colorFabricIds: string[];
  formIds: string[];
  specificationIds: string[];
}

export interface CreateCollection {
  name: string;
  description?: string;
  isActive?: boolean;
  materialIds: string[];
  colorFabricIds: string[];
  formIds: string[];
  specificationIds: string[];
}

export interface SenderAddress {
  id: string;
  name: string;
  phone: string;
  address: string;
  provinceId: number;
  districtId: number;
  wardId: number;
  provinceName?: string;
  districtName?: string;
  wardName?: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface CreateSenderAddress {
  name: string;
  phone: string;
  address: string;
  provinceId: number;
  districtId: number;
  wardId: number;
  provinceName?: string;
  districtName?: string;
  wardName?: string;
  isDefault: boolean;
  isActive?: boolean;
}

// Danh mục hành chính Viettel Post (PROVINCE_ID/DISTRICT_ID/WARDS_ID...).
export interface VtpCategory {
  PROVINCE_ID?: number;
  DISTRICT_ID?: number;
  WARDS_ID?: number;
  PROVINCE_NAME?: string;
  DISTRICT_NAME?: string;
  WARDS_NAME?: string;
}

export interface ProductionDaysOption {
  id: string;
  name: string;
  days: number;
  isActive: boolean;
}

export interface CreateProductionDaysOption {
  name: string;
  days: number;
  isActive?: boolean;
}

export interface DepositTransaction {
  id: string;
  code: string;
  amount: number;
  bankName: string;
  accountNumber?: string;
  description?: string;
  transactionDate: string;
  source: string;
  externalId?: string;
  matchedOrderId?: string;
  createdAt: string;
}

export interface CreateDepositTransaction {
  code: string;
  amount: number;
  bankName?: string;
  accountNumber?: string;
  description?: string;
  transactionDate?: string;
}
