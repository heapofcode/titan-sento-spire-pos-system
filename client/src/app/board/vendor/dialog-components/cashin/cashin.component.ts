import { Component, OnInit, Input } from '@angular/core';
import { Payment } from '../../../../shared/model/tables/payment';

@Component({
  selector: 'app-cashin',
  templateUrl: './cashin.component.html',
  styleUrls:['./cashin.component.scss']
})

export class CashinComponent implements OnInit {
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
