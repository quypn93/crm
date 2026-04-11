import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ScanRoutingModule } from './scan-routing.module';
import { QrScanLandingComponent } from './qr-scan-landing/qr-scan-landing.component';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [QrScanLandingComponent],
  imports: [CommonModule, FormsModule, ScanRoutingModule, SharedModule]
})
export class ScanModule { }
