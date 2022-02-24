import { Injectable} from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, Subscription } from 'rxjs';
import { map, tap, delay, finalize } from 'rxjs/operators';
import { User } from '../../shared/model/user';
import { AppConfig } from '../../../environments/environment';
import { LocalStorageService } from 'ngx-webstorage';

interface AuthResult {
  token: string;
  refreshToken: string;
  success: boolean;
  isLife: boolean;
  role: string;
  errors: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = `${AppConfig.api_url}/authmanagment`;
  private _user = new BehaviorSubject<User>(null);
  user$: Observable<User> = this._user.asObservable();

  constructor(private router: Router, private http: HttpClient, private esStorage:LocalStorageService)
  { }

  login(username: string, password: string) {
    return this.http
      .post<AuthResult>(`${this.apiUrl}/login`, { username, password })
      .pipe(
        map((x) => {
          this._user.next({
            role: x.role
          });
          this.setLocalStorage(x);
          return x;
        })
      );
  }

  logout() {
    this.clearLocalStorage();
    this._user.next(null);
    this.router.navigate(['login']);
  }

  refreshToken() {
    const token = this.esStorage.retrieve('token');
    const refreshToken = this.esStorage.retrieve('refreshToken');
    if (!token) {
      this.clearLocalStorage();
      return of(null);
    }

    return this.http
      .post<AuthResult>(`${this.apiUrl}/refreshtoken`, { token, refreshToken })
      .pipe(
        map((x) => {
          this._user.next({
            role: x.role
          });

           !x.isLife&&!x.success ? (console.log(x.errors), this.logout()) : this.tokenRefresh(x);

          return x;
        })
      );
  }

  tokenRefresh(x: AuthResult)
  {
    !x.isLife&&x.success ? (this.setLocalStorage(x), console.log(x.errors)) : console.log(x.errors);
  }

  setLocalStorage(x: AuthResult) {
    this.esStorage.store('token', x.token);
    this.esStorage.store('refreshToken', x.refreshToken);
    this.esStorage.store('role',x.role);
  }

  clearLocalStorage() {
    this.esStorage.clear();
  }
}
