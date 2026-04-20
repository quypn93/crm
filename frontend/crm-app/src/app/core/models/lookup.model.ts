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
