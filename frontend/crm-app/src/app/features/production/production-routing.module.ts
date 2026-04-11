import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductionDashboardComponent } from './production-dashboard/production-dashboard.component';
import { ProductionStageListComponent } from './production-stage-list/production-stage-list.component';
import { ProductionStageFormComponent } from './production-stage-form/production-stage-form.component';

const routes: Routes = [
  { path: '', component: ProductionDashboardComponent },
  { path: 'stages', component: ProductionStageListComponent },
  { path: 'stages/new', component: ProductionStageFormComponent },
  { path: 'stages/:id/edit', component: ProductionStageFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProductionRoutingModule { }
