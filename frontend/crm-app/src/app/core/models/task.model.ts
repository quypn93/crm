export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Urgent = 4
}

export enum TaskStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3
}

export interface Task {
  id: string;
  title: string;
  description?: string;
  dueDate?: Date;
  reminderDate?: Date;
  priority: TaskPriority;
  status: TaskStatus;
  customerId?: string;
  customerName?: string;
  dealId?: string;
  dealTitle?: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  createdByUserId: string;
  completedAt?: Date;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  dueDate?: Date;
  reminderDate?: Date;
  priority: TaskPriority;
  customerId?: string;
  dealId?: string;
  assignedToUserId?: string;
}

export interface UpdateTaskRequest extends CreateTaskRequest {
  id: string;
  status: TaskStatus;
}

export interface TaskFilter {
  search?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  customerId?: string;
  dealId?: string;
  assignedTo?: string;
  dueDateFrom?: Date;
  dueDateTo?: Date;
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export const TaskPriorityLabels: { [key in TaskPriority]: string } = {
  [TaskPriority.Low]: 'Thấp',
  [TaskPriority.Medium]: 'Trung bình',
  [TaskPriority.High]: 'Cao',
  [TaskPriority.Urgent]: 'Khẩn cấp'
};

export const TaskStatusLabels: { [key in TaskStatus]: string } = {
  [TaskStatus.Pending]: 'Chờ xử lý',
  [TaskStatus.InProgress]: 'Đang thực hiện',
  [TaskStatus.Completed]: 'Hoàn thành',
  [TaskStatus.Cancelled]: 'Đã hủy'
};
