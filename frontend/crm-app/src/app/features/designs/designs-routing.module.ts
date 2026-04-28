import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DesignListComponent } from './design-list/design-list.component';
import { DesignFormComponent } from './design-form/design-form.component';
import { DesignDetailComponent } from './design-detail/design-detail.component';
import { ColorFabricsComponent } from './color-fabrics/color-fabrics.component';
import { ShirtComponentsComponent } from './shirt-components/shirt-components.component';
import { DesignAssignComponent } from './design-assign/design-assign.component';
import { DesignMyTasksComponent } from './design-my-tasks/design-my-tasks.component';

const routes: Routes = [
  { path: '', component: DesignListComponent },
  { path: 'my-tasks', component: DesignMyTasksComponent },
  { path: 'assign', component: DesignAssignComponent },
  { path: 'new', component: DesignFormComponent },
  { path: 'color-fabrics', component: ColorFabricsComponent },
  { path: 'shirt-components', component: ShirtComponentsComponent },
  { path: ':id', component: DesignDetailComponent },
  // Assignment edit — simplified form (sale chỉ sửa spec/logo/designer).
  { path: ':id/edit', component: DesignAssignComponent },
  // Canvas editor cũ giữ cho admin/designer cần custom sâu.
  { path: ':id/canvas-edit', component: DesignFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DesignsRoutingModule { }
