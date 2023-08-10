import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ChatService } from './services/signalr.service';
import { ChatMessage } from './models/chatMessage';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { LoginFormComponent } from './components/login-form/login-form.component';
import { ModalService } from './services/modal.service';
import { RegFormComponent } from './components/reg-form/reg-form.component';
import { AccountService } from './services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'real-time-chat';

  private currentModalRef: MatDialogRef<any> | undefined;

  constructor(
    private chatService: ChatService,
    private dialog: MatDialog,
    private modalService: ModalService,
    public accountService: AccountService
  ) {}

  ngOnInit(): void {
    if (this.accountService.userValue != null) return;

    this.openLoginDialog();

    this.modalService.openLogin$.subscribe(() => {
      this.openLoginDialog();
    });

    this.modalService.openSignUp$.subscribe(() => {
      this.openSignUpDialog();
    });
    //to have an ability to close modal windows
    this.modalService.closeModal$.subscribe(() => {
      this.closeModalWindow();
    });
  }

  openLoginDialog(): void {
    if (this.currentModalRef) {
      // Use beforeClosed() to detect when the previous modal is about to close
      this.currentModalRef.beforeClosed().subscribe(() => {
        this.openLogin();
      });

      // Close the previous modal
      this.currentModalRef.close();
    } else {
      // If there is no previous modal, open the new one directly
      this.openLogin();
    }
  }

  openLogin(): void {
    this.currentModalRef = this.dialog.open(LoginFormComponent, {
      disableClose: true,
    });

    this.currentModalRef.afterClosed().subscribe((result: any) => {
      // Handle the result here if needed (e.g., handle login success/failure)
      console.log('The login dialog was closed with result:', result);
    });
  }

  openSignUpDialog(): void {
    if (this.currentModalRef) {
      // Use beforeClosed() to detect when the previous modal is about to close
      this.currentModalRef.beforeClosed().subscribe(() => {
        this.openSignUp();
      });

      // Close the previous modal
      this.currentModalRef.close();
    } else {
      // If there is no previous modal, open the new one directly
      this.openSignUp();
    }
  }

  openSignUp(): void {
    this.currentModalRef = this.dialog.open(RegFormComponent, {
      disableClose: true,
    });

    this.currentModalRef.afterClosed().subscribe((result: any) => {
      // Handle the result here if needed (e.g., handle sign-up success/failure)
      console.log('The sign-up dialog was closed with result:', result);
    });
  }

  closeModalWindow(): void {
    if (this.currentModalRef) {
      this.currentModalRef.close();
    }
  }

  logout(): void {
    this.accountService.logout();
  }
}
