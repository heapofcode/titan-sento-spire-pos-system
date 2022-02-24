import { Component, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { ToolbarItems, GridComponent, DataStateChangeEventArgs,
  CommandModel, CommandClickEventArgs, SearchEventArgs, PageSettingsModel } from '@syncfusion/ej2-angular-grids';
import { HttpService } from '../../../../core';
import { DialogComponent } from '@syncfusion/ej2-angular-popups';
import { TransientDraft } from '../../../../shared/model/tables/draft';

@Component({
  selector: 'app-return',
  templateUrl: './return.component.html',
  styleUrls:['./return.component.scss']
})

export class ReturnComponent implements OnInit {
  dialogwidth:number;
  dialogdatasource:any;
  take:number = 5;
  skip:number = 0;
  search:string = '';

  constructor(private _httpService:HttpService) {  }

  @ViewChild('grid') grid:GridComponent;
  @ViewChild('dialog') dialog:DialogComponent;

  @Output () onCloseDialog = new EventEmitter<object>();

  allowPaging:boolean = true;
  pageSettings:PageSettingsModel = { pageSizes: false, pageSize: this.take };
  datasource:any;
  toolbar:ToolbarItems[] = ['Search'];
  commands:CommandModel[] = [
    { buttonOption: { content: 'Подробно', cssClass: 'e-flat', iconCss:'fa fa-eye'}},
    { buttonOption: { content: 'Возврат', cssClass: 'e-flat', iconCss:'fas fa-undo'}}
  ];

  dataStateChange(args:DataStateChangeEventArgs){
    if(args.action.requestType === 'searching') {
      this.search = (args.action as SearchEventArgs).searchString;
    }
    if(args.action.requestType === 'paging'){
      this.take = args.take;
      this.skip = args.skip;
    }

    this._httpService.get(`/vendor/draft?take=${this.take}&&skip=${this.skip}&&search=${this.search}`)
      .subscribe(data=>{
        if(data.success) this.datasource = data});
  }

  // dataSourceChanged(args: DataSourceChangedEventArgs){
  //   if (args.requestType === 'delete') {
  //       this._httpService.delete(`/vendor/draft/return/${args.data[0].id}`).subscribe(
  //         data => {
  //           this.datasource = data;
  //           args.endEdit();
  //         },
  //         error => {
  //           console.log(error);
  //           args.endEdit();
  //         });
  //   }
  // }

  async commandClick(args: CommandClickEventArgs):Promise<void> {
    let data = <TransientDraft>args.rowData;
    if(args.target.innerText === 'ПОДРОБНО'){
      await this._httpService.get(`/vendor/draft/detail/${data.id}`).toPromise()
      .then(data=> {
        this.dialogdatasource = data;
        this.dialog.visible = true; })
      .catch(error=>console.log(error))
    }else if (args.target.innerText === 'ВОЗВРАТ'){
       this.onCloseDialog.emit({ show:`${false}`, id:`${data.id}` })
    }
  }

  hideDialog(){
    this.dialog.visible = false;
  }

  async ngOnInit():Promise<void> {
    await this._httpService.get(`/vendor/draft?take=${this.take}&&skip=${this.skip}&&search=${this.search}`).toPromise()
    .then(data=> {
      // if(data.result.length===0)
      //   this.take = 0;
      if(data.success)
        this.datasource = data})
    .catch(error=>console.log(error))
  }

}
