import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ChatService } from './services/signalr.service';
import { ChatMessage } from './models/chatMessage';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { LoginFormComponent } from './components/login-form/login-form.component';
import { RegFormComponent } from './components/reg-form/reg-form.component';
import { AccountService } from './services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'real-time-chat';

  constructor(
    private chatService: ChatService,
    public accountService: AccountService
  ) {}

  ngOnInit(): void {
    if (this.accountService.userValue != null) return;
  }
}
