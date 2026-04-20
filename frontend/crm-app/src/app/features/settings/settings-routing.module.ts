import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserListComponent } from './user-list/user-list.component';
import { UserFormComponent } from './user-form/user-form.component';
import { RoleListComponent } from './role-list/role-list.component';
import { RoleFormComponent } from './role-form/role-form.component';
import { CollectionsAdminComponent } from './collections-admin/collections-admin.component';
import { ProductionDaysAdminComponent } from './production-days-admin/production-days-admin.component';
import { DepositsAdminComponent } from './deposits-admin/deposits-admin.component';
import { LookupsAdminComponent } from './lookups-admin/lookups-admin.component';

const routes: Routes = [
  { path: '', redirectTo: 'users', pathMatch: 'full' },
  { path: 'users', component: UserListComponent },
  { path: 'users/new', component: UserFormComponent },
  { path: 'users/:id/edit', component: UserFormComponent },
  { path: 'roles', component: RoleListComponent },
  { path: 'roles/new', component: RoleFormComponent },
  { path: 'roles/:id/edit', component: RoleFormComponent },
  { path: 'collections', component: CollectionsAdminComponent },
  { path: 'production-days', component: ProductionDaysAdminComponent },
  { path: 'deposits', component: DepositsAdminComponent },
  { path: 'materials', component: LookupsAdminComponent, data: { resource: 'materials', title: 'Chất liệu' } },
  { path: 'product-forms', component: LookupsAdminComponent, data: { resource: 'product-forms', title: 'Form áo' } },
  { path: 'product-specifications', component: LookupsAdminComponent, data: { resource: 'product-specifications', title: 'Quy cách' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SettingsRoutingModule { }
