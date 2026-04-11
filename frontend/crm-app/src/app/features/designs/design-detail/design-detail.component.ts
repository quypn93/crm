import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DesignService, DesignDetail } from '../../../core/services/design.service';
import { DesignCanvasComponent } from '../design-canvas/design-canvas.component';

@Component({
  selector: 'app-design-detail',
  templateUrl: './design-detail.component.html',
  styleUrls: ['./design-detail.component.scss']
})
export class DesignDetailComponent implements OnInit {
  @ViewChild('designCanvas') designCanvasComponent!: DesignCanvasComponent;

  design: DesignDetail | null = null;
  isLoading = true;

  constructor(
    private designService: DesignService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDesign(id);
    }
  }

  loadDesign(id: string): void {
    this.isLoading = true;
    this.designService.getDesign(id).subscribe({
      next: (design) => {
        this.design = design;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.router.navigate(['/designs']);
      }
    });
  }

  editDesign(): void {
    if (this.design) {
      this.router.navigate(['/designs', this.design.id, 'edit']);
    }
  }

  duplicateDesign(): void {
    if (this.design) {
      this.designService.duplicateDesign(this.design.id, {}).subscribe({
        next: (newDesign) => {
          this.router.navigate(['/designs', newDesign.id, 'edit']);
        }
      });
    }
  }

  deleteDesign(): void {
    if (this.design && confirm(`Ban co chac muon xoa thiet ke "${this.design.designName}"?`)) {
      this.designService.deleteDesign(this.design.id).subscribe({
        next: () => {
          this.router.navigate(['/designs']);
        }
      });
    }
  }

  downloadDesign(): void {
    if (this.designCanvasComponent) {
      this.designCanvasComponent.downloadDesign();
    }
  }

  goBack(): void {
    this.router.navigate(['/designs']);
  }

  parseSizeJson(json: string | undefined): { [key: string]: number } | null {
    if (!json) return null;
    try {
      return JSON.parse(json);
    } catch {
      return null;
    }
  }
}
