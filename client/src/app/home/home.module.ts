import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomeComponent } from './home.component';

import { VendorModule } from '../board';


@NgModule({
  declarations: [HomeComponent],
  imports: [CommonModule, VendorModule]
})
export class HomeModule {}
