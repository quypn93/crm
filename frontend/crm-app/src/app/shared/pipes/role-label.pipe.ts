import { Pipe, PipeTransform } from '@angular/core';

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Quản trị viên',
  ProductionManager: 'Quản lý sản xuất',
  Designer: 'Nhà thiết kế',
  QualityControl: 'Kiểm tra chất lượng',
  Cutter: 'Thợ cắt',
  Sewer: 'Thợ may',
  Printer: 'Thợ in / thêu',
  Finisher: 'Hoàn thiện',
  Packer: 'Đóng gói',
  Sale: 'Sale',
};

@Pipe({ name: 'roleLabel' })
export class RoleLabelPipe implements PipeTransform {
  transform(role?: string): string {
    if (!role) return '';
    return ROLE_LABELS[role] ?? role;
  }
}
