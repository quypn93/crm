import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ProductionRoutingModule } from './production-routing.module';
import { ProductionDashboardComponent } from './production-dashboard/production-dashboard.component';
import { ProductionStageListComponent } from './production-stage-list/production-stage-list.component';
import { ProductionStageFormComponent } from './production-stage-form/production-stage-form.component';

@NgModule({
  declarations: [
    ProductionDashboardComponent,
    ProductionStageListComponent,
    ProductionStageFormComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    ProductionRoutingModule
  ]
})
export class ProductionModule { }
