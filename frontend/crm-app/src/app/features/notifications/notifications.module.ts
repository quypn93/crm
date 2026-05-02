import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationsRoutingModule } from './notifications-routing.module';
import { NotificationListPageComponent } from './notification-list-page/notification-list-page.component';
import { RolePreferencesPageComponent } from './role-preferences-page/role-preferences-page.component';

@NgModule({
  declarations: [
    NotificationListPageComponent,
    RolePreferencesPageComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    NotificationsRoutingModule
  ]
})
export class NotificationsModule { }
