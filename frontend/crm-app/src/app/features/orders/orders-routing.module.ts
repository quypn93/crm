import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrderListComponent } from './order-list/order-list.component';
import { OrderFormComponent } from './order-form/order-form.component';
import { OrderDetailComponent } from './order-detail/order-detail.component';
import { MyWarehouseOrdersComponent } from './my-warehouse-orders/my-warehouse-orders.component';

const routes: Routes = [
  { path: '', component: OrderListComponent },
  { path: 'new', component: OrderFormComponent },
  { path: 'my-warehouse', component: MyWarehouseOrdersComponent },
  { path: ':id', component: OrderDetailComponent },
  { path: ':id/edit', component: OrderFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule { }
