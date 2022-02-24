import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { LoginComponent } from './login.component';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs'
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';

@NgModule({
  declarations: [LoginComponent],
  imports: [CommonModule, TextBoxModule, ButtonModule, FormsModule]
})
export class LoginModule {}
