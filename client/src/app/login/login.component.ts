import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../core';
import { finalize } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { LocalStorageService } from 'ngx-webstorage';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  busy = false;
  username = '';
  password = '';
  loginError = false;
  private subscription: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private esStorage:LocalStorageService
  ) {}

  ngOnInit(): void {
    this.subscription = this.authService.user$.subscribe((x) => {
      if (this.route.snapshot.url[0].path === 'login') {
        const token = this.esStorage.retrieve('token');
        const refreshToken = this.esStorage.retrieve('refreshToken');
        if (x && token && refreshToken) {
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '';
          this.router.navigate([returnUrl]);
        }
      } // optional touch-up: if a tab shows login page, then refresh the page to reduce duplicate login
    });
  }

  login() {
    if (!this.username || !this.password) {
      return;
    }
    this.busy = true;
    const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '';
    this.authService
      .login(this.username, this.password)
      .pipe(finalize(() => (this.busy = false)))
      .subscribe(
        () => {
          this.router.navigate([returnUrl]);
        },
        () => {
          this.loginError = true;
        }
      );
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  // username:string;
  // userpassword:string;

  // @ViewChild('toast') toast;
  // valueTime:any = 2000;
  // position = { X: 'Right' };

  // constructor(private router: Router, private apiservice:APIService, private esStorage:LocalStorageService) { }

  // routedashboard(board:any){
  //   this.router.navigate((["/"+board]));
  // }

  // async login(): Promise<void>{
  //   await this.apiservice.get(`/authorization?username=${this.username}&&userpassword=${this.userpassword}`).toPromise()
  //   .then((data:any)=>{
  //     if (data.token.length>0){
  //       this.esStorage.store('jwt', data.token);
  //       this.esStorage.store('refreshToken', data.refreshToken);
  //       this.esStorage.store('userid',data.userId);
  //       this.esStorage.store('board',data.board);
  //       this.esStorage.store('role', data.role);
  //       this.routedashboard(data.board);
  //     }else
  //     this.toast.show({title: 'Ошибка', content: data.data, timeOut: this.valueTime});
  //   })
  //   .catch(error=>this.toast.show({title: 'Ошибка', content: error, timeOut: this.valueTime}))
  // }

}
