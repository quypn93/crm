export enum NotificationType {
  TaskAssigned = 1,
  TaskDueSoon = 2,
  TaskOverdue = 3,
  TaskCompleted = 4,
  TaskReassigned = 5
}

export enum NotificationSeverity {
  Info = 0,
  Success = 1,
  Warning = 2,
  Error = 3
}

export interface Notification {
  id: string;
  type: NotificationType;
  severity: NotificationSeverity;
  title: string;
  message: string;
  link?: string;
  entityType?: string;
  entityId?: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

export interface NotificationListResponse {
  items: Notification[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface NotificationFilter {
  unreadOnly?: boolean;
  page?: number;
  pageSize?: number;
}

export interface RolePreference {
  roleId: string;
  roleName: string;
  type: NotificationType;
  inApp: boolean;
  email: boolean;
  isDefault: boolean;
}

export interface RolePreferenceUpdate {
  roleId: string;
  type: NotificationType;
  inApp: boolean;
  email: boolean;
}

export const NOTIFICATION_TYPE_LABELS: Record<NotificationType, string> = {
  [NotificationType.TaskAssigned]: 'Công việc được giao',
  [NotificationType.TaskDueSoon]: 'Công việc sắp đến hạn',
  [NotificationType.TaskOverdue]: 'Công việc quá hạn',
  [NotificationType.TaskCompleted]: 'Công việc đã hoàn thành',
  [NotificationType.TaskReassigned]: 'Công việc bị chuyển'
};
