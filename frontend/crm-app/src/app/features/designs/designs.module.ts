import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { DesignsRoutingModule } from './designs-routing.module';
import { DesignListComponent } from './design-list/design-list.component';
import { DesignFormComponent } from './design-form/design-form.component';
import { DesignDetailComponent } from './design-detail/design-detail.component';
import { ColorFabricsComponent } from './color-fabrics/color-fabrics.component';
import { ShirtComponentsComponent } from './shirt-components/shirt-components.component';
import { DesignCanvasComponent } from './design-canvas/design-canvas.component';

@NgModule({
  declarations: [
    DesignListComponent,
    DesignFormComponent,
    DesignDetailComponent,
    ColorFabricsComponent,
    ShirtComponentsComponent,
    DesignCanvasComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    DesignsRoutingModule
  ]
})
export class DesignsModule { }
