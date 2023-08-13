import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-users-list',
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.css'],
})
export class UsersListComponent implements OnInit {
  title: string = 'Users-list';

  constructor() {}

  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }
}
