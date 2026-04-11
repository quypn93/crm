import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { DealService, Deal, StageColumn } from '../../../core/services/deal.service';

@Component({
  selector: 'app-deal-kanban',
  templateUrl: './deal-kanban.component.html',
  styleUrls: ['./deal-kanban.component.scss']
})
export class DealKanbanComponent implements OnInit {
  columns: StageColumn[] = [];
  isLoading = false;
  connectedLists: string[] = [];

  constructor(
    private dealService: DealService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadPipeline();
  }

  loadPipeline(): void {
    this.isLoading = true;
    this.dealService.getPipeline().subscribe({
      next: (data: StageColumn[]) => {
        this.columns = data || [];
        this.connectedLists = this.columns.map((_, i) => `stage-${i}`);
        this.isLoading = false;
      },
      error: () => {
        this.columns = [];
        this.isLoading = false;
      }
    });
  }

  drop(event: CdkDragDrop<Deal[]>, targetColumn: StageColumn): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const deal = event.previousContainer.data[event.previousIndex];

      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

      this.dealService.updateDealStage(deal.id, targetColumn.stage.id).subscribe({
        next: (updatedDeal) => {
          const dealIndex = event.container.data.findIndex((d: Deal) => d.id === deal.id);
          if (dealIndex !== -1) {
            event.container.data[dealIndex] = updatedDeal;
          }
          this.updateColumnTotals();
        },
        error: () => {
          transferArrayItem(
            event.container.data,
            event.previousContainer.data,
            event.currentIndex,
            event.previousIndex
          );
        }
      });
    }
  }

  updateColumnTotals(): void {
    this.columns.forEach(col => {
      col.totalValue = col.deals.reduce((sum, deal) => sum + deal.value, 0);
    });
  }

  addDeal(): void {
    this.router.navigate(['/deals/new']);
  }

  viewDeal(deal: Deal): void {
    this.router.navigate(['/deals', deal.id]);
  }

  editDeal(deal: Deal, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/deals', deal.id, 'edit']);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0
    }).format(value);
  }

  getStageColor(color: string): string {
    const colors: { [key: string]: string } = {
      'blue': '#3b82f6',
      'yellow': '#eab308',
      'orange': '#f97316',
      'purple': '#8b5cf6',
      'green': '#22c55e',
      'red': '#ef4444',
      'gray': '#6b7280'
    };
    return colors[color] || colors['gray'];
  }
}
