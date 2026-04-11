import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DesignService, Design, DesignFilter } from '../../../core/services/design.service';

@Component({
  selector: 'app-design-list',
  templateUrl: './design-list.component.html',
  styleUrls: ['./design-list.component.scss']
})
export class DesignListComponent implements OnInit {
  designs: Design[] = [];
  isLoading = false;
  searchTerm = '';
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  constructor(
    private designService: DesignService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDesigns();
  }

  loadDesigns(): void {
    this.isLoading = true;
    const params: DesignFilter = {
      search: this.searchTerm,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.designService.getDesigns(params).subscribe({
      next: (response) => {
        this.designs = response?.items || [];
        this.totalItems = response?.totalCount || 0;
        this.totalPages = response?.totalPages || 0;
        this.isLoading = false;
      },
      error: () => {
        this.designs = [];
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadDesigns();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadDesigns();
  }

  viewDesign(id: string): void {
    this.router.navigate(['/designs', id]);
  }

  editDesign(id: string): void {
    this.router.navigate(['/designs', id, 'edit']);
  }

  deleteDesign(design: Design): void {
    if (confirm(`Ban co chac muon xoa thiet ke "${design.designName}"?`)) {
      this.designService.deleteDesign(design.id).subscribe({
        next: () => {
          this.loadDesigns();
        }
      });
    }
  }

  duplicateDesign(design: Design): void {
    this.designService.duplicateDesign(design.id, {}).subscribe({
      next: (newDesign) => {
        this.router.navigate(['/designs', newDesign.id, 'edit']);
      }
    });
  }

  addDesign(): void {
    this.router.navigate(['/designs/new']);
  }

  manageColorFabrics(): void {
    this.router.navigate(['/designs/color-fabrics']);
  }

  manageShirtComponents(): void {
    this.router.navigate(['/designs/shirt-components']);
  }

  getPages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }
}
