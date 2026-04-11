import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { QrScanLandingComponent } from './qr-scan-landing/qr-scan-landing.component';

const routes: Routes = [
  { path: '', component: QrScanLandingComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ScanRoutingModule { }
