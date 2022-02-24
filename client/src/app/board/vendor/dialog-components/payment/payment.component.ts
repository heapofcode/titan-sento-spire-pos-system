import { Component, Input } from '@angular/core';
import { Payment } from '../../../../shared/model/tables/payment';

@Component({
  selector: 'app-payment',
  templateUrl: './payment.component.html',
  styleUrls:['./payment.component.scss']
})

export class PaymentComponent {
  constructor() {}

  @Input() payment:Payment;

  blurCash(args:any){
    if(args.value==null)
      this.payment.cashPayment = 0;
  }

  blurCard(args:any){
    if(args.value==null)
      this.payment.cardPayment = 0;
  }

  changeCash(args:any){
    this.payment.getAmount = this.payment.amount - this.payment.cardPayment - args.value;
    this.payment.getAmount < 0 ? (this.payment.cashBack = Math.abs(this.payment.getAmount), this.payment.getAmount = 0) : this.payment.cashBack = 0
  }

  changeCard(args:any){
    this.payment.getAmount = this.payment.amount - this.payment.cashPayment - args.value;
    this.payment.getAmount < 0 ? (this.payment.cashBack = Math.abs(this.payment.getAmount), this.payment.getAmount = 0) : this.payment.cashBack = 0
  }

  allOneClick(args:any){
    args.currentTarget.id==='allcash' ? this.payment.cashPayment = this.payment.amount : this.payment.cardPayment = this.payment.amount
  }

}
