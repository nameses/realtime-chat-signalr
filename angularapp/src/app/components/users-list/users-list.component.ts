//import { HttpClient } from '@angular/common/http';
//import { Component, OnInit } from '@angular/core';
//import { Router } from '@angular/router';
//import { UserConnection } from 'src/app/models/userConnection';
//import { AccountService } from 'src/app/services/account.service';
//import { environment } from 'src/environments/environment';

//@Component({
//  selector: 'app-users-list',
//  templateUrl: './users-list.component.html',
//  styleUrls: ['./users-list.component.css'],
//})
//export class UsersListComponent implements OnInit {
//  title: string = 'Users-list';
//  GET_URL: string = environment.apiUrl + '/user/get';
//  users: UserConnection[] = [];

//  constructor(
//    private http: HttpClient,
//    private router: Router,
//    public accountService: AccountService
//  ) {}

//  ngOnInit(): void {
//    this.http
//      .get<UserConnection[]>(this.GET_URL)
//      .subscribe((data) => (this.users = data));
//  }

//  public navToPrivateChat(user: UserConnection) {
//    this.router.navigate(['/private-chat'], {
//      queryParams: { username: user.username, connectionId: user.connectionId },
//    });
//    // private chat with user
//  }
//}
