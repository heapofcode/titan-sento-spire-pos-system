import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError} from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { AppConfig } from '../../../environments/environment';

const httpOptions = {
  headers: new HttpHeaders({ 'Content-Type': 'application/json' })
};

@Injectable({providedIn: 'root'})
export class HttpService  {

  constructor(private http: HttpClient) {  }

  // private handleError(error: HttpErrorResponse) {
  //   if (error.error instanceof ErrorEvent) {
  //     console.error('Произошла ошибка:', error.error.message);
  //   } else {
  //     console.error(
  //       `Код возврата серверной части ${error.status}, ` +
  //       `ошибка: ${error.message}`);
  //   }
  //   return throwError(
  //     'Сервер сейчас недоступен, повторите попытку позже.');
  // }

  get(apiurl:string):Observable<any> {
    return this.http.get(AppConfig.api_url+apiurl)
      .pipe(
        // retry(3),
        //catchError(this.handleError)
      )
  }

  put(apiurl:string, data:any=null ):Observable<any> {
    return this.http.put(AppConfig.api_url+apiurl, data, httpOptions)
      .pipe(
        // retry(3),
        //catchError(this.handleError)
      )
  }

  post(apiurl:string, data:any=null):Observable<any>
  {
    return this.http.post<any>(AppConfig.api_url+apiurl, data, httpOptions)
    .pipe(
      // retry(3),
      //catchError(this.handleError)
    )
  }

  delete(apiurl:string):Observable<any>
  {
    return this.http.delete<any>(AppConfig.api_url+apiurl, httpOptions)
    .pipe(
      // retry(3),
      //catchError(this.handleError)
    )
  }

}
