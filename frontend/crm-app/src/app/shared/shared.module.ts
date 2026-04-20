import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoleLabelPipe } from './pipes/role-label.pipe';
import { OrderCardComponent } from '../features/orders/order-card/order-card.component';
import { ToastContainerComponent } from './components/toast-container/toast-container.component';

@NgModule({
  declarations: [RoleLabelPipe, OrderCardComponent, ToastContainerComponent],
  imports: [CommonModule],
  exports: [RoleLabelPipe, OrderCardComponent, ToastContainerComponent]
})
export class SharedModule { }
