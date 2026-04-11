import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoleLabelPipe } from './pipes/role-label.pipe';

@NgModule({
  declarations: [RoleLabelPipe],
  imports: [CommonModule],
  exports: [RoleLabelPipe]
})
export class SharedModule { }
