import { Component, ElementRef, Input, OnChanges, AfterViewInit, ViewChild } from '@angular/core';
import { Order } from '../../../core/models/order.model';
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
  private viewReady = false;

  ngAfterViewInit(): void { this.viewReady = true; if (this.order) this.generateQr(); }
  ngOnChanges(): void     { if (this.viewReady && this.order) this.generateQr(); }

  private generateQr(): void {
    import('qrcode').then(QRCode => {
      const url = `${window.location.origin}/orders/${this.order.id}`;
      QRCode.toDataURL(url, { width: 220, margin: 1 }).then(d => this.qrDataUrl = d);
    });
  }

  async exportImage(): Promise<void> {
    if (!this.cardRef) return;
    this.isRendering = true;
    try {
      const el = this.cardRef.nativeElement;
      // Tạm thời reset scale để html2canvas chụp full resolution
      const prevTransform = el.style.transform;
      const prevMargin = el.style.marginBottom;
      el.style.transform = 'scale(1)';
      el.style.marginBottom = '0';
      const canvas = await html2canvas(el, {
        scale: 2,
        useCORS: true,
        allowTaint: true,
        backgroundColor: '#ffffff',
        scrollX: 0,
        scrollY: 0,
      });
      el.style.transform = prevTransform;
      el.style.marginBottom = prevMargin;
      const a = document.createElement('a');
      a.download = `${this.order.orderNumber}.png`;
      a.href = canvas.toDataURL('image/png');
      a.click();
    } finally {
      this.isRendering = false;
    }
  }

  // ── Data helpers ──────────────────────────────────────────

  parsePersonNames(): { size: string; names: string[] }[] {
    if (!this.order.personNamesBySize) return [];
    try {
      return Object.entries(JSON.parse(this.order.personNamesBySize))
        .map(([size, names]) => ({ size, names: names as string[] }));
    } catch { return []; }
  }

  parseGiftItems(): string[] {
    if (!this.order.giftItems) return [];
    try {
      return (JSON.parse(this.order.giftItems) as any[]).map(g => g.description ?? String(g));
    } catch { return [this.order.giftItems]; }
  }

  private parseStyleNotes(): [string, string][] {
    if (!this.order.styleNotes) return [];
    return this.order.styleNotes.split('|').map(p => p.trim()).filter(p => p.includes(':'))
      .map(p => { const i = p.indexOf(':'); return [p.slice(0,i).trim(), p.slice(i+1).trim()] as [string,string]; });
  }

  getMaterial(): string {
    const e = this.parseStyleNotes().find(([k]) => /chất liệu/i.test(k));
    if (e) return e[1];
    return [...new Set(this.order.items.map(i => i.material).filter(Boolean))].join(', ');
  }

  getColorText(): string {
    const lines = this.parseStyleNotes().filter(([k]) => /màu/i.test(k))
      .map(([k,v]) => `- ${k.toUpperCase()}: ${v}`);
    if (lines.length) return lines.join('\n');
    return [...new Set(this.order.items.map(i => i.color).filter(Boolean))].join(', ');
  }

  getStyleText(): string {
    return this.parseStyleNotes().filter(([k]) => !/màu|chất liệu/i.test(k))
      .map(([k,v]) => `- ${k.toUpperCase()}: ${v}`).join('\n');
  }

  getSizeQty(size: string): number {
    return this.order.items
      .filter(i => i.size?.trim().toUpperCase() === size)
      .reduce((s, i) => s + i.quantity, 0);
  }

  getTotalQty(): number {
    return this.order.items.reduce((s, i) => s + i.quantity, 0);
  }

  fmt(d: Date|string|null|undefined): string {
    if (!d) return '';
    const dt = new Date(d);
    if (isNaN(dt.getTime())) return '';
    return `${String(dt.getDate()).padStart(2,'0')}/${String(dt.getMonth()+1).padStart(2,'0')}`;
  }

  sizeColor(size: string): string {
    const map: Record<string,string> = {
      S:'#1565c0',M:'#1565c0',L:'#00695c',XL:'#b71c1c',
      XXL:'#1565c0',NC1:'#1b5e20',NC2:'#6a1b9a',NC3:'#e65100'
    };
    return map[size.toUpperCase()] ?? '#1a237e';
  }

  readonly SIZES = ['S','M','L','XL','XXL','NC1','NC2','NC3'];
}
