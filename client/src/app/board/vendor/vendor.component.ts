import { Component, OnInit, HostListener, ViewChild } from '@angular/core';
import { EditSettingsModel, GridComponent, DialogEditEventArgs, DataSourceChangedEventArgs, DataStateChangeEventArgs } from '@syncfusion/ej2-angular-grids';
import { NumericTextBoxComponent, TextBoxComponent } from '@syncfusion/ej2-angular-inputs';
import { MenuItemModel } from '@syncfusion/ej2-angular-navigations';
import { DialogComponent } from '@syncfusion/ej2-angular-popups';
import { HttpService } from '../../core/services/http.service';
import { Payment } from '../../shared/model/tables/payment';

@Component({
  selector: 'app-vendor',
  templateUrl: './vendor.component.html',
  styleUrls: ['./vendor.component.scss'],
})
export class VendorComponent implements OnInit {

  constructor(private _httpService:HttpService) {}

  //#region Variables
  currentworkshift:string;
  currentusername:string;
  payment:Payment = new Payment();
  isreturn:boolean;
  //#endregion

  //#region  ViewChild
  @ViewChild('grid') grid:GridComponent;
  @ViewChild('gridToolbar', {static:true}) gridToolbar: any;
  @ViewChild('scaner') scaner:TextBoxComponent;
  @ViewChild('qty') qty:NumericTextBoxComponent;
  @ViewChild('dialog') dialog:DialogComponent;
  //#endregion

  //#region Keybordhook
  @HostListener('window:keyup',['$event'])
  keyEvent(event:KeyboardEvent){
    if(this.activeModule==='main' && this.isreturn==false){
      if (event.key >= '0' && event.key <= '9'){
        this.scaner.value == null ? this.scaner.value = event.key : this.scaner.value = this.scaner.value + event.key;
      } else if (event.key === 'Enter'){
        if(this.scaner.enabled&&this.scaner.value){
          this._httpService.post(`/vendor/search/${this.scaner.value.toString()}`).subscribe(
          data=>{
            if(data)
              this.grid.refresh()
              // this.grid.addRecord(data)
          },
          error=>console.log(error)
          );

          this.scaner.value = null;
        }
      } else if (event.key === 'Backspace'){
        if(this.scaner.value)
          this.scaner.value = this.scaner.value.substring(0,this.scaner.value.length-1);
      } else if (event.key === 'Delete'){
        this.scaner.value = null;
      }
    }
  }
  //#endregion

  //#region Dialog
  activeModule:string='main';
  dialogwidth:number;

  beforecloseDialog(){
    this.activeModule='main';
  }

  async acceptDialog():Promise<void>{
    console.log(this.activeModule)
    if(this.payment.getAmount===0 && this.currentworkshift){
      await this._httpService.put(`/vendor/${this.activeModule}?isreturn=${this.isreturn}`,this.payment).toPromise()
      .then(data=> {
        data.success ? this.dialog.visible=false : this.dialog.visible=true;
        this.grid.refresh();
        console.log(data)
      })
      .catch(error=>console.log(error))
    }
  }

  hideDialog(){
    this.dialog.visible = false;
    this.activeModule='main';
  }

  async onCloseDialog(args:any):Promise<void>{
    if(args.show==='false'){
      await this._httpService.get(`/vendor/draft/return/${args.id}`).toPromise()
        .then(data=>{
          if(data.success){
            this.hideDialog();
            this.isreturn = true;
            this.datasource = data;
          }})
        .catch(error=>console.log(error))
    }
  }
  //#endregion

  //#region Grid
  editSettings:EditSettingsModel = {allowEditing: true, allowAdding: true, allowDeleting: true, mode:'Dialog' }
  datasource:any;
  toolbar:any;
  toolbarMenu: MenuItemModel[] = [
    {text:'Количество',id:'quantity'},
    {text:'Удалить',id:'delete'},
    {text:'Очистить',id:'clear'},
    {text: 'Операции',
      items: [
        { text: 'Открыть смену', id:'openworkshift' },
        { text: 'Закрыть смену', id:'closeworkshift' },
        { text: 'Внесение средств', id:'cashin' },
        { text: 'Изъятие средств', id:'cashout' },
        { text: 'Отмена чека', id:'cancel' },
        { text: 'Возврат чека', id:'return' },
        { text: 'Отчеты',
          items:[
            { text: 'Отчет без гашения', id:'xreport' },
            { text: 'Статистика продаж по кассиру', id:'vendorstat' }
          ]},
      ]
    },
    { text: 'Расчет',id:'payment' }
  ];

  async menuSelect(args:any):Promise<void>{
    switch (args.element.id){
      case 'quantity':
        this.grid.startEdit();
        break;
      case 'delete':
        this.grid.deleteRecord();
        break;
      case 'clear':
        this.grid.deleteRecord('id',{clr:0});
        this.isreturn = false;
        break;
      case 'payment':
      case 'cashin':
      case 'cashout':
      case 'cancel':
      case 'return':
        let tamount=0;
        (this.grid.aggregateModule as any).footerRenderer.aggregates.result.forEach(element => {
          tamount += element.amount;
        });
        this.payment = { amount:Number(tamount.toFixed(2)), getAmount:Number(tamount.toFixed(2)), cashBack:0, cashPayment:0, cardPayment:0 }
        this.dialog.visible = true;
        (args.element.id === 'cancel' || args.element.id === 'return') ? this.dialogwidth = 1000 : this.dialogwidth = 500;
        this.activeModule = args.element.id;
        break;
      default:
        await this._httpService.put(`/vendor/${args.element.id}`).toPromise()
          .then(data=>{console.log(data); this.takecurrentworkshift();})
          .catch(error=>console.log(error))
        console.log(`Operation with menu ${args.element.id}`);
    }
  }

  dataBound(){
    this.grid.getHeaderContent().append(this.grid.getFooterContent());
  }

  quantityBlur(args:any){
    if(args.value==null){
      this.qty.value = 1;
      this.qty.refresh();
    }
  }

  actionComplete(args: DialogEditEventArgs): void {
    if ((args.requestType === 'beginEdit' || args.requestType === 'add')) {
      if(args.dialog){
        args.dialog.width = 500;
        args.dialog.header = args.requestType === 'beginEdit' ? 'Редактирование' : 'Новый';
        this.activeModule='qauntity';
      }
    }
  }

  dataStateChange(args:DataStateChangeEventArgs){
    this._httpService.get('/vendor')
      .subscribe( data=> {
        if (data.success) {
          if(data.result.length===0) this.isreturn = false;
          this.datasource = data
        }})
    this.activeModule='main';
  }

  dataSourceChanged(args: DataSourceChangedEventArgs){
    // if (args.action === 'add') {
    //   this._httpService.post('/vendor', args.data).subscribe(
    //   () => {},
    //   error => console.log(error),
    //   () => {
    //     args.endEdit();
    //   });
    // } else
    if (args.action === 'edit') {
        this._httpService.put(`/vendor/edit`, args.data).subscribe(
        data => {
          this.datasource = data;
          args.endEdit();
        },
        error => {
          console.log(error);
          this.grid.closeEdit();
        });
    } else if (args.requestType === 'delete') {
        let value;
        args.data[0].id.clr === 0 ? value = 0 : value = args.data[0].id;
        this._httpService.delete(`/vendor/delete/${value}`).subscribe(
          (data) => {
            this.datasource = data;
            args.endEdit();
          },
          error => {
            console.log(error);
            args.endEdit();
          });
    }
  }
//#endregion

  async takecurrentworkshift():Promise<void>{
    await this._httpService.get(`/user/workshift`).toPromise()
    .then(data=>{
      if(data.success)
      {
        this.currentworkshift = data.userInfo.numberOfWorkShift;
        this.currentusername=data.userInfo.fullName;
      }
    })
    .catch(error=>console.log(error))
  }

  async ngOnInit():Promise<void> {
    this.toolbar = [{template: this.gridToolbar}];
    await this._httpService.get(`/vendor`).toPromise()
    .then(data=>{
      data.isReturn ? this.isreturn = true : this.isreturn = false;
      if(data.success) {this.datasource = data; this.takecurrentworkshift()}})
    //.then(data=>console.log(data))
    .catch(error=>console.log(error))
  }
}
