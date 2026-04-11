import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DesignListComponent } from './design-list/design-list.component';
import { DesignFormComponent } from './design-form/design-form.component';
import { DesignDetailComponent } from './design-detail/design-detail.component';
import { ColorFabricsComponent } from './color-fabrics/color-fabrics.component';
import { ShirtComponentsComponent } from './shirt-components/shirt-components.component';

const routes: Routes = [
  { path: '', component: DesignListComponent },
  { path: 'new', component: DesignFormComponent },
  { path: 'color-fabrics', component: ColorFabricsComponent },
  { path: 'shirt-components', component: ShirtComponentsComponent },
  { path: ':id', component: DesignDetailComponent },
  { path: ':id/edit', component: DesignFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DesignsRoutingModule { }
