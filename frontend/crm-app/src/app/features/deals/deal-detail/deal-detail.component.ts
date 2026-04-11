import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DealService, Deal } from '../../../core/services/deal.service';

@Component({
  selector: 'app-deal-detail',
  templateUrl: './deal-detail.component.html',
  styleUrls: ['./deal-detail.component.scss']
})
export class DealDetailComponent implements OnInit {
  deal: Deal | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private dealService: DealService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDeal(id);
    }
  }

  loadDeal(id: string): void {
    this.isLoading = true;
    this.dealService.getDeal(id).subscribe({
      next: (deal) => {
        this.deal = deal;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin giao dịch';
        this.isLoading = false;
      }
    });
  }

  editDeal(): void {
    if (this.deal) {
      this.router.navigate(['/deals', this.deal.id, 'edit']);
    }
  }

  deleteDeal(): void {
    if (this.deal && confirm(`Bạn có chắc muốn xóa giao dịch "${this.deal.title}"?`)) {
      this.dealService.deleteDeal(this.deal.id).subscribe({
        next: () => {
          this.router.navigate(['/deals']);
        }
      });
    }
  }

  markAsWon(): void {
    if (this.deal && confirm('Xác nhận giao dịch này đã thành công?')) {
      this.dealService.markAsWon(this.deal.id).subscribe({
        next: (updatedDeal) => {
          this.deal = updatedDeal;
        }
      });
    }
  }

  markAsLost(): void {
    if (this.deal && confirm('Xác nhận giao dịch này đã thất bại?')) {
      this.dealService.markAsLost(this.deal.id).subscribe({
        next: (updatedDeal) => {
          this.deal = updatedDeal;
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/deals']);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      maximumFractionDigits: 0
    }).format(value);
  }
}
