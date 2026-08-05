import { AfterViewInit, Component, ElementRef, Input, OnChanges, ViewChild } from '@angular/core';
import { Order } from '../../../core/models/order.model';
import { environment } from '../../../../environments/environment';
import html2canvas from 'html2canvas';

@Component({
  selector: 'app-order-card',
  templateUrl: './order-card.component.html',
  styleUrls: ['./order-card.component.scss']
})
export class OrderCardComponent implements OnChanges, AfterViewInit {
  @Input() order!: Order;
  @ViewChild('cardRef') cardRef!: ElementRef<HTMLElement>;

  isRendering = false;
  qrDataUrl = '';
  branding = environment.branding;
  readonly ADULT_SIZES = ['S', 'M', 'L', 'XL', 'XXL', 'NC', 'TE'];
  readonly CHILD_SIZES = ['NC1', 'NC2', 'NC3'];
  readonly READYMADE_SIZES = ['XS', 'S', 'M', 'L', 'XL', '2XL', '3XL'];

  // File xuất phải luôn dưới 4MB — html2canvas ở scale:2 xuất PNG lossless dễ vượt 5MB.
  private readonly MAX_EXPORT_BYTES = 4 * 1024 * 1024;

  private viewReady = false;

  ngAfterViewInit(): void {
    this.viewReady = true;
    if (this.order) this.generateQr();
  }

  ngOnChanges(): void {
    if (this.viewReady && this.order) this.generateQr();
  }

  resolveImageUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    const origin = (environment.apiUrl || '').replace(/\/api\/?$/, '');
    return origin + (path.startsWith('/') ? path : '/' + path);
  }

  async exportImage(): Promise<void> {
    if (!this.cardRef) return;

    this.isRendering = true;
    try {
      // Chờ webfont (Roboto việt hóa) load xong, tránh html2canvas chụp lúc
      // đang fallback font hệ thống làm vỡ dấu tiếng Việt.
      await document.fonts.ready;

      const el = this.cardRef.nativeElement;
      const wrapper = el.parentElement;
      const prevTransform = el.style.transform;
      const prevMarginBottom = el.style.marginBottom;
      const prevMarginRight = el.style.marginRight;
      const prevWrapperOverflow = wrapper?.style.overflow ?? '';

      el.style.transform = 'scale(1)';
      el.style.marginBottom = '0';
      el.style.marginRight = '0';
      if (wrapper) wrapper.style.overflow = 'visible';

      const canvas = await html2canvas(el, {
        scale: 2,
        useCORS: true,
        allowTaint: true,
        backgroundColor: '#ffffff',
        scrollX: 0,
        scrollY: 0,
      });

      el.style.transform = prevTransform;
      el.style.marginBottom = prevMarginBottom;
      el.style.marginRight = prevMarginRight;
      if (wrapper) wrapper.style.overflow = prevWrapperOverflow;

      const blob = await this.compressToTargetSize(canvas, this.MAX_EXPORT_BYTES);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.download = `${this.order.orderNumber}.jpg`;
      a.href = url;
      a.click();
      URL.revokeObjectURL(url);
    } finally {
      this.isRendering = false;
    }
  }

  // Xuất JPEG (nén được, khác PNG lossless) — giảm dần chất lượng, rồi giảm cả kích thước ảnh
  // nếu vẫn còn vượt ngưỡng, cho tới khi dưới maxBytes.
  private async compressToTargetSize(sourceCanvas: HTMLCanvasElement, maxBytes: number): Promise<Blob> {
    let canvas = sourceCanvas;

    for (let round = 0; round < 4; round++) {
      for (let quality = 0.92; quality >= 0.4; quality -= 0.08) {
        const blob = await this.canvasToJpegBlob(canvas, quality);
        if (blob.size <= maxBytes) return blob;
      }
      // Vẫn quá ngưỡng dù đã nén tối đa chất lượng — thu nhỏ kích thước ảnh rồi thử lại.
      canvas = this.downscaleCanvas(canvas, 0.8);
    }

    // Đã cố hết mức — trả về bản nén nhất có thể (hiếm khi tới đây với nội dung dạng này).
    return this.canvasToJpegBlob(canvas, 0.4);
  }

  private canvasToJpegBlob(canvas: HTMLCanvasElement, quality: number): Promise<Blob> {
    return new Promise((resolve, reject) => {
      canvas.toBlob(b => b ? resolve(b) : reject(new Error('Không thể tạo ảnh JPEG.')), 'image/jpeg', quality);
    });
  }

  private downscaleCanvas(source: HTMLCanvasElement, factor: number): HTMLCanvasElement {
    const target = document.createElement('canvas');
    target.width = Math.round(source.width * factor);
    target.height = Math.round(source.height * factor);
    const ctx = target.getContext('2d')!;
    ctx.drawImage(source, 0, 0, target.width, target.height);
    return target;
  }

  getSpecificationLines(): string[] {
    const item = this.order.items?.[0];
    // Không đưa ghi chú kiểu dáng (styleNotes) vào đây vì nội dung đó đã hiển thị
    // đầy đủ ở phần CHÚ Ý bên dưới; lặp lại sẽ gây trùng thông tin.
    const lines = [
      item?.collectionName,
      item?.formName,
      item?.materialName,
      this.getColorText(),
      item?.specificationName,
    ]
      .flatMap(value => (value || '').split('\n'))
      .map(value => value.replace(/^-\s*/, '').trim())
      .filter(Boolean);

    return [...new Set(lines.map(value => value.toUpperCase()))];
  }

  getOrderNote(): string {
    return (this.order.styleNotes || '').trim();
  }

  getOrderFooterLine(): string {
    const orderType = (this.order.orderTypeName || '').trim().toUpperCase();
    const delivery = this.getDeliveryText();
    return ['# ' + this.order.orderNumber, orderType, delivery].filter(Boolean).join(' / ');
  }

  private getDeliveryText(): string {
    if (this.order.deliveryMethod === 2) return 'GHTK';
    return (this.order.deliveryMethodName || '').trim().toUpperCase();
  }

  getSizeQty(size: string, gender?: 'NAM' | 'NU'): number {
    const normalizedSize = size.trim().toUpperCase();
    return (this.order.items || [])
      .filter(item => this.matchesSize(item.size, normalizedSize, gender))
      .reduce((sum, item) => sum + item.quantity, 0);
  }

  // Ẩn số 0: nếu size không có số lượng thì để trống thay vì hiển thị 0.
  getSizeQtyDisplay(size: string, gender?: 'NAM' | 'NU'): string {
    const qty = this.getSizeQty(size, gender);
    return qty > 0 ? String(qty) : '';
  }

  getTotalQty(): number {
    return (this.order.items || []).reduce((sum, item) => sum + item.quantity, 0);
  }

  // Form "Classic" chia NAM/NỮ; "Oversize"/"Unisex" hiển thị 1 dòng, không chia giới tính.
  isGenderedForm(): boolean {
    const name = (this.order?.items?.[0]?.formName || '').toLowerCase();
    return !(name.includes('oversize') || name.includes('unisex'));
  }

  // Dạng đơn "Áo sẵn" → bảng size chuẩn XS-3XL, 1 dòng, bất kể Form dáng.
  isReadyMadeOrder(): boolean {
    return (this.order?.orderTypeName || '').toLowerCase().includes('sẵn');
  }

  fmt(d: Date | string | null | undefined): string {
    if (!d) return '';
    const dt = new Date(d);
    if (Number.isNaN(dt.getTime())) return '';
    return `${String(dt.getDate()).padStart(2, '0')}/${String(dt.getMonth() + 1).padStart(2, '0')}`;
  }

  fmtFull(d: Date | string | null | undefined): string {
    if (!d) return '00/00/0000';
    const dt = new Date(d);
    if (Number.isNaN(dt.getTime())) return '00/00/0000';
    return `${String(dt.getDate()).padStart(2, '0')}/${String(dt.getMonth() + 1).padStart(2, '0')}/${dt.getFullYear()}`;
  }

  sizeColor(size: string): string {
    const map: Record<string, string> = {
      S: '#1565c0',
      M: '#1565c0',
      L: '#00695c',
      XL: '#b71c1c',
      XXL: '#1565c0',
      NC1: '#1b5e20',
      NC2: '#6a1b9a',
      NC3: '#e65100'
    };
    return map[size.toUpperCase()] ?? '#1a237e';
  }

  parsePersonNames(): { size: string; names: string[] }[] {
    return [];
  }

  parseGiftItems(): string[] {
    return [];
  }

  private generateQr(): void {
    import('qrcode').then(QRCode => {
      const token = this.order.qrCodeToken;
      if (!token) {
        this.qrDataUrl = '';
        return;
      }
      const url = `${window.location.origin}/scan/${token}`;
      QRCode.toDataURL(url, { width: 220, margin: 1 }).then(dataUrl => this.qrDataUrl = dataUrl);
    });
  }

  private parseStyleNotes(): [string, string][] {
    if (!this.order.styleNotes) return [];
    return this.order.styleNotes
      .split('|')
      .map(part => part.trim())
      .filter(part => part.includes(':'))
      .map(part => {
        const index = part.indexOf(':');
        return [part.slice(0, index).trim(), part.slice(index + 1).trim()] as [string, string];
      });
  }

  private getColorText(): string {
    const colorLines = this.parseStyleNotes()
      .filter(([key]) => /màu/i.test(key))
      // Bỏ qua ghi chú màu không có giá trị để fallback về tên màu thực tế
      .filter(([, value]) => value)
      .map(([key, value]) => `${key}: ${value}`);

    if (colorLines.length) return colorLines.join('\n');

    return [...new Set((this.order.items || []).map(item => item.mainColorName).filter(Boolean))]
      .join('\n');
  }

  private matchesSize(value: string | undefined, size: string, gender?: 'NAM' | 'NU'): boolean {
    const normalized = (value || '').trim().toUpperCase();
    if (!normalized) return false;

    if (normalized.includes(':')) {
      const [rawGender, rawSize] = normalized.split(':', 2);
      const normalizedGender = rawGender === 'NỮ' || rawGender === 'NU' ? 'NU' : 'NAM';
      return rawSize === size && (!gender || normalizedGender === gender);
    }

    if (gender === 'NU') return false;
    return normalized === size;
  }
}
