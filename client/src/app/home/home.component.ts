import { Component, OnInit } from '@angular/core';
// import { AuthService } from '../core';
import { LocalStorageService } from 'ngx-webstorage';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  role = '';

  constructor(
    // public authService: AuthService,
    private esStorage:LocalStorageService
    ) {}

  ngOnInit(): void {
    this.role = this.esStorage.retrieve('role');
  }
}
