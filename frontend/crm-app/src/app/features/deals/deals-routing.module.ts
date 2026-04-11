import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DealKanbanComponent } from './deal-kanban/deal-kanban.component';
import { DealFormComponent } from './deal-form/deal-form.component';
import { DealDetailComponent } from './deal-detail/deal-detail.component';

const routes: Routes = [
  { path: '', component: DealKanbanComponent },
  { path: 'new', component: DealFormComponent },
  { path: ':id', component: DealDetailComponent },
  { path: ':id/edit', component: DealFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DealsRoutingModule { }
