import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CatalogRoutingModule } from './catalog-routing.module';
import { CatalogListComponent } from './catalog-list/catalog-list.component';

@NgModule({
  declarations: [CatalogListComponent],
  imports: [CommonModule, FormsModule, CatalogRoutingModule]
})
export class CatalogModule { }
