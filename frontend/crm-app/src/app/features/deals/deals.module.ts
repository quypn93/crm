import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { DealsRoutingModule } from './deals-routing.module';
import { DealKanbanComponent } from './deal-kanban/deal-kanban.component';
import { DealFormComponent } from './deal-form/deal-form.component';
import { DealDetailComponent } from './deal-detail/deal-detail.component';

@NgModule({
  declarations: [
    DealKanbanComponent,
    DealFormComponent,
    DealDetailComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    DragDropModule,
    DealsRoutingModule
  ]
})
export class DealsModule { }
