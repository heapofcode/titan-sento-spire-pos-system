import { Component, OnInit, Input } from '@angular/core';
import { Payment } from '../../../../shared/model/tables/payment';

@Component({
  selector: 'app-cashout',
  templateUrl: './cashout.component.html',
  styleUrls:['./cashout.component.scss']
})

export class CashoutComponent implements OnInit {
  constructor() {}

  @Input() payment:Payment;
  min:number = 0.01;

  blur(args:any){
    if(args.value==null)
      this.payment.cashPayment = 0.01;
  }

  ngOnInit(): void {
    this.payment.cashPayment = this.min;
  }

}
