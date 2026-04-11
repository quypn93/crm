import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OrdersRoutingModule } from './orders-routing.module';
import { OrderListComponent } from './order-list/order-list.component';
import { OrderFormComponent } from './order-form/order-form.component';
import { OrderDetailComponent } from './order-detail/order-detail.component';
import { OrderCardComponent } from './order-card/order-card.component';

@NgModule({
  declarations: [
    OrderListComponent,
    OrderFormComponent,
    OrderDetailComponent,
    OrderCardComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    OrdersRoutingModule
  ]
})
export class OrdersModule { }
