import { Component, OnInit, ViewChild } from '@angular/core';
import { EditSettingsModel, ToolbarItems, GridComponent, DialogEditEventArgs, DataSourceChangedEventArgs,
  DataStateChangeEventArgs, CommandModel, CommandClickEventArgs, SearchEventArgs, PageSettingsModel } from '@syncfusion/ej2-angular-grids';
import { HttpService } from '../../../../core';
import { DialogComponent } from '@syncfusion/ej2-angular-popups';
import { TransientDraft } from '../../../../shared/model/tables/draft';

@Component({
  selector: 'app-cancel',
  templateUrl: './cancel.component.html',
  styleUrls:['./cancel.component.scss']
})

export class CancelComponent implements OnInit {
  dialogwidth:number;
  dialogdatasource:any;
  take:number = 5;
  skip:number = 0;
  search:string = '';

  constructor(private _httpService:HttpService) {
  }

  @ViewChild('grid') grid:GridComponent;
  @ViewChild('dialog') dialog:DialogComponent;


  allowPaging:boolean = true;
  pageSettings:PageSettingsModel = { pageSizes: false, pageSize: this.take };
  datasource:any;
  editSettings:EditSettingsModel = { showDeleteConfirmDialog:true, allowDeleting:true };
  toolbar:ToolbarItems[] = ['Search'];
  commands:CommandModel[] = [
    { buttonOption: { content: 'Подробно', cssClass: 'e-flat', iconCss:'fa fa-eye'}},
    { buttonOption: { content: 'Отмена', cssClass: 'e-flat', iconCss:'fas fa-times'}}
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

  dataSourceChanged(args: DataSourceChangedEventArgs){
    if (args.requestType === 'delete') {
        this._httpService.delete(`/vendor/draft/cancel/${args.data[0].id}?iscancel=${true}`).subscribe(
          data => {
            this.datasource = data;
            args.endEdit();
          },
          error => {
            console.log(error);
            args.endEdit();
          });
    }
  }

  async commandClick(args: CommandClickEventArgs):Promise<void> {
    if(args.target.innerText === 'ПОДРОБНО'){
      let data = <TransientDraft>args.rowData;
      await this._httpService.get(`/vendor/draft/detail/${data.id}`, ).toPromise()
      .then(data=> {
        this.dialogdatasource = data;
        this.dialog.visible = true; })
      .catch(error=>console.log(error))
    }else if (args.target.innerText === 'ОТМЕНА'){
       this.grid.deleteRecord();
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
