import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SettingsRoutingModule } from './settings-routing.module';
import { UserListComponent } from './user-list/user-list.component';
import { UserFormComponent } from './user-form/user-form.component';
import { RoleListComponent } from './role-list/role-list.component';
import { RoleFormComponent } from './role-form/role-form.component';
import { CollectionsAdminComponent } from './collections-admin/collections-admin.component';
import { ProductionDaysAdminComponent } from './production-days-admin/production-days-admin.component';
import { DepositsAdminComponent } from './deposits-admin/deposits-admin.component';
import { LookupsAdminComponent } from './lookups-admin/lookups-admin.component';

@NgModule({
  declarations: [
    UserListComponent,
    UserFormComponent,
    RoleListComponent,
    RoleFormComponent,
    CollectionsAdminComponent,
    ProductionDaysAdminComponent,
    DepositsAdminComponent,
    LookupsAdminComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    SettingsRoutingModule
  ]
})
export class SettingsModule { }
