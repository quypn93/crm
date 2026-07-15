import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OrdersRoutingModule } from './orders-routing.module';
import { OrderListComponent } from './order-list/order-list.component';
import { OrderFormComponent } from './order-form/order-form.component';
import { OrderDetailComponent } from './order-detail/order-detail.component';
import { MyWarehouseOrdersComponent } from './my-warehouse-orders/my-warehouse-orders.component';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [
    OrderListComponent,
    OrderFormComponent,
    OrderDetailComponent,
    MyWarehouseOrdersComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    OrdersRoutingModule,
    SharedModule
  ]
})
export class OrdersModule { }
