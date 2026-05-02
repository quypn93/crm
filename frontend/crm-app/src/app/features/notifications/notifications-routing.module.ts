import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NotificationListPageComponent } from './notification-list-page/notification-list-page.component';
import { RolePreferencesPageComponent } from './role-preferences-page/role-preferences-page.component';
import { RoleGuard } from '../../core/guards/role.guard';
import { RoleNames } from '../../core/services/auth.service';

const routes: Routes = [
  { path: '', component: NotificationListPageComponent },
  {
    path: 'admin/preferences',
    component: RolePreferencesPageComponent,
    canActivate: [RoleGuard],
    data: { roles: [RoleNames.Admin] }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class NotificationsRoutingModule { }
