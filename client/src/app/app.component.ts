import { Component, OnInit } from '@angular/core';
import { ElectronService } from './core/services/electron.service';
import { TranslateService } from '@ngx-translate/core';
import { AppConfig } from '../environments/environment';
import { LocalStorageService } from 'ngx-webstorage';
import { L10n, setCulture, loadCldr } from '@syncfusion/ej2-base';


//Syncfusion localization
loadCldr(require('cldr-data/main/ru-BY/currencies.json'),
require('cldr-data/main/ru-BY/numbers.json'),
require('cldr-data/main/ru-BY/ca-gregorian.json'),
require('cldr-data/main/ru-BY/timeZoneNames.json'),
require('cldr-data/supplemental/numberingSystems.json')
)

L10n.load({
  'ru-BY': {
    daterangepicker: {
      placeholder: 'Пожалуйста, выберите дату',
      startLabel:'Дата начала',
      endLabel: 'Дата окончания',
      applyText: 'Ок',
      cancelText: 'Отмена',
      selectedDays: 'Выбранные дни',
      days: 'Дней',
      customRange: 'Настраиваемая область'
    },
    grid: {
      EmptyRecord: '',
      GroupDropArea: 'Перетащите столбец для группировки',
      Item: 'элемент',
      Items: 'элементов',
      Search: 'Поиск',
      FilterButton: 'Фильтр',
      ClearButton: 'Сброс',
      StartsWith:'Начинается на',
      EndsWith:'Заканчивается на',
      Contains:'Содержит',
      Equal:'Равно',
      NotEqual:'Не равно',
      LessThan:'Меньше чем',
      LessThanOrEqual:'Меньше или равно',
      GreaterThan:'Больше чем',
      GreaterThanOrEqual:'Больше или равно',
      SelectAll:'Выбрать все',
      EnterValue:'Введите значение',
      ChooseDate:'Выбрать дату',
      CurrentPageInfo: '{0} из {1} страниц',
      TotalItemsInfo:'({0} элементов)',
      Add:'Добавить',
      Edit:'Изменить',
      Cancel:'Отменить',
      Update:'Сохранить',
      Delete:'Удалить',
      BatchSaveConfirm:'Вы уверены, что хотите сохранить изменения?',
      BatchSaveLostChanges:'Несохраненные изменения будут потеряны. Вы уверены что хотите продолжить?',
      ConfirmDelete:'Вы уверены, что хотите удалить запись?',
      CancelEdit:'Вы уверены, что хотите отменить изменения?',
      CancelButton:'Отмена',
      OKButton:'Ок',
      SaveButton:'Сохранить',
      EditOperationAlert:'Для операции редактирования не выбрано ни одной записи.',
      DeleteOperationAlert:'Для операции удаления не выбрано ни одной записи.',
    },
    pager:{
      currentPageInfo: '{0} из {1} страниц',
      totalItemsInfo: '({0} элементов)',
      firstPageTooltip: 'На первую страницу',
      lastPageTooltip: 'На последнюю страницу',
      nextPageTooltip: 'На следующую страницу',
      previousPageTooltip: 'Вернуться на последнюю страницу',
      nextPagerTooltip: 'К следующему пейджеру',
      previousPagerTooltip: 'На предыдущий пейджер',
      pagerDropDown:'Элементов на странице',
      pagerAllDropDown:'Элементов',
      All:'Все'
    }
  }
});

setCulture('ru-BY');

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  isAuth:boolean=false;

  constructor(
    private electronService: ElectronService,
    private translate: TranslateService,
    private esStorage:LocalStorageService
    ) {
      this.translate.setDefaultLang('en');
      console.log('AppConfig', AppConfig);

      if (electronService.isElectron) {
        console.log(process.env);
        console.log('Run in electron');
        console.log('Electron ipcRenderer', this.electronService.ipcRenderer);
        console.log('NodeJS childProcess', this.electronService.childProcess);
      } else {
        console.log('Run in browser');
      }
    }

    ngOnInit(){
      let isToken = this.esStorage.retrieve('token');
      !isToken ? this.isAuth = false : this.isAuth = true;
      this.esStorage.observe('token').subscribe( value => value === undefined ? this.isAuth = false : this.isAuth = true);
    }
}
