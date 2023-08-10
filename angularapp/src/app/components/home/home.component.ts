import { Component } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { AccountService } from 'src/app/services/account.service';
import { ModalService } from 'src/app/services/modal.service';
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
