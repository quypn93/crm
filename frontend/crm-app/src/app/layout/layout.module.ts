import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { MainLayoutComponent } from './main-layout/main-layout.component';
import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { NotificationBellComponent } from './notification-bell/notification-bell.component';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    MainLayoutComponent,
    HeaderComponent,
    SidebarComponent,
    NotificationBellComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    SharedModule
  ],
  exports: [
    MainLayoutComponent,
    HeaderComponent,
    SidebarComponent,
    NotificationBellComponent
  ]
})
export class LayoutModule { }
