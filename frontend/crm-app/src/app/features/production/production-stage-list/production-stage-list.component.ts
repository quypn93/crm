import { Component, OnInit } from '@angular/core';
import { ProductionService, ProductionStage } from '../../../core/services/production.service';

@Component({
  selector: 'app-production-stage-list',
  templateUrl: './production-stage-list.component.html',
  styleUrls: ['./production-stage-list.component.scss']
})
export class ProductionStageListComponent implements OnInit {
  stages: ProductionStage[] = [];
  isLoading = true;
  errorMessage = '';
  deletingId: string | null = null;

  constructor(private productionService: ProductionService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.productionService.getStages().subscribe({
      next: (data) => { this.stages = data; this.isLoading = false; },
      error: () => { this.errorMessage = 'Không thể tải danh sách khâu.'; this.isLoading = false; }
    });
  }

  deleteStage(stage: ProductionStage): void {
    if (!confirm(`Xoá khâu "${stage.stageName}"?`)) return;
    this.deletingId = stage.id;
    this.productionService.deleteStage(stage.id).subscribe({
      next: () => { this.deletingId = null; this.load(); },
      error: () => { this.deletingId = null; alert('Không thể xoá khâu này.'); }
    });
  }

  getRoleLabel(role?: string): string {
    return this.productionService.getRoleLabel(role);
  }
}
