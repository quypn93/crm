import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CatalogListComponent } from './catalog-list/catalog-list.component';
import { ComponentType } from '../../core/services/design.service';

const routes: Routes = [
  {
    path: 'forms',
    component: CatalogListComponent,
    data: { componentType: ComponentType.Form, title: 'Quản lý Form áo', placeholder: 'VD: Oversize, Regular, Slim...' }
  },
  {
    path: 'style-specs',
    component: CatalogListComponent,
    data: { componentType: ComponentType.StyleSpec, title: 'Quản lý Quy cách', placeholder: 'VD: ĐP4M, ĐP2M...' }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CatalogRoutingModule { }
