import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DesignService, CreateDesignDto, UpdateDesignDto, ColorFabric } from '../../../core/services/design.service';
import { DesignCanvasComponent, DesignCanvasData } from '../design-canvas/design-canvas.component';

@Component({
  selector: 'app-design-form',
  templateUrl: './design-form.component.html',
  styleUrls: ['./design-form.component.scss']
})
export class DesignFormComponent implements OnInit {
  @ViewChild('designCanvas') designCanvasComponent!: DesignCanvasComponent;

  form: FormGroup;
  isEditMode = false;
  isLoading = false;
  isSaving = false;
  designId: string | null = null;
  colorFabrics: ColorFabric[] = [];

  // Canvas data
  initialDesignData: string | null = null;
  initialSelectedComponents: string | null = null;
  currentDesignData: string = '';
  currentSelectedComponents: string = '';

  constructor(
    private fb: FormBuilder,
    private designService: DesignService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      designName: ['', Validators.required],
      designer: [''],
      customerFullName: [''],
      colorFabricId: [''],
      orderId: [''],
      total: [null],
      sizeMan: [''],
      sizeWomen: [''],
      sizeKid: [''],
      oversized: [''],
      finishedDate: [''],
      noteConfection: [''],
      noteOldCodeOrder: [''],
      noteAttachTagLabel: [''],
      noteOther: [''],
      saleStaff: ['']
    });
  }

  ngOnInit(): void {
    this.loadColorFabrics();

    this.designId = this.route.snapshot.paramMap.get('id');
    if (this.designId) {
      this.isEditMode = true;
      this.loadDesign(this.designId);
    }
  }

  loadColorFabrics(): void {
    this.designService.getAllColorFabrics().subscribe({
      next: (fabrics) => {
        this.colorFabrics = fabrics;
      }
    });
  }

  loadDesign(id: string): void {
    this.isLoading = true;
    this.designService.getDesign(id).subscribe({
      next: (design) => {
        this.form.patchValue({
          designName: design.designName,
          designer: design.designer,
          customerFullName: design.customerFullName,
          colorFabricId: design.colorFabricId,
          orderId: design.orderId,
          total: design.total,
          sizeMan: design.sizeMan,
          sizeWomen: design.sizeWomen,
          sizeKid: design.sizeKid,
          oversized: design.oversized,
          finishedDate: design.finishedDate ? design.finishedDate.substring(0, 10) : '',
          noteConfection: design.noteConfection,
          noteOldCodeOrder: design.noteOldCodeOrder,
          noteAttachTagLabel: design.noteAttachTagLabel,
          noteOther: design.noteOther,
          saleStaff: design.saleStaff
        });

        // Set canvas data
        this.initialDesignData = design.designData || null;
        this.initialSelectedComponents = design.selectedComponents || null;
        this.currentDesignData = design.designData || '';
        this.currentSelectedComponents = design.selectedComponents || '';

        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.router.navigate(['/designs']);
      }
    });
  }

  onDesignDataChange(data: DesignCanvasData): void {
    this.currentDesignData = data.canvasJson;
    this.currentSelectedComponents = JSON.stringify(data.selectedComponents);
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isSaving = true;
    const formValue = this.form.value;

    if (this.isEditMode && this.designId) {
      const dto: UpdateDesignDto = {
        id: this.designId,
        ...formValue,
        designData: this.currentDesignData,
        selectedComponents: this.currentSelectedComponents,
        colorFabricId: formValue.colorFabricId || null,
        orderId: formValue.orderId || null,
        finishedDate: formValue.finishedDate || null
      };

      this.designService.updateDesign(this.designId, dto).subscribe({
        next: () => {
          this.isSaving = false;
          this.router.navigate(['/designs', this.designId]);
        },
        error: () => {
          this.isSaving = false;
        }
      });
    } else {
      const dto: CreateDesignDto = {
        ...formValue,
        designData: this.currentDesignData,
        selectedComponents: this.currentSelectedComponents,
        colorFabricId: formValue.colorFabricId || null,
        orderId: formValue.orderId || null,
        finishedDate: formValue.finishedDate || null
      };

      this.designService.createDesign(dto).subscribe({
        next: (design) => {
          this.isSaving = false;
          this.router.navigate(['/designs', design.id]);
        },
        error: () => {
          this.isSaving = false;
        }
      });
    }
  }

  downloadDesign(): void {
    if (this.designCanvasComponent) {
      this.designCanvasComponent.downloadDesign();
    }
  }

  cancel(): void {
    if (this.isEditMode && this.designId) {
      this.router.navigate(['/designs', this.designId]);
    } else {
      this.router.navigate(['/designs']);
    }
  }
}
