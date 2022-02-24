import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { AppRoutingModule } from './app-routing.module';

//NgxWebStorage - storage service manager
import { NgxWebstorageModule } from 'ngx-webstorage';

// NG Translate
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

//App component
import { AppComponent } from './app.component';
import { MenuComponent } from './shared/components/menu/menu.component';

//Localce DateTime localization
import { ServiceWorkerModule } from '@angular/service-worker';
import { AppConfig } from '../environments/environment';


import { HomeModule } from './home/home.module';
import { LoginModule } from './login/login.module';

// AoT requires an exported function for factories
export function HttpLoaderFactory(http: HttpClient): TranslateHttpLoader {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

@NgModule({
  declarations: [
    AppComponent,
    MenuComponent],
  imports: [
    HomeModule,
    LoginModule,
    BrowserModule,
    FormsModule,
    HttpClientModule,
    CoreModule,
    SharedModule,
    AppRoutingModule,
    NgxWebstorageModule.forRoot(),
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    }),
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: AppConfig.production })
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
