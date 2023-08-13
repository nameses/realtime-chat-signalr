import { Component } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { AccountService } from 'src/app/services/account.service';
import { ChatService } from 'src/app/services/signalr.service';
import { LoginFormComponent } from '../login-form/login-form.component';
import { RegFormComponent } from '../reg-form/reg-form.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent {
  private currentModalRef: MatDialogRef<any> | undefined;

  constructor(
    private chatService: ChatService,
    public accountService: AccountService
  ) {}

  ngOnInit(): void {
    if (this.accountService.userValue != null) return;
  }

  logout(): void {
    this.accountService.logout();
  }
}
