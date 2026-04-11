import { Component, OnInit, OnDestroy, ViewChild, ElementRef, Input, Output, EventEmitter, AfterViewInit } from '@angular/core';
import { fabric } from 'fabric';
import { DesignService, ShirtComponent, ColorFabric, ComponentType, ComponentTypeNames } from '../../../core/services/design.service';

export interface SelectedComponents {
  [key: string]: string; // ComponentType key -> imageUrl
}

export interface DesignCanvasData {
  canvasJson: string;
  selectedComponents: SelectedComponents;
}

@Component({
  selector: 'app-design-canvas',
  templateUrl: './design-canvas.component.html',
  styleUrls: ['./design-canvas.component.scss']
})
export class DesignCanvasComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('designCanvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;

  @Input() colorFabricId: string | null = null;
  @Input() initialDesignData: string | null = null;
  @Input() initialSelectedComponents: string | null = null;

  @Output() designDataChange = new EventEmitter<DesignCanvasData>();

  canvas!: fabric.Canvas;
  colorFabrics: ColorFabric[] = [];
  componentsByType: { [key: number]: ShirtComponent[] } = {};
  selectedComponents: SelectedComponents = {};
  componentImages: { [key: string]: fabric.Image } = {};

  componentTypes = Object.entries(ComponentTypeNames).map(([key, label]) => ({
    type: Number(key) as ComponentType,
    label
  }));

  zoomLevel = 1;
  canvasWidth = 800;
  canvasHeight = 600;

  constructor(private designService: DesignService) {}

  ngOnInit(): void {
    this.loadColorFabrics();
  }

  ngAfterViewInit(): void {
    this.initCanvas();

    // Load initial data if provided
    if (this.initialSelectedComponents) {
      try {
        this.selectedComponents = JSON.parse(this.initialSelectedComponents);
      } catch {
        this.selectedComponents = {};
      }
    }

    if (this.initialDesignData) {
      this.loadCanvasFromJson(this.initialDesignData);
    }

    // Load components if colorFabricId is provided
    if (this.colorFabricId) {
      this.loadComponentsByColorFabric(this.colorFabricId);
    }
  }

  ngOnDestroy(): void {
    if (this.canvas) {
      this.canvas.dispose();
    }
  }

  initCanvas(): void {
    this.canvas = new fabric.Canvas(this.canvasRef.nativeElement, {
      width: this.canvasWidth,
      height: this.canvasHeight,
      backgroundColor: '#ffffff',
      preserveObjectStacking: true
    });

    // Enable object selection
    this.canvas.selection = true;

    // Listen for changes
    this.canvas.on('object:modified', () => this.emitDesignData());
    this.canvas.on('object:added', () => this.emitDesignData());
    this.canvas.on('object:removed', () => this.emitDesignData());
  }

  loadColorFabrics(): void {
    this.designService.getAllColorFabrics().subscribe({
      next: (fabrics) => {
        this.colorFabrics = fabrics;
      }
    });
  }

  loadComponentsByColorFabric(colorFabricId: string): void {
    this.designService.getShirtComponentsByColorFabric(colorFabricId).subscribe({
      next: (components) => {
        // Group components by type
        this.componentsByType = {};
        components.forEach(comp => {
          if (!this.componentsByType[comp.type]) {
            this.componentsByType[comp.type] = [];
          }
          this.componentsByType[comp.type].push(comp);
        });

        // Re-apply selected components to canvas
        this.applySelectedComponentsToCanvas();
      }
    });
  }

  onColorFabricChange(colorFabricId: string): void {
    this.colorFabricId = colorFabricId;
    if (colorFabricId) {
      this.loadComponentsByColorFabric(colorFabricId);
    } else {
      this.componentsByType = {};
    }
  }

  onComponentSelect(type: ComponentType, imageUrl: string): void {
    const typeKey = ComponentType[type];
    this.selectedComponents[typeKey] = imageUrl;

    if (imageUrl) {
      this.addImageToCanvas(imageUrl, typeKey);
    } else {
      this.removeImageFromCanvas(typeKey);
    }

    this.emitDesignData();
  }

  getSelectedComponentUrl(type: ComponentType): string {
    const typeKey = ComponentType[type];
    return this.selectedComponents[typeKey] || '';
  }

  addImageToCanvas(src: string, typeKey: string): void {
    fabric.Image.fromURL(src, (img) => {
      if (!img) return;

      const scale = Math.min(
        this.canvasWidth / (img.width || 1),
        this.canvasHeight / (img.height || 1)
      );

      img.scale(scale);
      img.set({
        selectable: false,
        evented: false,
        data: { type: typeKey }
      });

      // Remove existing image of the same type
      if (this.componentImages[typeKey]) {
        this.canvas.remove(this.componentImages[typeKey]);
      }

      this.componentImages[typeKey] = img;
      this.canvas.add(img);
      this.canvas.renderAll();
    }, { crossOrigin: 'anonymous' });
  }

  removeImageFromCanvas(typeKey: string): void {
    if (this.componentImages[typeKey]) {
      this.canvas.remove(this.componentImages[typeKey]);
      delete this.componentImages[typeKey];
      this.canvas.renderAll();
    }
  }

  applySelectedComponentsToCanvas(): void {
    // Clear existing component images
    Object.keys(this.componentImages).forEach(key => {
      this.canvas.remove(this.componentImages[key]);
    });
    this.componentImages = {};

    // Add selected components
    Object.entries(this.selectedComponents).forEach(([typeKey, imageUrl]) => {
      if (imageUrl) {
        this.addImageToCanvas(imageUrl, typeKey);
      }
    });
  }

  addText(): void {
    const text = new fabric.IText('Nhap text', {
      left: 100,
      top: 100,
      fontFamily: 'Arial',
      fill: '#333333',
      fontSize: 40
    });
    this.canvas.add(text);
    this.canvas.setActiveObject(text);
    this.canvas.renderAll();
  }

  zoomIn(): void {
    this.zoomLevel = Math.min(this.zoomLevel * 1.1, 3);
    this.canvas.setZoom(this.zoomLevel);
    this.canvas.renderAll();
  }

  zoomOut(): void {
    this.zoomLevel = Math.max(this.zoomLevel / 1.1, 0.5);
    this.canvas.setZoom(this.zoomLevel);
    this.canvas.renderAll();
  }

  resetZoom(): void {
    this.zoomLevel = 1;
    this.canvas.setZoom(1);
    this.canvas.renderAll();
  }

  deleteSelected(): void {
    const activeObject = this.canvas.getActiveObject();
    if (activeObject) {
      this.canvas.remove(activeObject);
      this.canvas.renderAll();
    }
  }

  loadCanvasFromJson(json: string): void {
    try {
      this.canvas.loadFromJSON(json, () => {
        this.canvas.renderAll();
      });
    } catch (e) {
      console.error('Error loading canvas from JSON:', e);
    }
  }

  getCanvasJson(): string {
    return JSON.stringify(this.canvas.toJSON());
  }

  getSelectedComponentsJson(): string {
    return JSON.stringify(this.selectedComponents);
  }

  emitDesignData(): void {
    this.designDataChange.emit({
      canvasJson: this.getCanvasJson(),
      selectedComponents: this.selectedComponents
    });
  }

  downloadDesign(): void {
    this.generateDesignImage().then(dataUrl => {
      const link = document.createElement('a');
      link.href = dataUrl;
      link.download = 'shirt-design.png';
      link.click();
    });
  }

  async generateDesignImage(): Promise<string> {
    // Create a temporary canvas for the download image
    const tempCanvas = document.createElement('canvas');
    const extraHeight = 300;
    tempCanvas.width = this.canvasWidth + 400;
    tempCanvas.height = this.canvasHeight + extraHeight;

    const ctx = tempCanvas.getContext('2d')!;

    // Set white background
    ctx.fillStyle = '#ffffff';
    ctx.fillRect(0, 0, tempCanvas.width, tempCanvas.height);

    // Draw left border
    ctx.fillStyle = '#b71f39';
    ctx.fillRect(0, 0, 60, tempCanvas.height);

    // Draw the design canvas
    const designDataUrl = this.canvas.toDataURL({
      format: 'png',
      quality: 1.0
    });

    return new Promise((resolve) => {
      const img = new Image();
      img.onload = () => {
        // Draw design centered
        ctx.drawImage(img, 200, 100);

        // Draw separator line
        ctx.strokeStyle = '#b71f39';
        ctx.lineWidth = 2;
        ctx.setLineDash([20, 15]);
        ctx.beginPath();
        ctx.moveTo(0, this.canvasHeight + 50);
        ctx.lineTo(tempCanvas.width, this.canvasHeight + 50);
        ctx.stroke();

        // Draw company info
        ctx.setLineDash([]);
        ctx.fillStyle = '#333333';
        ctx.font = 'bold 16px Arial';
        ctx.fillText('DONG PHUC BON MUA', 80, this.canvasHeight + 100);
        ctx.font = '14px Arial';
        ctx.fillText('So 33 Ngo 102 - Khuat Duy Tien, Nhan Chinh, Thanh Xuan, Ha Noi', 80, this.canvasHeight + 130);
        ctx.fillText('Hotline: 0969.228.488 | Website: www.dongphucbonmua.com', 80, this.canvasHeight + 155);

        // Draw note
        ctx.fillStyle = '#b71f39';
        ctx.font = 'bold 14px Arial';
        ctx.fillText('Chu y: Quy khach vui long kiem tra ky mau ao, noi dung truoc khi san xuat.', 80, this.canvasHeight + 200);
        ctx.fillText('Cong ty se khong chiu bat ky sai sot nao sau khi khach hang da duyet hinh anh.', 80, this.canvasHeight + 225);

        resolve(tempCanvas.toDataURL('image/png'));
      };
      img.src = designDataUrl;
    });
  }
}
