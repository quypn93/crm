import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      {
        path: 'customers',
        loadChildren: () => import('./features/customers/customers.module').then(m => m.CustomersModule)
      },
      {
        path: 'deals',
        loadChildren: () => import('./features/deals/deals.module').then(m => m.DealsModule)
      },
      {
        path: 'tasks',
        loadChildren: () => import('./features/tasks/tasks.module').then(m => m.TasksModule)
      },
      {
        path: 'orders',
        loadChildren: () => import('./features/orders/orders.module').then(m => m.OrdersModule)
      },
      {
        path: 'designs',
        loadChildren: () => import('./features/designs/designs.module').then(m => m.DesignsModule)
      },
      {
        path: 'reports',
        loadChildren: () => import('./features/reports/reports.module').then(m => m.ReportsModule)
      },
      {
        path: 'production',
        loadChildren: () => import('./features/production/production.module').then(m => m.ProductionModule)
      },
      {
        path: 'settings',
        loadChildren: () => import('./features/settings/settings.module').then(m => m.SettingsModule)
      }
    ]
  },
  {
    path: 'scan/:token',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/scan/scan.module').then(m => m.ScanModule)
  },
  { path: '**', redirectTo: 'dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
