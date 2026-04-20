import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export type ToastKind = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toasts = new BehaviorSubject<Toast[]>([]);
  readonly toasts$: Observable<Toast[]> = this._toasts.asObservable();
  private _next = 1;

  show(message: string, kind: ToastKind = 'info', durationMs = 4000): void {
    const id = this._next++;
    const current = this._toasts.getValue();
    this._toasts.next([...current, { id, kind, message }]);
    if (durationMs > 0) {
      setTimeout(() => this.dismiss(id), durationMs);
    }
  }

  success(message: string, durationMs = 3000): void { this.show(message, 'success', durationMs); }
  error(message: string, durationMs = 5000): void   { this.show(message, 'error', durationMs); }
  info(message: string, durationMs = 3000): void    { this.show(message, 'info', durationMs); }
  warning(message: string, durationMs = 4000): void { this.show(message, 'warning', durationMs); }

  dismiss(id: number): void {
    this._toasts.next(this._toasts.getValue().filter(t => t.id !== id));
  }
}
