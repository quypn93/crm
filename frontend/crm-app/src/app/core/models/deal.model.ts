export interface DealStage {
  id: string;
  name: string;
  order: number;
  color: string;
  probability: number;
  isDefault: boolean;
  isWonStage: boolean;
  isLostStage: boolean;
}

export interface Deal {
  id: string;
  title: string;
  value: number;
  currency: string;
  customerId: string;
  customerName?: string;
  stageId: string;
  stageName?: string;
  stageColor?: string;
  expectedCloseDate?: Date;
  actualCloseDate?: Date;
  probability: number;
  notes?: string;
  lostReason?: string;
  createdAt: Date;
  updatedAt?: Date;
  createdByUserId: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
}

export interface CreateDealRequest {
  title: string;
  value: number;
  currency?: string;
  customerId: string;
  stageId: string;
  expectedCloseDate?: Date;
  notes?: string;
  assignedToUserId?: string;
}

export interface UpdateDealRequest extends CreateDealRequest {
  id: string;
}

export interface UpdateDealStageRequest {
  dealId: string;
  stageId: string;
}

export interface DealFilter {
  search?: string;
  stageId?: string;
  customerId?: string;
  assignedTo?: string;
  minValue?: number;
  maxValue?: number;
  closeDateFrom?: Date;
  closeDateTo?: Date;
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface DealsByStage {
  stage: DealStage;
  deals: Deal[];
  totalValue: number;
  count: number;
}
