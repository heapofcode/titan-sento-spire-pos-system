import { Component, OnInit } from '@angular/core';
import { HttpService, AuthService } from '../../../core';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent implements OnInit {
  checked = false;
  hamburgerChange = 'close';
  menu:any[];

  constructor(private _httpService:HttpService, public authService: AuthService)
  { }

  menuOpen(){
    this.checked = true;
    this.hamburgerChange = 'open';
    (document.querySelector('.brif-section') as HTMLElement).style.marginLeft = '170px';
  }
  menuClose(){
    this.checked = false;
    this.hamburgerChange = 'close';
    (document.querySelector('.brif-section') as HTMLElement).style.marginLeft = '';
  }
  changeChecked(){
    this.checked==true?this.menuClose():this.menuOpen();
  }

  logout(event:any){
    this.authService.logout();
    if (this.checked)
      this.menuClose();
  }

  async ngOnInit():Promise<void> {
      await this._httpService.get(`/user/menu`).toPromise()
      .then(data=>{
        if(data.success)
          this.menu = data.jsonMenu
        })
      .catch(error=>console.log(error))
  }
}
