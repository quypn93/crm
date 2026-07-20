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

      const a = document.createElement('a');
      a.download = `${this.order.orderNumber}.png`;
      a.href = canvas.toDataURL('image/png');
      a.click();
    } finally {
      this.isRendering = false;
    }
  }

  getSpecificationLines(): string[] {
    const item = this.order.items?.[0];
    const lines = [
      item?.collectionName,
      item?.formName,
      item?.materialName,
      this.getColorText(),
      item?.specificationName,
      this.getStyleText(),
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

  getTotalQty(): number {
    return (this.order.items || []).reduce((sum, item) => sum + item.quantity, 0);
  }

  // Form "Classic" chia NAM/NỮ; "Oversize"/"Unisex" hiển thị 1 dòng, không chia giới tính.
  isGenderedForm(): boolean {
    const name = (this.order?.items?.[0]?.formName || '').toLowerCase();
    return !(name.includes('oversize') || name.includes('unisex'));
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
      .map(([key, value]) => `${key}: ${value}`);

    if (colorLines.length) return colorLines.join('\n');

    return [...new Set((this.order.items || []).map(item => item.mainColorName).filter(Boolean))]
      .join('\n');
  }

  private getStyleText(): string {
    return this.parseStyleNotes()
      .filter(([key]) => !/màu|chất liệu/i.test(key))
      .map(([key, value]) => `${key}: ${value}`)
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
