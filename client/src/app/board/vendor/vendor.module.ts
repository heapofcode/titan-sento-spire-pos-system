import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VendorComponent } from './vendor.component';

import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ToastModule } from '@syncfusion/ej2-angular-notifications';
import { GridAllModule,ToolbarService,PageService,FilterService,
EditService,SortService,GroupService,CommandColumnService,SearchService,
AggregateService } from '@syncfusion/ej2-angular-grids';
import { ButtonAllModule, ChipListAllModule } from '@syncfusion/ej2-angular-buttons';
import { TextBoxModule, NumericTextBoxModule } from '@syncfusion/ej2-angular-inputs'
import { MenuModule } from '@syncfusion/ej2-angular-navigations';
import { DialogAllModule } from '@syncfusion/ej2-angular-popups';
import { PaymentComponent } from './dialog-components/payment/payment.component';
import { CashinComponent } from './dialog-components/cashin/cashin.component';
import { CashoutComponent } from './dialog-components/cashout/cashout.component';
import { CancelComponent } from './dialog-components/cancel/cancel.component';
import { ReturnComponent } from './dialog-components/return/return.component';

@NgModule({
  declarations: [
    VendorComponent,
    PaymentComponent,
    CashinComponent,
    CashoutComponent,
    CancelComponent,
    ReturnComponent
  ],
  imports: [
  CommonModule,
  ToastModule,
  GridAllModule,
  ButtonAllModule,
  TextBoxModule,
  NumericTextBoxModule,
  ReactiveFormsModule,
  FormsModule,
  MenuModule,
  DialogAllModule,
  ChipListAllModule
  ],
  exports:[VendorComponent],
  providers:[ToolbarService,PageService,FilterService,EditService,SortService,GroupService,CommandColumnService,SearchService,AggregateService ]
})
export class VendorModule {}
